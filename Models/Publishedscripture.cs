namespace AkouoApi.Models;

public class PublishedScripture : BaseModel
{
    public PublishedScripture()
    {
        BibleId = "";
        Biblename = "";
        Book = "";
        Contenttype = "";
        Iso = "";
        Languagename = "";
        Reference = "";
        S3file = "";
    }
    public int Bid { get; set; }
    public string BibleId { get; set; }
    public string Biblename { get; set; }
    public string Iso { get; set; }
    public string? Bibledescription { get; set; }
    public int? Isomediafileid { get; set; }
    public Mediafile? Isomediafile { get; set; }
    public int? Biblemediafileid { get; set; }
    public Mediafile? Biblemediafile { get; set; }
    public string? Publishingdata { get; set; }
    public string Languagename { get; set; }
    public bool Rtl { get; set; }
    public int Planid { get; set; }
    public int? Movementid { get; set; }
    public int Sectionid { get; set; }
    public Section? Section { get; set; }
    public decimal Sectionsequence { get; set; }
    public string? Sectiontitle { get; set; }
    public int Level { get; set; }
    public int? Titlemediafileid { get; set; }
    public Mediafile? Titlemediafile { get; set; }
    public bool IsPublic { get; set; }
    public int Passageid { get; set; }
    public decimal Sequencenum { get; set; }
    public string Book { get; set; }
    public string Reference { get; set; }
    public string? Title { get; set; }
    public int? Startchapter { get; set; }
    public int? Startverse { get; set; }
    public int? Endchapter { get; set; }
    public int? Endverse { get; set; }
    public string? Passagetype { get; set; }
    public string? Bookname { get; set; }
    public int? Bookid { get; set; }
    public int? Bookmediafileid { get; set; }
    public string? Altname { get; set; }
    public int? Altbookmediafileid { get; set; }
    public int? Altbookid { get; set; }
    public int? Sharedresourceid { get; set; }
    public Sharedresource? Sharedresource { get; set; }
    public int? Mediafileid { get; set; }
    public Mediafile? Mediafile { get; set; }
    public decimal? Duration { get; set; }
    public string? Contenttype { get; set; }
    public string? Transcription { get; set; }
    public string? S3file { get; set; }
    public decimal Filesize { get; set; }
    public DateTime Datecreated { get; set; }
}
