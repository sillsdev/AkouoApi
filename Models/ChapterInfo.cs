namespace AkouoApi.Models;

public class ChapterInfo:BaseInfo
{
    public ChapterInfo(List<Section> sections,
                    Audio? audio, Image [] graphics, SectionInfo [] sectionInfo, IEnumerable<PublishedAndReady> ready, int index, string text) :
    base(sections, audio, graphics, sectionInfo, ready)
    {
        Id = index; //TODO: This is not the correct id...or maybe this will work?
        Chapter = index;
        Text = text;
        Movement_start = 1;
        Movement_end = 1;
    }
    public int Id { get; }
    public int Chapter { get; set; }
    public int Movement_start { get; set; }
    public int Movement_end { get; set; }
}
