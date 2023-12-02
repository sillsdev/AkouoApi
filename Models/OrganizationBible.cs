namespace AkouoApi.Models
{
    public partial class Organizationbible : BaseModel, IArchive
    {
        public virtual Bible Bible { get; set; } = null!;

        public int BibleId { get; set; }

        public virtual Organization Organization { get; set; } = null!;

        public int OrganizationId { get; set; }

        public bool Ownerorg { get; set; }
        public bool Archived { get; set; }
    }
}
