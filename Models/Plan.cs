
namespace AkouoApi.Models
{
    public partial class Plan : BaseModel, IArchive
    {
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";

        public string? OrganizedBy { get; set; }

        //public bool Flat { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        public int PlantypeId { get; set; }

        //public Plantype Plantype { get; set; } = null!;

        public bool Archived { get; set; }
    }
}
