using System.ComponentModel.DataAnnotations.Schema;

namespace AkoúoApi.Models
{
    public class Passage : BaseModel, IArchive
    {
        public decimal Sequencenum { get; set; }

        public string? Book { get; set; }

        public string? Reference { get; set; }

        public string? State { get; set; }

        public string? Title { get; set; }

        public int SectionId { get; set; }
        
        public virtual Section? Section { get; set; }

        public string? StepComplete { get; set; } //json

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

        public int? PassagetypeId { get; set; }

        public virtual Passagetype? Passagetype { get; set; }
        public bool Archived { get; set; }

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
