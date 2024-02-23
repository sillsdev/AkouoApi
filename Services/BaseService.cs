using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;

namespace AkouoApi.Services;
public class PublishedAndReady
{
    public PublishedAndReady(Bible bible, Project project, Section section, Passage passage, Mediafile mediafile)
    {
        Bible = bible;
        Project = project;
        Section = section;
        Passage = passage;
        Mediafile = mediafile;
    }
    public Bible Bible { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Passage Passage { get; set; } = null!;
    public Mediafile Mediafile { get; set; } = null!;
}
public class BaseService
{
    protected readonly ILogger<LanguageService> _logger;
    protected readonly AppDbContext _context;
    protected readonly IS3Service _s3Service;
    protected readonly MediafileService _mediafileService;
    public BaseService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService)
    {
        _logger = logger;
        _context = context;
        _s3Service = s3Service;
        _mediafileService = mediafileService;
    }
    //This gets the vernacular and notes and chapter
    protected IQueryable<PublishedAndReady> Ready(bool vernacularOnly, Bible? bible = null)
    {
        return _context.Mediafiles.Where(m => !m.Archived && m.ArtifactTypeId == null && m.ReadyToShare)
            .Join(_context.Passages.Where(p => p.Reference != null && (p.PassagetypeId == null || !vernacularOnly)), m => m.PassageId, p => p.Id, (m, p) => new { m, p })
            .Join(_context.Sections.Where(s => !s.Archived && s.Published).Include(s => s.TitleMediafile), mp => mp.p.SectionId, s => s.Id, (mp, s) => new { mp.m, mp.p, s })
            .Join(_context.Plans, mps => mps.s.PlanId, pl => pl.Id, (mps, pl) => new { mps, pl })
            .Join(_context.Projects, mpspl => mpspl.pl.ProjectId, pr => pr.Id, (mpspl, pr) => new { mpspl.mps.m, mpspl.mps.p, mpspl.mps.s, pr })
            .Join(_context.OrganizationBibles, mpspr => mpspr.pr.OrganizationId, ob => ob.OrganizationId, (mpspr, ob) => new { mpspr, ob })
            .Join(_context.Bibles.Where(b => (bible == null || b.Id == bible.Id) && !b.Archived &&
                b.Iso != null && b.BibleId != null && b.BibleName != null).Include(b => b.BibleMediafile).Include(b => b.IsoMediafile),
                mpsprob => new { Id = mpsprob.ob.BibleId, Iso = mpsprob.mpspr.pr.Language }, b => new { b.Id, b.Iso }, (mpsprob, b) => 
             new PublishedAndReady(b, mpsprob.mpspr.pr, mpsprob.mpspr.s, mpsprob.mpspr.p, mpsprob.mpspr.m));
    }
    protected IQueryable<PublishedAndReady> VernacularReady(Bible? bible = null)
    {
        return Ready(true, bible);
    }
    protected IEnumerable<Bible> ReadyBibles()
    {
        return VernacularReady().ToList().Select(r => r.Bible).Distinct(new RecordEqualityComparer<Bible>());
    }

    protected IEnumerable<Section> ReadyVernacularSections(Bible bible)
    {
        return VernacularReady(bible).ToList().Select(v => v.Section).Distinct(new RecordEqualityComparer<Section>());
    }
    protected IEnumerable<Passage> ReadyVernacularPassages(Bible bible)
    {
        return VernacularReady(bible).ToList().Select(v => v.Passage).Distinct(new RecordEqualityComparer<Passage>());
    }
    protected Dictionary<decimal, Section> MovementMap(Bible bible)
    {
        //do I need to just get scripture type plans?
        IQueryable<Section> sections =
            _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
            .Join(_context.Organizations, o => o.OrganizationId, org => org.Id, (o, org) => org)
            .Join(_context.Projects.Where(p => p.Language == bible.Iso), org => org.Id, p => p.OrganizationId, (org, p) => p)
            .Join(_context.Plans, p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
            .Join(_context.Sections.Where(s => !s.Archived), pl => pl.Id, s => s.PlanId, (pl, s) => s).Include(s => s.TitleMediafile)
            .OrderBy(s => s.Sequencenum); //if multiple plans...assume sequence numbers are unique

        Dictionary<decimal,Section> movements = new ();
        Section? curMove = null;
        foreach (Section s in sections)
        {
            if (s.Level == 2)
            {
                curMove = s;
            } else
            {
                if (curMove != null)
                    movements.Add(s.Sequencenum, curMove);
            }   
        }
        return movements;
    }
    protected IEnumerable<Section> ReadyMovements(Bible bible, string?bookId = null)
    {
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible);
        if (bookId != null)
            publishedpassages = publishedpassages.Where(p => p.Book == bookId);
        IEnumerable<decimal> sections = publishedpassages.Select(p => p.Section?.Sequencenum ?? 0).Distinct();
        Dictionary<decimal, Section> allmovements = MovementMap(bible);
        HashSet<Section> movements = new();
        foreach (decimal s in sections)
        {
            Section? movement = allmovements[s];
            if (movement != null)
                movements.Add(movement);
        }
        return movements;
    }
    protected Audio? GetAudio(Mediafile? media)
    {
        return media == null || media.S3File == null
            ? null
            : new Audio(media.ContentType ?? "",
                  media.DateUpdated,
                  media.S3File,
                  _s3Service.ObjectUrl(media.S3File,
                                      _mediafileService.DirectoryName(media)));
    }
    protected Image[] GetGraphicImages(int resourceid, string resourcetype)
    {
        string[] sizes = { "512", "1024" };
        List<Image> images = new ();
        List<Graphic> graphics = _context.Graphics.Where(g => g.ResourceId == resourceid && g.ResourceType == resourcetype).ToList();
        graphics.ForEach(graphics => {
            JObject info = JObject.Parse(graphics.Info ?? "{}");
            foreach (string size in sizes)
            {
                JToken? graphic = info [size];
                string url = graphic? ["content"]?.Value<string>() ?? "";
                if (graphic != null)
                {
                    images.Add(new Image(size == "512" ? "Thumbnail" : "WEBP",
                        graphics.DateUpdated??new DateTime(),
                        url.Split('/').Last(), url)
                    );
                }
            };
        });
        return images.ToArray();
    }   
    protected static string? GetDefault(string? defaultParams, string label)
    {
        dynamic tmp = JObject.Parse(defaultParams ?? "{}");
        return tmp.Value<string>(label);
    }
}
