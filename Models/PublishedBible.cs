using System.ComponentModel.DataAnnotations.Schema;

namespace AkouoApi.Models;

public class PublishedBible : BaseModel
{
    public PublishedBible()
    {
        BibleId = "";
        Biblename = "";
        Iso = "";
    }
    public string BibleId { get; set; }
    public string Biblename { get; set; }
    public string Iso { get; set; }
    public string? Description { get; set; }
    public int? Isomediafileid { get; set; }
    public Mediafile? Isomediafile { get; set; }
    public int? Biblemediafileid { get; set; }
    public Mediafile? Biblemediafile { get; set; }
    public string? Publishingdata { get; set; }
    public bool HasPublic { get; set; }
    public int Organizationid { get; set; }
}

[Table("Vwpublishedbibles")]
public class VwPublishedBible : PublishedBible
{ }
[Table("Vwobthelpsbibles")]
public class VwHelpsBible: PublishedBible
{ }