using System.ComponentModel.DataAnnotations.Schema;

namespace AkouoApi.Models;

[Table("vwmpublished_deleted")]
public class PublishedDeleted
{
    public int Id { get; set; }
    public int Bid { get; set; }
    public string Bibleid { get; set; } = "";
    public int? Bookid { get; set; }
    public string? Book { get; set; }
    public int? Chapter { get; set; }
    public int? Movementid { get; set; }
    public int? Sectionid { get; set; }
    public int? Passageid { get; set; }
    public DateTime Deleted_at { get; set; }
    public string? Passagetype { get; set; }
    public string Visibility { get; set; } = "";
}
