namespace AkouoApi.Models;

public class UpdatedInfo(string obtType, int Id, string Bibleid, int? bookid = null, string? book_id = null, int? movement_id = null, int? section_id = null)
{
    public int Id { get; } = Id;
    public string Obt_type { get; set; } = obtType;
    public string Bibleid { get; set; } = Bibleid;
    public int? Bookid { get; set; } = bookid;
    public string? Book_id { get; set; } = book_id;
    public int? Movement_id { get; set; } = movement_id;
    public int? Section_id { get; set; } = section_id;
}
