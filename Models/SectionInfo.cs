using System.Diagnostics;

namespace AkouoApi.Models;

public class SectionInfo
{
    public SectionInfo(Section s, Audio? audio, Image [] graphics, PassageInfo [] passageInfo, AudioNote [] audioNotes, bool isPublic)
    {

        Id = s.Id;
        Section = (int)s.Sequencenum;
        Title = s.Name;
        Title_audio = audio != null ? [audio] : [];
        Images = graphics;
        Passages = passageInfo;
        Audio_notes = audioNotes;
        IsPublic = isPublic;
    }
    public int Id { get; }
    public int Section { get; set; }
    public string Title { get; set; } = "";
    public Audio [] Title_audio { get; set; } = [];
    public Image [] Images { get; set; } = [];
    public PassageInfo [] Passages { get; set; } = [];
    public string Notes_title { get; set; } = "Notes";
    public AudioNote [] Audio_notes { get; set; } = [];
    public bool IsPublic { get; set; }

}
