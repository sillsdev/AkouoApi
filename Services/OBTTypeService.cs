using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;

namespace AkouoApi.Services;

public class OBTTypeService : BaseService
{

    public OBTTypeService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
    }
    public List<OBTType> GetBibleOBTTypes(string bibleId)
    {
       return GetBibleOBTTypes(_context.Bibles.Where(o => o.BibleId == bibleId).FirstOrDefault()); 
    }
    public List<OBTType> GetBibleOBTTypes(Bible? bible)
    {
        List<OBTType> obts = new();
        if (bible != null)
        {
            List<PublishedAndReady> all = Ready(false, bible).ToList();
            Debug.WriteLine("OBTTypeService.GetBibleOBTTypes: " + all);
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
                obts.Add(new OBTType(OBTTypeEnum.movement, movements.Count()));
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
            foreach(PublishedAndReady note in maybechapter)
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
                obts.Add(new OBTType(OBTTypeEnum.introduction, introcount));
            }
            int appendix = 0; //TODO EH - check last section?
            if (appendix > 0)
            {
                obts.Add(new OBTType(OBTTypeEnum.appendix));
            }
            if (notes.Count() > introcount + appendix)
            {
                obts.Add(new OBTType(OBTTypeEnum.audio_note, notes.Count() - introcount - appendix));
            }
        }
        int extra = 0; //TODO Where do we get this from?
        if (extra > 0)
        {
            obts.Add(new OBTType(OBTTypeEnum.extra));
        }
        
        return obts;
    }

    public List<OBTType> GetBibleNoteCategories(string bibleId)
    {
        return GetBibleNoteCategories(_context.Bibles.Where(o => o.BibleId == bibleId).FirstOrDefault());
    }
    public List<OBTType> GetBibleNoteCategories(Bible? bible)
    {
        List<NoteCategoryInfo> cats = new();
        if (bible != null)
        {
            IEnumerable<Artifactcategory> acs = Ready(false, bible).Where(p => p.Passage.PassagetypeId == NoteType().Id).ToList().Select(p => GetNoteResource(p.Passage)?.ArtifactCategory).Select(a => a!).Distinct(new RecordEqualityComparer<Artifactcategory>());
            foreach (Artifactcategory ac in acs)
            {
                cats.Add(new NoteCategoryInfo(ac));
            }
        }
        return cats;
    }

}
