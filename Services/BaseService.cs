using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace AkouoApi.Services;

public class MovementMaps
{
    public Dictionary<Section, Section> BySection { get; set; } = [];
    public Dictionary<Section, List<Section>> ByMovement { get; set; } = [];
}
public class BaseService(ILogger<LanguageService> logger,
                       AppDbContext context,
                       IS3Service s3Service,
                       MediafileService mediafileService)
{
    protected readonly ILogger<LanguageService> _logger = logger;
    protected readonly AppDbContext _context = context;
    protected readonly IS3Service _s3Service = s3Service;
    protected readonly MediafileService _mediafileService = mediafileService;
    protected readonly ConcurrentDictionary<int,IEnumerable<Published>> _cacheVernacularReady = new();
    protected const string NOTE = "NOTE";
    protected const string CHAPTER = "CHNUM";

    //This gets the vernacular and notes and chapters

    protected IQueryable<Published> Ready(bool vernacularOnly, bool publishBeta, int? bid = null, string? book = null)
    {
        return
             (IQueryable<Published>)_context.Published
             .Where(s => (s.Ispublic || (publishBeta && s.Isbeta)) &&
                         (bid == null || s.Bid == bid) &&
                         (!vernacularOnly || s.Passagetype == null) &&
                         (book == null || s.Book == book))
                 .Include(s => s.Section)
                 .Include(s => s.Mediafile)
                 .Include(s => s.Sharedresource).ThenInclude(r => r!.ArtifactCategory)
                 .Include(s => s.Sharedresource).ThenInclude(r => r!.TitleMediafile);
    }
    protected IQueryable<Published> HelpsReady(bool vernacularOnly, int? bid = null, string? book = null)
    {
        return
            (IQueryable<Published>)_context.Published
            .Where(s => (bid == null || s.Bid == bid) &&
                        (!vernacularOnly || s.Passagetype == null) &&
                        (book == null || s.Book == book) && s.Isobthelps)
                .Include(s => s.Section)
                .Include(s => s.Mediafile)
                .Include(s => s.Sharedresource).ThenInclude(r => r!.ArtifactCategory)
                .Include(s => s.Sharedresource).ThenInclude(r => r!.TitleMediafile)


        ;
    }
    protected IQueryable<Bible> ReadyBibles(bool publishBeta, string? bibleId = null)
    {
        return _context.Publishedbibles
                    .Where(s => ((publishBeta && s.HasBeta) || s.HasPublic) &&
                           (bibleId == null || s.BibleId == bibleId))
                    .Include(s => s.Isomediafile)
                    .Include(s => s.Biblemediafile)
                    .Select(s => new Bible(s.Id, s.BibleId, s.Iso, s.Biblename, s.Description, s.Group1, s.Group2, s.Publishingdata, s.Isomediafile, s.Biblemediafile));
    }
    protected IQueryable<Bible> HelpsReadyBibles(string? bibleId = null)
    {
        IQueryable<Bible> x = _context.Publishedbibles
                    .Where(s => (bibleId == null || s.BibleId == bibleId) && s.HasObtHelps)
                    .Include(s => s.Isomediafile)
                    .Include(s => s.Biblemediafile)
                    .Select(s => new Bible(s.Id, s.BibleId, s.Iso, s.Biblename, s.Description, s.Group1, s.Group2, s.Publishingdata, s.Isomediafile, s.Biblemediafile));
        return x;
    }

    protected IEnumerable<Section> ReadyVernacularSections(Bible bible, bool publishBeta)
    {
        return [.. _context.Published
            .Where(s => (s.Ispublic || (publishBeta && s.Isbeta)) &&
                   s.Bid == bible.Id &&
                   //s.Mediafileid != null &&
                   s.Passagetype == null)
            .Include(s => s.Titlemediafile)
            .Select(s => new Section(s.Sectionid, s.Sectionsequence, s.Sectiontitle, s.Planid, s.Level, s.Titlemediafile))
            .Distinct()];

    }
    protected IEnumerable<Passage> ReadyVernacularPassages(Bible bible, bool publishBeta, string? book)
    {
        return [.. _context.Published
            .Where(s => (s.Ispublic || (publishBeta && s.Isbeta)) &&
                        (book == null || s.Book == book) &&
                        (s.Bid == bible.Id) &&
                        //s.Mediafileid != null &&
                        s.Passagetype == null)
            .Include(s => s.Section)
            .Include(s => s.Sharedresource).ThenInclude(r => r!.TitleMediafile)
            .Select(s => new Passage(s.Passageid, s.Sequencenum, s.Book, s.Reference, s.Sectionid, s.Sharedresource, s.Title, s.Startchapter, s.Startverse, s.Endchapter, s.Endverse, s.Passagetype))
            .Distinct()];

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
    protected List<Section> BibleSections(Bible bible, string? book = null)
    {
        return [.. _context.Published
                .Where(s => s.Bid == bible.Id && (book == null || s.Book == book))
                .Select(s => new Section(s.Sectionid, s.Sectionsequence, s.Sectiontitle, s.Planid, s.Level, s.Titlemediafile))
                .Distinct()];
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
        return ready.Any(r => r.Movementid != null);
    }
    protected List<MovementShort> ReadyMovements(List<Published> ready, int? movementId = null)
    {
        List<MovementShort> ret = [];
        IEnumerable<int?> movementids = ready.Where(p => p.Movementid is not null && (movementId == null || p.Movementid == movementId)).Select(r => r.Movementid).Distinct();
        if (!movementids.Any())
        {
            List<SectionShort> sections = [];
            IOrderedEnumerable<Section> readySections = ready.Where(r => r.Movementid is null && r.Level == SectionLevel.Section)
                .Select(r => new Section(r)).Distinct(new RecordEqualityComparer<Section>())
                .OrderBy(s => s.Sequencenum);
            readySections.ToList().ForEach(s => sections.Add(new SectionShort(s)));
            ret.Add(new MovementShort(null, [.. sections]));
        }
        else
        {
            List<Section> movements = [.. _context.Sections.Where(s => movementids.Contains(s.Id)).OrderBy(s => s.Sequencenum)];
            for (int ix = 0; ix < movements.Count; ix++)
            {
                Section m = movements[ix];
                List<SectionShort> sections = [];
                m.State = ix.ToString();
                IOrderedEnumerable<Section> readySections  = ready.Where(r => r.Movementid == m.Id && r.Level == SectionLevel.Section)
                .Select(r => new Section(r)).Distinct(new RecordEqualityComparer<Section>())
                .OrderBy(s => s.Sequencenum);
                readySections.ToList().ForEach(s => sections.Add(new SectionShort(s)));
                ret.Add(new MovementShort(m, [.. sections]));
            }
        }
        return ret;
    }
    protected Dictionary<MovementShort, IOrderedEnumerable<Section>> MovementSections(List<MovementShort> movements, List<Published> ready)
    {
        Dictionary<MovementShort, IOrderedEnumerable<Section>> allmovements = [];
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
    protected Image [] GraphicInfo(string? imageInfo, int? imageid, DateTime? imagedate)
    {
        List<Image> images = [];
        if (imageid != null)
        {
            string[] sizes = ["512", "1024"];
            foreach (string size in sizes)
            {
                JObject info = JObject.Parse(imageInfo ?? "{}");
                JToken? graphic =  info [size];
                string url = graphic? ["content"]?.Value<string>() ?? "";
                if (graphic != null)
                {
                    images.Add(new Image(imageid ?? 0, //we already checked
                        size,
                        imagedate ?? new DateTime(),
                        url.Split('/').Last(), url)
                    );
                }
            }
        }
        return [.. images];
    }
    protected Image [] PassageGraphic(Published noteOrChapter)
    {
        return GraphicInfo(noteOrChapter.Passageimage, noteOrChapter.Passageimageid, noteOrChapter.Passageimagedate);
    }
    protected Image [] GetGraphicImages(int resourceid, string resourcetype)
    {
        //Debug.WriteLine($"GGG resourceid {resourceid} type {resourcetype}");
        Graphic? graphics =  _context.Graphics.Where(g => g.ResourceId == resourceid && g.ResourceType == resourcetype).FirstOrDefault();
        if (graphics != null)
        {
            return GraphicInfo(graphics.Info, graphics.Id, graphics.DateUpdated);
        }
        ;
        return [];
    }
    protected static string? GetDefault(string? defaultParams, string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return null;
        }

        JObject tmp = JObject.Parse(defaultParams ?? "{}");
        return !tmp.TryGetValue(label, out JToken? value) || value == null
            ? null
            : value switch
            {
                JObject or JArray => value.ToString(),
                JValue jValue => jValue.Value?.ToString(),
                _ => value.ToString()
            };
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
