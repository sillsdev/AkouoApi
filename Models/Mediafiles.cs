
namespace AkouoApi.Models
{
    public partial class Mediafile : BaseModel, IArchive
    {
        public int? PassageId { get; set; }
        public virtual Passage? Passage { get; set; }

        public int PlanId { get; set; }
        public virtual Plan? Plan { get; set; }

        public int? ArtifactTypeId { get; set; }

        public int? VersionNumber { get; set; }
        public decimal? Duration { get; set; }
        public string? ContentType { get; set; }
        public string? Transcription { get; set; }
        public string? PublishedAs { get; set; }
        public long Filesize { get; set; }
        public bool Archived { get; set; }

        public bool ReadyToShare { get; set; }
        public int? ResourcePassageId { get; set; }
        public Passage? ResourcePassage { get; set; }
        public DateTime DateUpdated { get; set; }
    }
    public partial class SourceMediafile : Mediafile
    {

    }

}
