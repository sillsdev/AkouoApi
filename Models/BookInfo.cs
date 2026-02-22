namespace AkouoApi.Models;
public class BookInfo(string testament, string testamentOrder, string bookGroup)
{
    public string Testament { get; set; } = testament;
    public string TestamentOrder { get; set; } = testamentOrder;
    public string BookGroup { get; set; } = bookGroup;
}

