namespace AkouoApi.Models;

public class ChapterInfo:BaseInfo
{
    public ChapterInfo(ChapterShort myShort,
                       string title,
                       IOrderedEnumerable<Published> passages, 
                       IEnumerable<MovementShort> movements,
                       Audio? audio, 
                       Image [] graphics,
                       SectionInfo [] sectionInfo,
                       List<AudioNote> audio_notes, 
                       IEnumerable<Published> ready) :
    base(myShort.Id, title, audio, graphics, myShort.Sections, sectionInfo, ready)
    {
        Chapter = myShort.Chapter;
        Movement_start = movements.FirstOrDefault()?.Id??0;
        Movement_end = movements.LastOrDefault()?.Id ?? 0;
        Passage_start = (int)passages.First().Id;
        Passage_end = (int)passages.Last().Id;
        Audio_notes = audio_notes;
    }
    public int Chapter { get; set; }
    public int Movement_start { get; set; }
    public int Movement_end { get; set; }
    public int Passage_start { get; set; }
    public int Passage_end { get; set; }
    public string Notes_title { get; set; } = "Notes";
    public List<AudioNote> Audio_notes { get; set; } = new();
}
