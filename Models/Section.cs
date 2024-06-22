
using System.ComponentModel.DataAnnotations.Schema;


namespace AkouoApi.Models
{
    public class Section : BaseModel, IArchive
    {
        public Section() : base()
        {
            Name = "";
        }
        public Section(Published p) : base()
        {
            Id = p.Sectionid;
            Sequencenum = p.Sectionsequence;
            Name = p.Sectiontitle ?? "";
            PlanId = p.Planid;
            Published = true;
            PublishTo = "";
            Level = p.Level;
            TitleMediafileId = p.Titlemediafileid;
            State = "";
            TitleMediafile = p.Titlemediafile;
            Archived = false;
        }
        public Section(int id,
                       decimal sequencenum, 
                       string? name, 
                       int planId,
                       SectionLevel level, 
                       Mediafile? titleMediafile)
        {
            Id = id;
            Sequencenum = sequencenum;
            Name = name??"";
            PlanId = planId;
            Published = true;
            PublishTo = "";
            Level = level;
            TitleMediafileId = titleMediafile?.Id;
            State = "";
            TitleMediafile = titleMediafile;
            Archived = false;
        }

        public decimal Sequencenum { get; set; }

        public string Name { get; set; }

        //public string? State { get; set; }

        public int PlanId { get; set; }

        public virtual Plan? Plan { get; set; }
        public bool Published { get; set; }
        public string PublishTo { get; set; } = "{}";
        public SectionLevel Level { get; set; }
        [ForeignKey(nameof(TitleMediafile))]
        public int? TitleMediafileId { get; set; }
        public string? State { get; set; }
        public Mediafile? TitleMediafile { get; set; }
        public bool Archived { get; set; }

    }
}
