namespace AkouoApi.Models;

public class PassageBase(Passage p, OBTTypeEnum obtt, Audio? audio, string? text)
{
    public int Id { get; } = p.Id;
    public decimal Passage { get; set; } = p.Sequencenum;
    public string Text { get; set; } = text ?? "";
    public string Obt_type { get; set; } = obtt.ToString();
    public Audio [] Audio { get; set; } = audio != null ? [audio] : [];
}
public class PassageInfo : PassageBase
{
    public PassageInfo(Passage p, OBTTypeEnum obtt, Audio? audio, string? text) : base(p, obtt, audio, text)
    {
        Chapter_start = p.StartChapter ?? 0;
        Verse_start = p.StartVerse ?? 0;
        Chapter_end = p.EndChapter ?? Chapter_start;
        Verse_end = p.EndVerse ?? Verse_start;
    }
    public int Chapter_start { get; set; }
    public int Verse_start { get; set; }
    public int Chapter_end { get; set; }
    public int Verse_end { get; set; }
    public List<AudioNote> Audio_notes { get; set; } = [];
}

public class AudioNote(Passage p, OBTTypeEnum obtt, Audio? audio, string? text, Sharedresource? sr, Image [] images, Audio? titleaudio) : PassageBase(p, obtt, audio, text)
{
    public Audio [] Title_audio { get; set; } = titleaudio == null ? [] : [titleaudio];
    public Image [] Images { get; set; } = images;
    public string Title { get; set; } = sr?.Title ?? p.Reference ?? "";
    public string Note_category { get; set; } = sr?.ArtifactCategory?.Categoryname ?? "audio_note";
    public int Note_category_id { get; set; } = sr?.ArtifactCategory?.Id ?? 0;
    public string LinkUrl { get; set; } = sr?.LinkUrl ?? "";
}
