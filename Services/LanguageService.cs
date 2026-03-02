using AkouoApi.Data;
using AkouoApi.Models;
using Newtonsoft.Json;

namespace AkouoApi.Services;

public class LanguageService(ILogger<LanguageService> logger,
                       AppDbContext context,
                       IS3Service s3Service,
                       MediafileService mediafileService) : BaseService(logger, context, s3Service, mediafileService)
{
    private static string? GetBibleLanguage(Bible? bible)
    {
        if (bible == null)
            return null;
        string? props = GetDefault(bible.PublishingData, "langProps");
        if (props != null)
        {
            dynamic? j = JsonConvert.DeserializeObject(props);
            while (j is string)
                j = JsonConvert.DeserializeObject(j);
            return j?.Value<string>("languageName");
        }
        return null;
    }

    private List<Language> GetLanguages(IEnumerable<Bible> readybibles)
    {
        List<string> isos = readybibles.Select(o=> o.Iso??"").Distinct().ToList();
        List<Language> languages = [];
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
                                audio != null ? [audio]: [],
                                bibles);
            languages.Add(lang);
        }
        return languages.OrderBy(l => l.Name).ToList();
    }

    public List<Language> GetLanguages(bool publishBeta)
    {
        return GetLanguages([.. ReadyBibles(publishBeta)]);
    }
    public List<Language> GetLanguage(string iso, bool publishBeta)
    {
        return GetLanguages(ReadyBibles(publishBeta).ToList().Where(o => o.Iso == iso));
    }
    public List<Language> GetHelpsLanguages()
    {
        return GetLanguages([.. HelpsReadyBibles()]);
    }
    public List<Language> GetHelpsLanguage(string iso)
    {
        return GetLanguages(HelpsReadyBibles().ToList().Where(o => o.Iso == iso));
    }
}
