namespace AkouoApi.Models;

public class BaseInfo
{
    public BaseInfo(List<Section> sections,
                    Audio? audio, 
                    Image [] graphics, 
                    SectionInfo [] sectionInfo, 
                    IEnumerable<Published> ready)
    {
        decimal sectionstart = sections.Any() ? sections.Min(s => s.Sequencenum) : 0;
        decimal sectionend = sections.Any() ? sections.Max(s => s.Sequencenum) : 0;
        int startchap = ready.Where(p => p.Sectionsequence == sectionstart && p.Passagetype == null).Min(p => p.DestinationChapter()) ?? 0;
        int endchap = ready.Where(p => p.Sectionsequence == sectionend && p.Passagetype == null).Max(p => p.DestinationChapter()) ?? 0;
        int startverse = ready.Where(p => p.Sectionsequence == sectionstart && p.Passagetype == null && p.Startchapter == startchap).Min(p => p.Startverse) ?? 0;
        int endverse = ready.Where(p => p.Sectionsequence == sectionend && p.Passagetype == null && p.Endchapter == endchap).Max(p => p.Endverse) ?? 0;
        Section_start = (int) sectionstart;
        Section_end = (int) sectionend;
        Verse_start = startverse;
        Verse_end = endverse;
        Title_audio = audio != null ? new Audio [] { audio } : Array.Empty<Audio>();
        Images = graphics;
        Sections = sectionInfo;
    }
    public string Text { get; set; } = "";
    public Audio [] Title_audio { get; set; } = Array.Empty<Audio>();
    public Image [] Images { get; set; } = Array.Empty<Image>();
    public int Section_start { get; set; }
    public int Section_end { get; set; }
    public int Verse_start { get; set; }
    public int Verse_end { get; set; }
    public SectionInfo [] Sections { get; set; }

}
