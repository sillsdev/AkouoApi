
namespace AkoúoApi.Models
{
    public class Section : BaseModel, IArchive
    {
        public Section() : base()
        {
            Name = "";
        }

        public decimal Sequencenum { get; set; }

        public string Name { get; set; }

        //public string? State { get; set; }

        public int PlanId { get; set; }

        public virtual Plan? Plan { get; set; }
        public bool Published { get; set; }
        public int Level { get; set; }
        public bool Archived { get; set; }

    }
}
