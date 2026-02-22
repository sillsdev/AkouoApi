namespace AkouoApi.Models;

public class Audio
{
    public Audio()
    {
    }
    public Audio(Mediafile media, string url)
    {
        string? extension = Path.GetExtension(media.PublishedAs)?.ToLowerInvariant();
        Format = extension != null
            ? extension switch
            {
                ".mp3" => "audio/mpeg", // [1.3.2]
                ".m4a" => "audio/mp4", // [1.3.2]
                ".mp4" => "audio/mp4", // [1.3.2]
                ".aac" => "audio/aac", // [1.4.2]
                ".wav" => "audio/wav", // [1.2.1]
                ".ogg" or ".oga" => "audio/ogg", // [1.3.1]
                ".flac" => "audio/flac", // [1.5.2]
                                         // For unknown file types, application/octet-stream is the default. [1.6.1]
                _ => media.ContentType ?? "",
            }
            : media.ContentType ?? "";
        Id = media.Id;
        Timestamp = media.DateUpdated.ToUniversalTime();
        Audio_filename = media.PublishedAs ?? "";
        Url = !url.StartsWith("https://") ? "https://" + url : url;
        Duration = media.Duration ?? 0;
    }
    public int Id { get; }
    public string Format { get; } = "";
    public DateTime Timestamp { get; }
    public string Audio_filename { get; } = "";
    public string Url { get; } = "";
    public decimal Duration { get; } = 0;
}
