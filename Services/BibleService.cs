using AkouoApi.Data;
using AkouoApi.Models;
using System.Diagnostics;

namespace AkouoApi.Services;

public class BibleService : BaseService
{
    public BibleService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
    }

    private List<BibleShort> ShortBibles(List<Bible> bibles)
    {
        List<BibleShort> sb = new();
        
        foreach(Bible b in  bibles)
        { 
            Audio? audio = GetAudio(b.BibleMediafile);
            Audio[] aa = audio != null ? new Audio[] { audio } : Array.Empty<Audio>();
            sb.Add(new BibleShort(b,aa));
        }
        return sb;
    }
    private List<BibleFull> FullBibles(List<Bible> bibles)
    {
        List<BibleFull> sb = new();

        foreach (Bible b in bibles)
        {
            Audio? audio = GetAudio(b.BibleMediafile);
            Audio[] aa = audio != null ? new Audio[] { audio } : Array.Empty<Audio>();
            sb.Add(new BibleFull(b, aa));
        }
        return sb;
    }

    public List<BibleShort> GetBibles(bool publishBeta)
    {
        return ShortBibles(ReadyBibles(publishBeta).ToList());
    }
    public List<BibleFull> GetBible(string bibleId, bool publishBeta)
    {
        return FullBibles(ReadyBibles(publishBeta).Where(o => o.BibleId == bibleId).ToList());
    }
    public List<BibleShort> GetBibleByIso(string iso, bool publishBeta)
    {
        return ShortBibles(ReadyBibles(publishBeta).Where(o => o.Iso == iso).ToList());
    }
    public List<OBTType> GetBibleOBTTypes(string bibleId, bool beta)
    {
        return GetBibleOBTTypes(_context.Bibles.Where(o => o.BibleId == bibleId).FirstOrDefault(), beta);
    }
    public List<OBTType> GetBibleOBTTypes(Bible? bible, bool publishBeta)
    {
        List<OBTType> obts = new();
        if (bible != null)
        {
            List<PublishedAndReady> all = Ready(false, publishBeta, bible).ToList();
            if (all.Any(all => all.Passage.PassagetypeId == null))
            {
                obts.Add(new OBTType(OBTTypeEnum.scripture));
                if (all.Any(all => (all.Passage.Book ?? "") != ""))
                {
                    obts.Add(new OBTType(OBTTypeEnum.book));
                }
            }
            int chapterid = ChapterType().Id;
            IEnumerable<Section> movements = AnyMovements(bible);
            if (movements.Any())
            {
                obts.Add(new OBTType(OBTTypeEnum.movement));
            }
            if (all.Any(all => all.Passage.PassagetypeId == chapterid))
            {
                obts.Add(new OBTType(OBTTypeEnum.chapter_number));
            }
            int noteid = NoteType().Id;

            IEnumerable<PublishedAndReady> notes = all.Where(all => all.Passage.PassagetypeId == noteid);
            int introcount = notes.Where(n => n.Section.Level < 3).Count();
            PublishedAndReady [] ordered = all.Where(n => n.Section.Level == 3).OrderBy(a => a.Section.Sequencenum).ThenBy(a => a.Passage.Sequencenum).ToArray();
            IEnumerable<PublishedAndReady> maybechapter = notes.Where(n => n.Section.Level == 3);
            foreach (PublishedAndReady note in maybechapter)
            {
                int ix = Array.IndexOf(ordered, note);
                while (ix > 0 && ordered [ix].Passage.PassagetypeId == noteid)
                {
                    ix--;
                }
                if (ordered [ix].Passage.PassagetypeId == chapterid)
                {
                    introcount++;
                }
            }
            if (introcount > 0)
            {
                obts.Add(new OBTType(OBTTypeEnum.introduction));
            }
            int appendix = 0; //TODO EH - check last section?
            if (appendix > 0)
            {
                obts.Add(new OBTType(OBTTypeEnum.appendix));
            }
            if (notes.Count() > introcount + appendix)
            {
                obts.Add(new OBTType(OBTTypeEnum.audio_note));
            }
        }
        int extra = 0; //TODO Where do we get this from?
        if (extra > 0)
        {
            obts.Add(new OBTType(OBTTypeEnum.extra));
        }

        return obts;
    }

    public List<NoteCategoryInfo> GetBibleNoteCategories(string bibleId, bool beta)
    {
        return GetBibleNoteCategories(_context.Bibles.Where(o => o.BibleId == bibleId).FirstOrDefault(), beta);
    }
    public List<NoteCategoryInfo> GetBibleNoteCategories(Bible? bible, bool publishBeta)
    {
        List<NoteCategoryInfo> cats = new();
        if (bible != null)
        {
            IEnumerable<PublishedAndReady> r = Ready(false, publishBeta, bible).ToList().Where(p => p.Passage.PassagetypeId == NoteType().Id).ToList();
            IEnumerable<Artifactcategory> acs = r.Select(p => GetNoteResource(p.Passage)?.ArtifactCategory).Select(a => a!).Distinct(new RecordEqualityComparer<Artifactcategory>());
            foreach (Artifactcategory ac in acs)
            {
                if (ac != null)
                cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
            }
        }
        return cats;
    }

}
