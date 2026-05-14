using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using static AkouoApi.Utility.ResourceHelpers;
namespace AkouoApi.Models;

[Table("vwmpublished")]
public class Published
{
    public int Id { get; set; }
    public int Passageid { get; set; }
    public int Bid { get; set; }
    public string Bibleid { get; set; } = "";
    public int? Sr { get; set; }
    public int Organizationid { get; set; }
    public bool Isscripture { get; set; }
    public int? Movementid { get; set; }
    public int Planid { get; set; }
    public int Sectionid { get; set; }
    public Section? Section { get; set; }

    public decimal Sectionsequence { get; set; }
    public string? Sectiontitle { get; set; }
    public SectionLevel Level { get; set; }
    public int? Titlemediafileid { get; set; }
    public Mediafile? Titlemediafile { get; set; }
    public bool Story { get; set; }
    public bool Ispublic { get; set; }
    public bool Isbeta { get; set; }
    public bool Isobthelps { get; set; }
    public int? Sectionimageid { get; set; }
    public DateTime? Sectionimagedate { get; set; }
    public string? Sectionimage { get; set; }
    public decimal Sequencenum { get; set; }
    public string Book { get; set; } = "";
    public string? Reference { get; set; }
    public string? Title { get; set; }
    public int? Startchapter { get; set; }
    public int? Startverse { get; set; }
    public int? Endchapter { get; set; }
    public int? Endverse { get; set; }
    public string? Passagetype { get; set; }
    public string Bookname { get; set; } = "";
    public int? Bookmediafileid { get; set; }
    public int? Bookid { get; set; }
    public string Altname { get; set; } = "";
    public int? Altbookmediafileid { get; set; }
    public int? Altbookid { get; set; }
    public int? Sharedresourceid { get; set; }
    public Sharedresource? Sharedresource { get; set; }
    public int Mediafileid { get; set; }
    public Mediafile? Mediafile { get; set; }
    public int Duration { get; set; }
    public string Contenttype { get; set; } = "";
    public string? Transcription { get; set; }
    public string S3file { get; set; } = "";
    public int Filesize { get; set; }
    public DateTime Datecreated { get; set; }
    public string Publishedas { get; set; } = "";
    public string? Passageimage { get; set; }
    public int? Passageimageid { get; set; }
    public DateTime? Passageimagedate { get; set; }
    private int? destinationChapter;
    private bool destinationChapterSet = false;

    public int? DestinationChapter()
    {
        if (!destinationChapterSet)
        {
            if (Startchapter is null || Startchapter == Endchapter)
            {
                destinationChapter = Startchapter;
            }
            else
            {
                string verses = LoadResource("eng-vrs.json");
                Dictionary<string, int []>? versemap = JsonSerializer.Deserialize<Dictionary<string, int[]>>(verses);
                int lastverse = versemap?[Book]?[(Startchapter??1)-1] ?? 1000;
                destinationChapter = (Endverse > lastverse - Startverse + 1 ? Endchapter : Startchapter) ?? 0;
            }
            destinationChapterSet = true;
        }
        return destinationChapter;
    }
}