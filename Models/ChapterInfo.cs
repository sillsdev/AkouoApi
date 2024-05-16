namespace AkouoApi.Models;

public class ChapterInfo:BaseInfo
{
    public ChapterInfo(List<Section> sections, IOrderedEnumerable<Passage> passages, IEnumerable<Section> movements,
                    Audio? audio, Image [] graphics, SectionInfo [] sectionInfo, List<AudioNote> audio_notes, IEnumerable<PublishedAndReady> ready, int index, string text) :
    base(sections, audio, graphics, sectionInfo, ready)
    {
        Id = index; 
        Chapter = index;
        Text = text;
        Movement_start = movements.FirstOrDefault()?.Id??0;
        Movement_end = movements.LastOrDefault()?.Id ?? 0;
        Passage_start = (int)passages.First().Id;
        Passage_end = (int)passages.Last().Id;
        Audio_notes = audio_notes;
    }
    public int Id { get; }
    public int Chapter { get; set; }
    public int Movement_start { get; set; }
    public int Movement_end { get; set; }
    public int Passage_start { get; set; }
    public int Passage_end { get; set; }
    public string Notes_title { get; set; } = "Notes";
    public List<AudioNote> Audio_notes { get; set; } = new();
}
