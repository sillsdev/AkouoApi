namespace AkouoApi.Models;

public class MovementInfo: BaseInfo
{
    public MovementInfo(List<Section> sections, 
                        Audio? audio, 
                        Image[] graphics, 
                        SectionInfo[] sectionInfo, 
                        IEnumerable<Published> ready, int index, Section m,
                        AudioNote [] audioNotes) : 
        base(sections, audio, graphics, sectionInfo, ready)
    {
        int startchap = ready.Where(p => p.Sectionsequence == Section_start && p.Passagetype == null).Select(p => (int?)p.DestinationChapter()).Min() ?? 0;
        int endchap = ready.Where(p => p.Sectionsequence == Section_end && p.Passagetype == null).Select(p => (int?)p.DestinationChapter()).Max() ?? 0;
        Chapter_start = startchap;
        Chapter_end = endchap;
        Id = m.Id;
        Movement = index;
        Text = m.Name;
        Audio_notes = audioNotes;
    }
    public int Id { get; }
    public int Movement { get; set; }
    public int Chapter_start { get; set; }
    public int Chapter_end { get; set; }
    public string Notes_title { get; set; } = "Notes";
    public AudioNote[] Audio_notes { get; set; } = Array.Empty<AudioNote>();



}
