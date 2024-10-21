namespace AkouoApi.Models;

public class PublishedChapter:BaseModel
{
    public PublishedChapter()
    {
        Bibleid = "";
        Book = "";
    }
    public string Bibleid { get; set; }
    public string Book { get; set; }
    public int Chapter { get; set; }

}
