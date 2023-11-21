using AkoúoApi.Services;

namespace AkoúoApi.Models;

public class Audio
{
    public Audio()
    {
    }
    public Audio(string format, DateTime timestamp, string audio_filename, string url)
    {
        Format = format;
        Timestamp = timestamp;
        Audio_Filename = audio_filename;
        Url = url;
    }
    public string Format { get; } = "";
    public DateTime Timestamp { get; }
    public string Audio_Filename { get; } = "";
    public string Url { get; } = "";
}
