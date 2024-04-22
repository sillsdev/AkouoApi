using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace AkouoApi.Services;
public class MovementMaps
{
    public Dictionary<Section, Section> BySection { get; set; } = new();
    public Dictionary<Section, List<Section>> ByMovement { get; set; } = new();
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
    //This gets the vernacular and notes and chapters
    protected IQueryable<PublishedAndReady> Ready(bool vernacularOnly, bool publishBeta, Bible? bible = null)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return _context.Mediafiles.Where(m => !m.Archived && m.ArtifactTypeId == null && m.ReadyToShare)
            .Join(_context.Passages.Where(p => p.Reference != null && (p.PassagetypeId == null || !vernacularOnly)).Include(p => p.SharedResource).ThenInclude(r => r.TitleMediafile), m => m.PassageId, p => p.Id, (m, p) => new { m, p })
            .Join(_context.Sections.Where(s => !s.Archived && (s.Published || s.Level ==1) && (publishBeta || s.PublishTo == "{\"Public\": \"true\"}")).Include(s => s.TitleMediafile), mp => mp.p.SectionId, s => s.Id, (mp, s) => new { mp.m, mp.p, s })
            .Join(_context.Plans, mps => mps.s.PlanId, pl => pl.Id, (mps, pl) => new { mps, pl })
            .Join(_context.Projects, mpspl => mpspl.pl.ProjectId, pr => pr.Id, (mpspl, pr) => new { mpspl.mps.m, mpspl.mps.p, mpspl.mps.s, pr })
            .Join(_context.OrganizationBibles, mpspr => mpspr.pr.OrganizationId, ob => ob.OrganizationId, (mpspr, ob) => new { mpspr, ob })
            .Join(_context.Bibles.Where(b => (bible == null || b.Id == bible.Id) && !b.Archived &&
                b.Iso != null && b.BibleId != null && b.BibleName != null).Include(b => b.BibleMediafile).Include(b => b.IsoMediafile),
                mpsprob => new { Id = mpsprob.ob.BibleId, Iso = mpsprob.mpspr.pr.Language }, b => new { b.Id, b.Iso }, (mpsprob, b) =>
             new PublishedAndReady(b, mpsprob.mpspr.pr, mpsprob.mpspr.s, mpsprob.mpspr.p, mpsprob.mpspr.m));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    protected IQueryable<PublishedAndReady> VernacularReady(bool publishBeta, Bible? bible = null)
    {
        return Ready(true, publishBeta, bible);
    }
    protected IEnumerable<Bible> ReadyBibles(bool publishBeta)
    {
        return VernacularReady(publishBeta).ToList().Select(r => r.Bible).Distinct(new RecordEqualityComparer<Bible>());
    }

    protected IEnumerable<Section> ReadyVernacularSections(Bible bible, bool publishBeta)
    {
        return VernacularReady(publishBeta, bible).ToList().Select(v => v.Section).Distinct(new RecordEqualityComparer<Section>());
    }
    protected IEnumerable<Passage> ReadyVernacularPassages(Bible bible, bool publishBeta)
    {
        return VernacularReady(publishBeta, bible).ToList().Select(v => v.Passage).Distinct(new RecordEqualityComparer<Passage>());
    }
    protected Sharedresource? GetNoteResource(Passage p)
    {
        int? id = p.SharedResourceId ?? _context.Sharedresources.Where(sr => sr.PassageId == p.Id).FirstOrDefault()?.Id;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return id != null
            ? _context.Sharedresources.Where(sr => sr.Id == id).Include(sr => sr.TitleMediafile).Include(sr => sr.ArtifactCategory).ThenInclude(ac => ac.TitleMediafile).FirstOrDefault()
            : null;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    protected IQueryable<Section> BibleSections(Bible bible, string? book=null)
    {
        return _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
            .Join(_context.Organizations, o => o.OrganizationId, org => org.Id, (o, org) => org)
            .Join(_context.Projects.Where(p => p.Language == bible.Iso), org => org.Id, p => p.OrganizationId, (org, p) => p)
            .Join(_context.Plans.Where(p => (book == null ||
                        _context.Passages.Where(p => p.Book == book)
                        .Join(_context.Sections.Where(s => !s.Archived), p => p.SectionId, s => s.Id, (p, s) => s.PlanId).Contains(p.Id))), p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
            .Join(_context.Sections.Where(s => !s.Archived), pl => pl.Id, s => s.PlanId, (pl, s) => s).Include(s => s.TitleMediafile);
    }
    protected MovementMaps MovementMap(Bible bible, string book)
    {
        IQueryable<Section> sections = BibleSections(bible, book)
            .OrderBy(s => s.Sequencenum); //if multiple plans...assume sequence numbers are unique

        MovementMaps movements = new ();
        Section? curMove = null;
        int moveCount = 0;
        foreach (Section s in sections)
        {
            if (s.Level == 2)
            {
                if (!movements.ByMovement.ContainsKey(s))
                {
                    s.State = (++moveCount).ToString();
                    movements.ByMovement.Add(s, new List<Section>());
                }
                curMove = s;
            } else
            {
                if (curMove != null)
                {
                    movements.BySection.Add(s, curMove);
                    movements.ByMovement [curMove].Add(s);
                }
            }   
        }
        return movements;
    }
    protected IEnumerable<Section> MovementsFromSections(IEnumerable<Section> sections, MovementMaps allmovements, int? justthismovement)
    {
        HashSet<Section> movements = new();
        foreach (Section s in sections)
        {
            allmovements.BySection.TryGetValue(s, out Section? movement);
            if (movement != null && (justthismovement == null || movement.Id == justthismovement))
                movements.Add(movement);
        }
        return movements;
    }
    protected IEnumerable<Section> AnyMovements(Bible bible)
    {
        return BibleSections(bible).Where(s => s.Level == 2);
    }
    protected IEnumerable<Section> ReadyMovements(Bible bible, string bookId, bool beta, int? justthismovement)
    {
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible, beta).Where(p => p.Book == bookId);
        IEnumerable<Section> sections = publishedpassages.Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>());
        MovementMaps allmovements = MovementMap(bible, bookId);
        return MovementsFromSections(sections, allmovements, justthismovement);
    }
    protected Audio? GetAudio(Mediafile? media)
    {
        return media == null || media.S3File == null
            ? null
            : new Audio(media,
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
                    images.Add(new Image(graphics.Id, 
                        size == "512" ? "Thumbnail" : "WEBP",
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

    protected Passagetype NoteType()
    {
        return _context.Passagetypes.Where(t => t.Abbrev == "NOTE").FirstOrDefault() ?? new Passagetype();
    }
    protected Passagetype ChapterType()
    {
        return _context.Passagetypes.Where(t => t.Abbrev == "CHNUM").FirstOrDefault() ?? new Passagetype();
    }
}
