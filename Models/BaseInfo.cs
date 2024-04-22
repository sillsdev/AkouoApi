namespace AkouoApi.Models;

public class BaseInfo
{
    public BaseInfo(List<Section> sections,
                        Audio? audio, Image [] graphics, SectionInfo [] sectionInfo, IEnumerable<PublishedAndReady> ready)
    {
        decimal sectionstart = sections.Min(s => s.Sequencenum);
        decimal sectionend = sections.Max(s => s.Sequencenum);
        int startchap = ready.Where(p => p.Section.Sequencenum == sectionstart && p.Passage.PassagetypeId == null).Min(p => p.Passage.StartChapter) ?? 0;
        int endchap = ready.Where(p => p.Section.Sequencenum == sectionend && p.Passage.PassagetypeId == null).Max(p => p.Passage.EndChapter) ?? 0;
        int startverse = ready.Where(p => p.Section.Sequencenum == sectionstart && p.Passage.PassagetypeId == null && p.Passage.StartChapter == startchap).Min(p => p.Passage.StartVerse) ?? 0;
        int endverse = ready.Where(p => p.Section.Sequencenum == sectionend && p.Passage.PassagetypeId == null && p.Passage.EndChapter == endchap).Max(p => p.Passage.EndVerse) ?? 0;
        Section_start = (int)sectionstart;
        Section_end = (int)sectionend;
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
