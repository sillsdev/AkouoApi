using AkoúoApi.Services;
using System.ComponentModel.DataAnnotations.Schema;


namespace AkoúoApi.Models
{
    public partial class Mediafile : BaseModel, IArchive
    {
        public int? PassageId { get; set; }
        public virtual Passage? Passage { get; set; }

        public int PlanId { get; set; }
        public virtual Plan? Plan { get; set; }

        public int? ArtifactTypeId { get; set; }
        public Artifacttype? ArtifactType { get; set; }
        public int? ArtifactCategoryId { get; set; }
        public Artifactcategory? ArtifactCategory { get; set; }

        public int? VersionNumber { get; set; }
        public int? Duration { get; set; }
        public string? ContentType { get; set; }
        public string? Transcription { get; set; }
        public string? OriginalFile { get; set; }
        public string? S3File { get; set; }
        public long Filesize { get; set; }
        public string? Topic { get; set; }
        public string? Transcriptionstate { get; set; }
        public bool Archived { get; set; }

        //public int? RecordedbyUserId { get; set; }
        //virtual public User? RecordedbyUser { get; set; }

        public string? Languagebcp47 { get; set; }

        public string? PerformedBy { get; set; }
        public bool ReadyToShare { get; set; }
        public int? ResourcePassageId { get; set; }
        public Passage? ResourcePassage { get; set; }
        public bool? Link { get; set; }

        public int? SourceMediaId { get; set; }
        public Mediafile? SourceMedia { get; set; }
        public DateTime DateUpdated { get; set; }

        public bool IsVernacular {
            get { return ArtifactTypeId is null; }
        }

    
    }
    public partial class SourceMediafile : Mediafile
    {

    }

}
