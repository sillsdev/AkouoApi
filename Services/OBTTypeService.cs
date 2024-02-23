using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
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
            if (all.Any(all => all.Passage.PassagetypeId == null))
            {
                obts.Add(new OBTType(OBTTypeEnum.scripture));
                if (all.Any(all => (all.Passage.Book ?? "") != ""))
                {
                    obts.Add(new OBTType(OBTTypeEnum.book));
                }
            }
            IEnumerable<string> movements = ReadyMovements(bible).Select(s => s.Name);
            if (movements.Any())
            {
                obts.Add(new OBTType(OBTTypeEnum.movement, movements.Count()));
            }
            Passagetype? chnumtype = _context.Passagetypes.Where(t => t.Abbrev == "CHNUM").FirstOrDefault();
            if (chnumtype != null && all.Any(all => all.Passage.PassagetypeId == chnumtype.Id))
            {
                obts.Add(new OBTType(OBTTypeEnum.chapter_number));
            }
            Passagetype? notetype = _context.Passagetypes.Where(t => t.Abbrev == "NOTE").FirstOrDefault();
            if (notetype != null)
            {
                IEnumerable<PublishedAndReady> notes = all.Where(all => all.Passage.PassagetypeId == notetype.Id);
                int introcount = notes.Where(n => n.Section.Level < 3).Count();
                PublishedAndReady [] ordered = all.Where(n => n.Section.Level == 3).OrderBy(a => a.Section.Sequencenum).ThenBy(a => a.Passage.Sequencenum).ToArray();
                IEnumerable<PublishedAndReady> maybechapter = notes.Where(n => n.Section.Level == 3);
                foreach(PublishedAndReady note in maybechapter)
                {
                    int ix = Array.IndexOf(ordered, note);
                    while (ix > 0 && ordered [ix].Passage.PassagetypeId == notetype.Id)
                    {
                        ix--;
                    }
                    if (ordered [ix].Passage.PassagetypeId == chnumtype?.Id)
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
        }
        return obts;
    }
}
