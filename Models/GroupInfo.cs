namespace AkouoApi.Models;

public class GroupInfo(PublishedGroup g, Audio? audio, Image [] graphics)
{
    public int Id { get; } = g.Id;
    public string Obt_type { get; set; } = g.Obt_type;
    public string Bibleid { get; set; } = g.Bibleid;
    public int Bookid { get; set; } = g.Book_id;
    public string Name { get; set; } = g.Name;
    public string Book_id { get; set; } = g.Book;
    public int? Movement_id { get; set; } = g.Movement_id;
    public int? Section_id { get; set; } = g.Section_id;
    public int Group1 { get; set; } = g.Group1;
    public int Group2 { get; set; } = g.Group2;
    public Audio [] Title_audio { get; set; } = audio != null ? [audio] : [];
    public Image [] Images { get; set; } = graphics;
}
