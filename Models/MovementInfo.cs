using System.Diagnostics;

namespace AkouoApi.Models;

public class MovementInfo : BaseInfo
{
    public MovementInfo(int id,
                        string title,
                        Audio? audio,
                        Image [] graphics,
                        SectionShort [] sections,
                        SectionInfo [] sectionInfo,
                        IEnumerable<Published> ready, int index,
                        AudioNote [] audioNotes) :
        base(id, title, audio, graphics, sections, sectionInfo, ready)
    {
        int startchap = ready.Where(p => p.Sectionsequence == Section_start && p.Passagetype == null).Select(p => (int?)p.DestinationChapter()).Min() ?? 0;
        int endchap = ready.Where(p => p.Sectionsequence == Section_end && p.Passagetype == null).Select(p => (int?)p.DestinationChapter()).Max() ?? 0;
        Chapter_start = startchap;
        Chapter_end = endchap;
        Movement = index;
        Audio_notes = audioNotes;
        Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} XXX MovementInfo title audio {audio?.Url}, {audio?.Audio_filename}");
    }
    public int Movement { get; set; }
    public int Chapter_start { get; set; }
    public int Chapter_end { get; set; }
    public string Notes_title { get; set; } = "Notes";
    public AudioNote [] Audio_notes { get; set; } = [];



}
