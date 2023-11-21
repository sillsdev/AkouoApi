using AkoúoApi.Data;
using AkoúoApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace AkoúoApi.Services;

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
    protected IQueryable<Bible> ReadyBibles()
    {
        return _context.Bibles
            .Where(o => !o.Archived && o.AnyPublished && o.Iso != null &&
                                            o.BibleId != null && o.BibleName != null)
            .Include(b => b.BibleMediafile).Include(b => b.IsoMediafile);
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
    protected static string? GetDefault(string? defaultParams, string label)
    {
        dynamic tmp = JObject.Parse(defaultParams ?? "{}");
        return tmp.Value<string>(label);
    }
}
