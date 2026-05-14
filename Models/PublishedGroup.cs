using System.ComponentModel.DataAnnotations.Schema;

namespace AkouoApi.Models;

[Table("vwmpublishedgroups")]
public class PublishedGroup : BaseModel
{
    public string Obt_type { get; set; } = "";
    public int Bid { get; set; }
    public string Bibleid { get; set; } = "";
    public string Name { get; set; } = "";
    public int Planid { get; set; }
    public string Book { get; set; } = "";
    public int Book_id { get; set; }
    public int? Movement_id { get; set; }
    public int? Section_id { get; set; }
    public int? Titlemediafileid { get; set; }
    public string? Publishedas { get; set; }
    public string? Contenttype { get; set; }
    public DateTime? Dateupdated { get; set; }
    public int? Duration { get; set; }
    public int? Filesize { get; set; }
    public int? Imageid { get; set; }
    public DateTime? Imagedate { get; set; }
    public string? Image { get; set; }
    public int Group1 { get; set; }
    public int Group2 { get; set; }
}