namespace AkouoApi.Models;

public class MovementInfo: BaseInfo
{
    public MovementInfo(string bookId, List<Section> sections, 
                        Audio? audio, Image[] graphics, SectionInfo[] sectionInfo, IEnumerable<PublishedAndReady> ready, int index, Section m) : 
        base(sections, audio, graphics, sectionInfo, ready)
    {
        Book_id = bookId;
        int startchap = ready.Where(p => p.Section.Sequencenum == Section_start && p.Passage.PassagetypeId == null).Min(p => p.Passage.StartChapter ?? 0);
        int endchap = ready.Where(p => p.Section.Sequencenum == Section_end && p.Passage.PassagetypeId == null).Max(p => p.Passage.EndChapter ?? 0);
        Chapter_start = startchap;
        Chapter_end = endchap;
        Id = m.Id;
        Movement = index;
        Text = m.Name;
    }
    public int Id { get; }
    public string Book_id { get; set; }
    public int Movement { get; set; }
    public int Chapter_start { get; set; }
    public int Chapter_end { get; set; }
    public string Obt_type { get { return OBTTypeEnum.movement.ToString(); } }



}
