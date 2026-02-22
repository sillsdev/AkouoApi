namespace AkouoApi.Models;

public class Image(int id, string size, DateTime timestamp, string graphics_filename, string url)
{
    public string Format { get; set; } = size == "512" ? "Thumbnail" : "WEBP";
    public string Size { get; set; } = size;
    public string? Url { get; set; } = url;
    public string? Graphics_filename { get; set; } = graphics_filename;
    public DateTime? Timestamp { get; set; } = timestamp;
    public int? Id { get; set; } = id;

}
