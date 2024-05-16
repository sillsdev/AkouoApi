using AkouoApi.Services;

namespace AkouoApi.Models;

public class PassageBase
{
    public PassageBase(Passage p, Audio? audio, string? text)
    {
        Id = p.Id;
        Passage = p.Sequencenum;
        Text = text ?? ""; 
        Audio = audio != null ? new Audio [] { audio } : Array.Empty<Audio>();
    }
    public int Id { get; }
    public decimal Passage { get; set; }
    public string Text { get; set; } = "";
    public string Obt_type { get; set; } = "scripture";
    public Audio [] Audio { get; set; } = Array.Empty<Audio>();
}
public class PassageInfo: PassageBase
{
    public PassageInfo(Passage p, Audio? audio, string? text):base(p, audio, text)
    {
        Chapter_start = p.StartChapter??0;
        Verse_start = p.StartVerse ?? 0;
        Chapter_end = p.EndChapter ?? Chapter_start;
        Verse_end = p.EndVerse ?? Verse_start;
        Passage = (int)Passage;
    }
    public int Chapter_start { get; set; }
    public int Verse_start { get; set; }
    public int Chapter_end { get; set; }
    public int Verse_end { get; set; }
    public List<AudioNote> Audio_notes { get; set; } = new();
}

public class AudioNote : PassageBase
{
    public AudioNote(Passage p, Audio? audio, string? text, Sharedresource? sr, Image [] images, Audio? titleaudio) :base(p, audio, text)
    {
        Obt_type = OBTTypeEnum.audio_note.ToString();
        Images = images;
        Title = sr?.Title ?? "";
        Title_audio = titleaudio != null ? new Audio [] { titleaudio } : Array.Empty<Audio>();
        Note_category = sr?.ArtifactCategory?.Categoryname ?? "audio_note";
        Note_category_id = sr?.ArtifactCategory?.Id??0;
    }
    public Image [] Images { get; set; } = Array.Empty<Image>();
    public string Title { get; set; } = "";
    public Audio [] Title_audio { get; set; } = Array.Empty<Audio>();
    public string Note_category { get; set; } = "audio_note";
    public int Note_category_id { get; set; } = 1;
}
