using static System.Net.Mime.MediaTypeNames;

namespace AkouoApi.Models;

public class BaseInfoShort
{
    public BaseInfoShort(int id, string title,
                    SectionShort [] sections)
    {
        Id = id;
        Title = title;
        Sections = sections;
    }
    public int Id { get; }
    public string Title { get; }
    public SectionShort [] Sections { get; set; }
}
