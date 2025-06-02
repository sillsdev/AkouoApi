using AkouoApi.Data;
using AkouoApi.Models;

namespace AkouoApi.Services;

public class BibleService(ILogger<LanguageService> logger,
                       AppDbContext context,
                       IS3Service s3Service,
                       MediafileService mediafileService) : BaseService(logger, context, s3Service, mediafileService)
{
    private List<BibleShort> ShortBibles(List<Bible> bibles)
    {
        List<BibleShort> sb = [];

        foreach (Bible b in bibles)
        {
            Audio? audio = GetAudio(b.BibleMediafile);
            Audio[] aa = audio != null ? [audio] : [];
            sb.Add(new BibleShort(b, aa));
        }
        return sb;
    }
    private List<BibleFull> FullBibles(List<Bible> bibles)
    {
        List<BibleFull> sb = [];

        foreach (Bible b in bibles)
        {
            Audio? audio = GetAudio(b.BibleMediafile);
            Audio[] aa = audio != null ? [audio] : [];
            sb.Add(new BibleFull(b, aa));
        }
        return sb;
    }

    public List<BibleShort> GetBibles(bool publishBeta)
    {
        return ShortBibles([.. ReadyBibles(publishBeta)]);
    }
    public List<BibleShort> GetHelpsBibles()
    {
        return ShortBibles([.. HelpsReadyBibles()]);
    }
    public List<BibleFull> GetBible(string bibleId, bool publishBeta)
    {
        return FullBibles([.. ReadyBibles(publishBeta, bibleId)]);
    }
    public List<BibleFull> GetHelpsBible(string bibleId)
    {
        return FullBibles([.. HelpsReadyBibles(bibleId)]);
    }
    public List<BibleShort> GetBibleByIso(string iso, bool publishBeta)
    {
        return ShortBibles(ReadyBibles(publishBeta).ToList().Where(o => o.Iso == iso).ToList());
    }
    public List<BibleShort> GetHelpsBibleByIso(string iso)
    {
        return ShortBibles(HelpsReadyBibles().ToList().Where(o => o.Iso == iso).ToList());
    }

    private List<OBTType> GetOBTTypes(List<Published> all, List<Published> extra)
    {
        List<OBTType> obts = [];
        if (all.Any(all => all.Passagetype == null))
        {
            obts.Add(new OBTType(OBTTypeEnum.scripture));
            if (all.Any(all => (all.Book ?? "") != ""))
            {
                obts.Add(new OBTType(OBTTypeEnum.book));
            }
        }
        int chapterid = ChapterType().Id;
        if (AnyMovements(all))
        {
            obts.Add(new OBTType(OBTTypeEnum.movement));
        }
        if (all.Any(all => all.Passagetype == CHAPTER))
        {
            obts.Add(new OBTType(OBTTypeEnum.chapter));
        }
        int noteid = NoteType().Id;

        IEnumerable<Published> notes = all.Where(all => all.Passagetype == NOTE);
        int introcount = notes.Where(n => n.Level < SectionLevel.Section).Count();
        Published [] ordered = [.. all.Where(n => n.Level == SectionLevel.Section).OrderBy(a => a.Sectionsequence).ThenBy(a => a.Sequencenum)];
        IEnumerable<Published> maybechapter = notes.Where(n => n.Level == SectionLevel.Section);
        foreach (Published note in maybechapter)
        {
            int ix = Array.IndexOf(ordered, note);
            while (ix > 0 && ordered [ix].Passagetype == NOTE)
            {
                ix--;
            }
            if (ordered [ix].Passagetype == CHAPTER)
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
        if (extra.Count != 0)
        {
            obts.Add(new OBTType(OBTTypeEnum.extra));
        }
        if (all.Where(a => a.Altbookmediafileid is not null || a.Bookmediafileid is not null || a.Titlemediafileid is not null).Any() ||
            extra.Where(e => e.Altbookmediafileid is not null || e.Bookmediafileid is not null || e.Titlemediafile is not null).Any())
            obts.Add(new OBTType(OBTTypeEnum.title));

        obts.Sort();
        return obts;
    }


    public List<OBTType> GetBibleOBTTypes(string bibleId, bool beta)
    {
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw (new Exception("Bible not found"));

        List<Published> all = [.. Ready(true, false, beta, bible.Id)];
        List<Published> extra = [.. Ready(false, false, beta, bible.Id)];
        return GetOBTTypes(all, extra);
    }

    public List<OBTType> GetHelpsOBTTypes(string bibleId)
    {
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw (new Exception("Bible not found"));
        List<Published> all = [.. HelpsReady(true, false, bible.Id)];
        List<Published> extra = [.. HelpsReady(false, false, bible.Id)];
        return GetOBTTypes(all, extra);
    }
    private List<NoteCategoryInfo> GetNoteCategories(int orgId, IEnumerable<Published> scripture, IEnumerable<Published> general)
    {
        List<NoteCategoryInfo> cats = [];
        IEnumerable<Artifactcategory> acs = scripture.Select(p => p.Sharedresource?.ArtifactCategory).Select(a => a!).Distinct(new RecordEqualityComparer<Artifactcategory>());
        foreach (Artifactcategory ac in acs)
        {
            if (ac != null)
                cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
        }
        //any just used in general projects?
        acs = general.Select(p => p.Sharedresource?.ArtifactCategory).Select(a => a!).Distinct(new RecordEqualityComparer<Artifactcategory>());
        foreach (Artifactcategory ac in acs)
        {
            if (ac != null && !cats.Any(c => c.Id == ac.Id))
                cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
        }
        //get the special ones
        _context.Artifactcategorys.Where(a => a.OrganizationId == orgId && (a.Specialuse ?? "") != "").ToList().ForEach(ac => {
            cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
        });

        cats.Sort();
        return cats;
    }

    public List<NoteCategoryInfo> GetBibleNoteCategories(string bibleId, bool beta)
    {
        return GetBibleNoteCategories(_context.Vwpublishedbibles.Where(o => o.BibleId == bibleId).FirstOrDefault(), beta);
    }
    public List<NoteCategoryInfo> GetBibleNoteCategories(PublishedBible? bible, bool publishBeta)
    {
        if (bible != null)
        {
            IEnumerable<Published> scripture = Ready(true,false, publishBeta, bible.Id).ToList().Where(p => p.Passagetype == NOTE).ToList();
            IEnumerable<Published> general = Ready(false,false, publishBeta, bible.Id).ToList().Where(p => p.Passagetype == NOTE).ToList();
            return GetNoteCategories(bible.Organizationid, scripture, general);
        }
        else
            throw (new Exception("Bible not found"));
    }
    public List<NoteCategoryInfo> GetHelpsNoteCategories(string bibleId)
    {
        return GetHelpsNoteCategories(_context.Vwobthelpsbibles.Where(o => o.BibleId == bibleId).FirstOrDefault());
    }
    public List<NoteCategoryInfo> GetHelpsNoteCategories(PublishedBible? bible)
    {
        if (bible != null)
        {
            IEnumerable<Published> all = HelpsReady(true, false, bible.Id).ToList().Where(p => p.Passagetype == NOTE).ToList();
            return GetNoteCategories(bible.Organizationid, all, []);
        }
        else
            throw (new Exception("Bible not found"));
    }
}
