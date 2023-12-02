using AkouoApi.Models;

namespace AkouoApi.Models;

public class Book :BaseModel
{
    public string? Book_id { get; set; }  //"GEN"
    public string? Name { get; set; }
    public string? Name_long { get; set; }
    public string? Name_alt { get; set; }
    public string? Testament { get; set; }
    public int? Testament_order { get; set; }
    public string? Book_order { get; set; } // "A01"
    public string? Book_group { get; set; } // "The Law"
    public int [] Chapters { get; set; } = Array.Empty<int>();
    public string [] Movements { get; set; } = Array.Empty<string>();
    public Audio [] Audio { get; set; } = Array.Empty<Audio>();
    public Audio [] Audio_alt { get; set; } = Array.Empty<Audio>();
    public Image [] Images { get; set; } = Array.Empty<Image>();
}
