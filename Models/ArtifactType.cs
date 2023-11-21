
namespace AkoúoApi.Models
{
    public class Artifacttype : BaseModel, IArchive
    {
        public string? Typename { get; set; }
        public bool Archived { get; set; }
        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
}
