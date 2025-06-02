namespace AkouoApi.Models;

public class ChapterInfo(ChapterShort myShort,
                   string title,
                   IOrderedEnumerable<Published> passages,
                   IEnumerable<MovementShort> movements,
                   Audio? audio,
                   Image [] graphics,
                   SectionInfo [] sectionInfo,
                   List<AudioNote> audio_notes,
                   IEnumerable<Published> ready) : BaseInfo(myShort.Id, title, audio, graphics, myShort.Sections, sectionInfo, ready)
{
    public int Chapter { get; set; } = myShort.Chapter;
    public int Movement_start { get; set; } = movements.FirstOrDefault()?.Id ?? 0;
    public int Movement_end { get; set; } = movements.LastOrDefault()?.Id ?? 0;
    public int Passage_start { get; set; } = (int)passages.First().Id;
    public int Passage_end { get; set; } = (int)passages.Last().Id;
    public string Notes_title { get; set; } = "Notes";
    public List<AudioNote> Audio_notes { get; set; } = audio_notes;
}
