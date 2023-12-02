namespace AkouoApi.Models
{
    public partial class Project : BaseModel, IArchive    {
        public Project() : base()
        {
            Name = "";
        }
        public string Name { get; set; }
        public string? Description { get; set; }

        public virtual Organization Organization { get; set; } = null!;
        public int OrganizationId { get; set; }

        public string? Language { get; set; }

        public string? LanguageName { get; set; }

        //public bool? Rtl { get; set; } = true;

        //public bool IsPublic { get; set; } = true;

        public bool Archived { get; set; }

    }
}
