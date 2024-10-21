using AkouoApi.Services;
using static System.Net.WebRequestMethods;

namespace AkouoApi.Models;

public class Audio
{
    public Audio()
    {
    }
    public Audio(Mediafile media, string url)
    {
        Id = media.Id;
        Format = media.ContentType ?? "";
        Timestamp = media.DateUpdated.ToUniversalTime();
        Audio_filename = media.PublishedAs ?? "";
        Url = !url.StartsWith("https://") ? "https://" + url : url;
        Duration = media.Duration??0;                  
    }
    public int Id { get; }
    public string Format { get; } = "";
    public DateTime Timestamp { get; }
    public string Audio_filename { get; } = "";
    public string Url { get; } = "";
    public decimal Duration { get; } = 0;
}
