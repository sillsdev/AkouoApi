using System.ComponentModel.DataAnnotations.Schema;

namespace AkouoApi.Models
{
    public partial class Graphic : BaseModel, IArchive
    {
        public virtual Organization Organization { get; set; } = null!;

        [ForeignKey(nameof(Organization))]
        public int OrganizationId { get; set; }
        public virtual Mediafile? Mediafile { get; set; } 
        [ForeignKey(nameof(Mediafile))]
        public int? MediafileId { get; set; }
        public string ResourceType { get; set; } = "";
        public int ResourceId { get; set; }
        [Column(TypeName = "jsonb")]
        public string? Info { get; set; } //json
        public DateTime? DateUpdated { get; set; }
        public bool Archived { get; set; }
    }
}
