using AkouoApi.Services;

namespace AkouoApi.Models;

public class SectionInfo
{
    public SectionInfo(Section s, Audio? audio, Image[] graphics, PassageInfo[] passageInfo)
    {
        Id = s.Id;
        Section = (int)s.Sequencenum;
        Text = s.Name;
        Title_audio = audio != null ? new Audio [] { audio } : Array.Empty<Audio>();
        Images = graphics;
        Passages = passageInfo;
    }
    public int Id { get; }
    public int Section { get; set; }
    public string Text { get; set; } = "";
    public Audio [] Title_audio { get; set; } = Array.Empty<Audio>();
    public Image [] Images { get; set; } = Array.Empty<Image>();
    public PassageInfo [] Passages { get; set; } = Array.Empty<PassageInfo>();

}
