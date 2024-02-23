namespace AkouoApi.Models;
public class BookInfo
{
    public BookInfo(string testament, int testamentOrder, string bookGroup)
    {
        Testament = testament;
        TestamentOrder = testamentOrder;
        BookGroup = bookGroup;
    }
    public string Testament { get; set; } = "";
    public int TestamentOrder { get; set; }
    public string BookGroup { get; set; } = "";
}

