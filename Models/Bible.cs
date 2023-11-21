using System.ComponentModel.DataAnnotations.Schema;

namespace AkoúoApi.Models
{
    public class Bible : BaseModel
    {
        public string? BibleId { get; set; }
        public string? Iso { get; set; }
        public string? BibleName { get; set; }
        public string? Description { get; set; }
        public bool AnyPublished { get; set; }
        public string? PublishingData { get; set; } //json
        public int? IsoMediafileId { get; set; }
        public virtual Mediafile? IsoMediafile { get; set; }
        public int? BibleMediafileId { get; set; }
        public virtual Mediafile? BibleMediafile { get; set; }

        public bool Archived { get; set; }
    }
}
