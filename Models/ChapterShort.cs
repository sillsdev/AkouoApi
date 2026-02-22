namespace AkouoApi.Models;

public class ChapterShort(int id, int num, string title, SectionShort [] sectionInfo) : BaseInfoShort(id, title, sectionInfo)
{
    public int Chapter { get; } = num;
}
