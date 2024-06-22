using Amazon.S3.Model;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Collections.Specialized.BitVector32;

namespace AkouoApi.Models
{
    public class Passage : BaseModel
    {
        public Passage() : base()
        {
            Reference = "";
            Title = "";
            StepComplete = "{}";
        }
        public Passage(Published ps)
        {
            Id = ps.Passageid;
            Sequencenum = ps.Sequencenum;
            Book = ps.Book;
            Reference = ps.Reference;
            Title = ps.Title;
            SectionId = ps.Sectionid;
            SharedResourceId = ps.Sharedresource?.Id;
            SharedResource = ps.Sharedresource;
            StartChapter = ps.Startchapter;
            StartVerse = ps.Startverse;
            EndChapter = ps.Endchapter;
            EndVerse = ps.Endverse;
            Passagetype = ps.Passagetype;
        }
        public Passage(int id,
                       decimal sequencenum,
                       string? book,
                       string? reference,
                       int sectionId,
                       Sharedresource? sharedResource,
                       string? title,
                       int? startChapter,
                       int? startVerse,
                       int? endChapter,
                       int? endVerse,
                       string? passagetype) : base()
        {
            Id = id;
            Sequencenum = sequencenum;
            Book = book;
            Reference = reference;
            Title = title;
            SectionId = sectionId;
            SharedResourceId = sharedResource?.Id;
            SharedResource = sharedResource;
            StartChapter = startChapter;
            StartVerse = startVerse;
            EndChapter = endChapter;
            EndVerse = endVerse;
            Passagetype = passagetype;
        }
        public decimal Sequencenum { get; set; }

        public string? Book { get; set; }

        public string? Reference { get; set; }

        public string? Title { get; set; }

        public int SectionId { get; set; }
        
        public virtual Section? Section { get; set; }

        public string? StepComplete { get; set; } //json

        [ForeignKey(nameof(SharedResource))] 
        public int? SharedResourceId { get; set; }

        public Sharedresource? SharedResource { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? StartChapter { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? StartVerse { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? EndChapter { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)] 
        public int? EndVerse { get; set; }

        public string? Passagetype { get; set; }
        public bool ValidScripture {
            get {
                return StartChapter != null && StartVerse != null && EndVerse != null;
            }
        }
       
        public string Verses {
            get {
                string? tmp = StartChapter != EndChapter
                    ? Reference
                    : StartVerse != EndVerse ? StartVerse?.ToString() + "-" + EndVerse?.ToString() : StartVerse?.ToString();
                return tmp ?? "";
            }
        }

    }
}
