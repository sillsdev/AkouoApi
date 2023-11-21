using AkoúoApi.Data;
using AkoúoApi.Models;
using AkouoApi.Models;
using AkoúoApi.Services;
using Microsoft.EntityFrameworkCore;

namespace AkoúoApi.Services;

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

    public List<BibleShort> GetBibles()
        {
            return ShortBibles(ReadyBibles().ToList());
        }
        public List<BibleFull> GetBible(string bibleId)
        {
            return FullBibles(ReadyBibles().Where(o => o.BibleId == bibleId).ToList());
        }
        public List<BibleShort> GetBibleByIso(string iso)
        {
            return ShortBibles(ReadyBibles().Where(o => o.Iso == iso).ToList());
        }
    
}
