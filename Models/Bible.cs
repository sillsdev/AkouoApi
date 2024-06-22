using System.ComponentModel.DataAnnotations.Schema;

namespace AkouoApi.Models
{
    public class Bible : BaseModel
    {
        public Bible() : base() { }
        public Bible(int id,
                     string bibleId,
                     string iso,
                     string? bibleName,
                     string? description,
                     string? pubData,
                     Mediafile? isoMedia,
                     Mediafile? bibleMedia) : base()
        { 
        
            Id = id;
            BibleId = bibleId;
            Iso = iso;
            BibleName = bibleName;
            Description = description;
            PublishingData = pubData;
            IsoMediafileId = isoMedia?.Id;
            IsoMediafile = isoMedia;
            BibleMediafileId = bibleMedia?.Id;
            BibleMediafile = bibleMedia;
            AnyPublished = true;
            Archived = false;
        }
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
