using AkouoApi.Data;
using AkouoApi.Models;

namespace AkouoApi.Services;

public class GroupService(ILogger<LanguageService> logger,
                       AppDbContext context,
                       IS3Service s3Service,
                       MediafileService mediafileService) : BaseService(logger, context, s3Service, mediafileService)
{
    public List<GroupInfo> GetBibleGroups(string bibleId, bool beta, int? group)
    {
        Bible? bible = ReadyBibles(beta, bibleId).FirstOrDefault();
        return bible != null ? GetBibleGroups(bible.BibleId, group) : throw new Exception("Bible not found");
    }

    private List<GroupInfo> GetBibleGroups(string bibleId, int? group)
    {
        IQueryable<PublishedGroup> groups = _context.Publishedgroups.Where(g => g.Bibleid == bibleId);

        groups = group switch
        {
            1 => groups.Where(g => g.Group1 > 0),
            2 => groups.Where(g => g.Group2 > 0),
            _ => groups
        };

        return [.. groups
            .OrderBy(g => g.Name)
            .AsEnumerable()
            .Select(g => {
                Mediafile? media = g.Titlemediafileid != null ? new()
                {
                    Id = g.Titlemediafileid??0,
                    PublishedAs = g.Publishedas,
                    ContentType = g.Contenttype,
                    DateUpdated = g.Dateupdated ?? DateTime.Now,
                    Duration = g.Duration,
                    Filesize = g.Filesize ?? 0
                }: null;
                Audio? audio = g.Titlemediafileid != null ? GetAudio(media) : null;
                Image[] images = GraphicInfo(g.Image, g.Imageid, g.Imagedate);
                return new GroupInfo(g, audio, images);
            })];
    }
}