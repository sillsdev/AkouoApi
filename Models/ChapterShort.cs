namespace AkouoApi.Models;

public class ChapterShort : BaseInfoShort
{
    public ChapterShort(int id, int num, string title, SectionShort [] sectionInfo) : base(id, title, sectionInfo)
    {
        Chapter = num;
    }
    public int Chapter { get; }
}
