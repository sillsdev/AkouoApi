using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace AkouoApi.Services;

public class LanguageService : BaseService
{

    public LanguageService(ILogger<LanguageService> logger, 
                           AppDbContext context, 
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {

    }

    private static string? GetBibleLanguage(Bible? bible)
    {
        if (bible == null)
            return null;
        string? props = GetDefault(bible.PublishingData, "langProps");
        return JObject.Parse(props ?? "{}").Value<string>("languageName");
    }

    private List<Language> GetLanguages(IEnumerable<Bible> readybibles)
    {
        List<string> isos = readybibles.Select(o=> o.Iso??"").Distinct().ToList();
        List<Language> languages = new();
        for (int ix = 0; ix < isos.Count; ix++)
        {
            string iso = isos[ix];
            IEnumerable<Bible> isoBibles = readybibles.Where(o => o.Iso == iso);
            //find an isoMediafile for each language
            Mediafile? isomedia = isoBibles.Where(o => o.IsoMediafileId != null).FirstOrDefault()?.IsoMediafile;
            string name = GetBibleLanguage(isoBibles.FirstOrDefault()) ?? iso;
            //find the number of bibles for each language
            int bibles = isoBibles.Select(o=> o.BibleId).Distinct().Count();
            Audio? audio=GetAudio(isomedia);
            
            Language lang = new(iso,
                                name,
                                audio != null ? new Audio [] { audio }: Array.Empty<Audio>(),
                                bibles);
            languages.Add(lang);
        };

        return languages;
    }

    public List<Language> GetLanguages(bool publishBeta)
    {
        return GetLanguages(ReadyBibles(publishBeta).ToList());
    }
    public List<Language> GetLanguage(string iso, bool publishBeta)
    {
        return GetLanguages(ReadyBibles(publishBeta).Where(o => o.Iso == iso).ToList());
    }


    

}
