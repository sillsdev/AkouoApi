namespace AkouoApi.Models;

public class ChapterWrapper
{
    public ChapterWrapper(string book_id)
    {
        Book_id = book_id;
    }

    public string Book_id { get; set; } = "";
    public string Obt_type { get { return OBTTypeEnum.chapter.ToString(); } }
    public List<ChapterInfo> Chapters { get; set; } = new();
}
