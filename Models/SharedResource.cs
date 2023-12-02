namespace AkouoApi.Models;

public partial class Sharedresource : BaseModel, IArchive
{
    public int? PassageId { get; set; }
    public Passage? Passage { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Languagebcp47 { get; set; }

    public string? TermsOfUse { get; set; }

    public string? Keywords { get; set; }

    public int? ArtifactCategoryId { get; set; }

    public Artifactcategory? ArtifactCategory { get; set; }
    public int? TitleMediafileId { get; set; }

    public Mediafile? TitleMediafile { get; set; }

    public bool Note { get; set; }

    public string? LinkUrl { get; set; }

    public bool Archived { get; set; }
}
