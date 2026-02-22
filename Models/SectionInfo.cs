namespace AkouoApi.Models;

public class SectionInfo(Section s, Audio? audio, Image [] graphics, PassageInfo [] passageInfo, AudioNote [] audioNotes, bool isPublic)
{
    public int Id { get; } = s.Id;
    public int Section { get; set; } = (int)s.Sequencenum;
    public string Title { get; set; } = s.Name;
    public Audio [] Title_audio { get; set; } = audio != null ? [audio] : [];
    public Image [] Images { get; set; } = graphics;
    public PassageInfo [] Passages { get; set; } = passageInfo;
    public string Notes_title { get; set; } = "Notes";
    public AudioNote [] Audio_notes { get; set; } = audioNotes;
    public bool IsPublic { get; set; } = isPublic;
}
