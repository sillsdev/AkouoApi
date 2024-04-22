namespace AkouoApi.Models;

public class Image
{
    public Image()
    {
    }
    public Image(int id, string format, DateTime timestamp, string graphics_filename, string url)
    {
        Id = id;
        Format = format;
        Timestamp = timestamp;
        Graphics_filename = graphics_filename;
        Url = url;
    }
    public string? Format { get; set; }
    public string? Url { get; set; }
    public string? Graphics_filename { get; set; }
    public DateTime? Timestamp { get; set; }
    public int? Id { get; set; }

}
