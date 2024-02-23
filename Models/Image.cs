namespace AkouoApi.Models;

public class Image
{
    public Image()
    {
    }
    public Image(string format, DateTime timestamp, string graphics_filename, string url)
    {
        Format = format;
        Timestamp = timestamp.ToString();
        Graphics_filename = graphics_filename;
        Url = url;
    }
    public string? Format { get; set; }
    public string? Url { get; set; }
    public string? Graphics_filename { get; set; }
    public string? Timestamp { get; set; }
    public int? Id { get; set; }

}
