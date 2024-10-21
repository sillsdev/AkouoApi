using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

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
    protected readonly ConcurrentDictionary<int,IEnumerable<Published>> _cacheVernacularReady;
    protected const string NOTE = "NOTE";
    protected const string CHAPTER = "CHNUM"; 
    
    public BaseService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService)
    {
        _logger = logger;
        _context = context;
        _s3Service = s3Service;
        _mediafileService = mediafileService;
        _cacheVernacularReady = new();
    }
    //This gets the vernacular and notes and chapters
    
    protected IQueryable<Published> Ready(bool scripture, bool vernacularOnly, bool publishBeta, int? bid = null, string? book = null)
    {
        return scripture ? 
            _context.Vwpublishedscripture
            .Where(s => (s.IsPublic || publishBeta) &&
                        (bid == null || s.Bid == bid) &&
                        (!vernacularOnly || s.Passagetype == null) &&
                        (book == null || s.Book == book))
                .Include(s => s.Section)
                .Include(s => s.Mediafile)
                .Include(s => s.Sharedresource)
                        .ThenInclude(r => r!.ArtifactCategory)
                .Include(s => s.Sharedresource)
                        .ThenInclude(r => r!.TitleMediafile)
            : _context.Vwpublishedgeneral
            .Where(s => (s.IsPublic || publishBeta) &&
                        (bid == null || s.Bid == bid) &&
                        (!vernacularOnly || s.Passagetype == null) &&
                        (book == null || s.Book == book))
                .Include(s => s.Mediafile)
                .Include(s => s.Sharedresource).ThenInclude(r => r!.ArtifactCategory)
                .Include(s => s.Sharedresource).ThenInclude(r => r!.TitleMediafile)
        ;
    }
    protected IQueryable<Published> HelpsReady(bool scripture, bool vernacularOnly, int? bid = null, string? book = null)
    {
        return scripture ?
            _context.Vwobthelpsscripture
            .Where(s => (bid == null || s.Bid == bid) &&
                        (!vernacularOnly || s.Passagetype == null) &&
                        (book == null || s.Book == book))
                .Include(s => s.Mediafile)
                .Include(s => s.Sharedresource).ThenInclude(r => r!.ArtifactCategory)
            : _context.Vwobthelpsgeneral
            .Where(s => (bid == null || s.Bid == bid) &&
                        (!vernacularOnly || s.Passagetype == null) &&
                        (book == null || s.Book == book))
                .Include(s => s.Mediafile)
                .Include(s => s.Sharedresource).ThenInclude(r => r!.ArtifactCategory);
        ;
    }
    protected IQueryable<Bible> ReadyBibles(bool publishBeta, string? bibleId=null)
    {
        return _context.Vwpublishedbibles
                    .Where(s => (publishBeta || s.HasPublic) && 
                           (bibleId == null || s.BibleId == bibleId))
                    .Include(s => s.Isomediafile)
                    .Include(s => s.Biblemediafile)
                    .Select(s => new Bible(s.Id, s.BibleId, s.Iso, s.Biblename, s.Description, s.Publishingdata, s.Isomediafile, s.Biblemediafile))
                    ;
    }
    protected IQueryable<Bible> HelpsReadyBibles(string? bibleId = null)
    {
        var x = _context.Vwobthelpsbibles
                    .Where(s => bibleId == null || s.BibleId == bibleId)
                    .Include(s => s.Isomediafile)
                    .Include(s => s.Biblemediafile)
                    .Select(s => new Bible(s.Id, s.BibleId, s.Iso, s.Biblename, s.Description, s.Publishingdata, s.Isomediafile, s.Biblemediafile));
        return x;
    }

    protected IEnumerable<Section> ReadyVernacularSections(Bible bible, bool publishBeta)
    {
        return _context.Vwpublishedscripture
            .Where(s => (s.IsPublic || publishBeta) &&
                   s.Bid == bible.Id && 
                   s.Mediafileid != null &&
                   s.Passagetype == null) 
            .Include(s => s.Titlemediafile)
            .Select(s => new Section(s.Sectionid, s.Sectionsequence, s.Sectiontitle,s.Planid, s.Level, s.Titlemediafile))
            .Distinct().ToList();

    }
    protected IEnumerable<Passage> ReadyVernacularPassages(Bible bible, bool publishBeta, string? book)
    {
        return _context.Vwpublishedscripture
            .Where(s => (publishBeta || s.IsPublic) &&
                        (book == null || s.Book == book) &&
                        (s.Bid == bible.Id) &&
                        s.Mediafileid != null &&
                        s.Passagetype == null)
            .Include(s => s.Section)
            .Include(s => s.Sharedresource).ThenInclude(r => r!.TitleMediafile)
            .Select(s => new Passage(s.Passageid, s.Sequencenum, s.Book, s.Reference, s.Sectionid, s.Sharedresource, s.Title, s.Startchapter, s.Startverse, s.Endchapter, s.Endverse, s.Passagetype))
            .Distinct().ToList();

    }
    protected Sharedresource? GetNoteResource(int? sharedresourceid, int passageid)
    {
        int? id = sharedresourceid ?? _context.Sharedresources.Where(sr => sr.PassageId == passageid).FirstOrDefault()?.Id;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        return id != null
            ? _context.Sharedresources.Where(sr => sr.Id == id).Include(sr => sr.TitleMediafile).Include(sr => sr.ArtifactCategory).ThenInclude(ac => ac.TitleMediafile).FirstOrDefault()
            : null;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    protected List<Section> BibleSections(Bible bible, string? book=null)
    {
        return _context.Vwpublishedscripture
                .Where(s => s.Bid == bible.Id && (book == null || s.Book == book))
                .Select(s => new Section(s.Sectionid, s.Sectionsequence, s.Sectiontitle, s.Planid, s.Level, s.Titlemediafile))
                .Distinct().ToList();
        //Does the above do the book correctly??
        //return _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
        //    .Join(_context.Organizations, o => o.OrganizationId, org => org.Id, (o, org) => org)
        //   .Join(_context.Projects.Where(p => p.Language == bible.Iso), org => org.Id, p => p.OrganizationId, (org, p) => p)
        //    .Join(_context.Plans.Where(p => (book == null ||
        //                _context.Passages.Where(p => p.Book == book && !p.Archived)
        //                .Join(_context.Sections, p => p.SectionId, s => s.Id, (p, s) => s.PlanId).Contains(p.Id))), p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
        //    .Join(_context.Sections.Where(s => !s.Archived), pl => pl.Id, s => s.PlanId, (pl, s) => s).Include(s => s.TitleMediafile);
    }
    
    protected bool AnyMovements(List<Published> ready)
    {
        return ready.Any(r=> r.Movementid != null);
    }
    protected List<MovementShort> ReadyMovements(List<Published> ready, int? movementId=null)
    {
        List<MovementShort> ret = new();
        IEnumerable<int?> movementids = ready.Where(p => (movementId == null || p.Movementid == movementId)).Select(r => r.Movementid).Distinct();
        if (!movementids.Any())
        {
            List<SectionShort> sections = new ();
            IOrderedEnumerable<Section> readySections = ready.Where(r => r.Movementid is null && r.Level == SectionLevel.Section)
                .Select(r => new Section(r)).Distinct(new RecordEqualityComparer<Section>())
                .OrderBy(s => s.Sequencenum);
            readySections.ToList().ForEach(s => sections.Add(new SectionShort(s)));
            ret.Add(new MovementShort(null, sections.ToArray()));
        }
        else
        {
            List<Section> movements = _context.Sections.Where(s => movementids.Contains(s.Id)).OrderBy(s => s.Sequencenum).ToList();
            for (int ix = 0; ix < movements.Count(); ix++)
            {
                Section m = movements[ix];
                List<SectionShort> sections = new ();
                m.State = ix.ToString();
                IOrderedEnumerable<Section> readySections  = ready.Where(r => r.Movementid == m.Id && r.Level == SectionLevel.Section)
                .Select(r => new Section(r)).Distinct(new RecordEqualityComparer<Section>())
                .OrderBy(s => s.Sequencenum);
                readySections.ToList().ForEach(s => sections.Add(new SectionShort(s)));
                ret.Add(new MovementShort(m, sections.ToArray()));
            }
        }
        return ret;
    }
    protected Dictionary<MovementShort, IOrderedEnumerable<Section>> MovementSections(List<MovementShort> movements, List<Published> ready)
    {
        Dictionary<MovementShort, IOrderedEnumerable<Section>> allmovements = new();
        movements.ForEach(m => {
            allmovements.Add(m, ready.Where(r => r.Movementid == m.Id && r.Level == SectionLevel.Section)
                .Select(r => new Section(r)).Distinct(new RecordEqualityComparer<Section>())
                .OrderBy(s => s.Sequencenum));
        });
        return allmovements;
    }
    protected Audio? GetAudio(Mediafile? media)
    {
        return media == null || media.PublishedAs == null
            ? null
            : new Audio(media,
                  _s3Service.ObjectUrl(media.PublishedAs));
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
        return _context.Passagetypes.Where(t => t.Abbrev == NOTE).FirstOrDefault() ?? new Passagetype();
    }
    protected Passagetype ChapterType()
    {
        return _context.Passagetypes.Where(t => t.Abbrev == CHAPTER).FirstOrDefault() ?? new Passagetype();
    }
}
