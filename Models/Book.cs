namespace AkouoApi.Models;

public class Book :BaseModel
{
    private readonly Dictionary<string, BookInfo> BookInfo = new ()
    {
        {  "GEN", new ("OT", 1, "The Law" ) },
        {  "EXO", new ("OT", 2, "The Law" ) },
        {  "LEV", new ("OT", 3, "The Law") },
        {  "NUM", new ("OT", 4,  "The Law") },
        {  "DEU", new ("OT", 5,  "The Law") },
        {  "JOS", new ("OT", 6,  "Historical Books") },
        {  "JDG", new ("OT", 7,  "Historical Books") },
        {  "RUT", new ("OT", 8,  "Historical Books") },
        {  "1SA", new ("OT", 9,  "Historical Books") },
        {  "2SA", new ("OT", 10,  "Historical Books") },
        {  "1KI", new ("OT", 11,  "Historical Books") },
        {  "2KI", new ("OT", 12,  "Historical Books") },
        {  "1CH", new ("OT", 13,  "Historical Books") },
        {  "2CH", new ("OT", 14,  "Historical Books") },
        {  "EZR", new ("OT", 15,  "Historical Books") },
        {  "NEH", new ("OT", 16,  "Historical Books") },
        {  "EST", new ("OT", 17,  "Historical Books") },
        {  "JOB", new ("OT", 18,  "Wisdom Books") },
        {  "PSA", new ("OT", 19,  "Wisdom Books") },
        {  "PRO", new ("OT", 20,  "Wisdom Books") },
        {  "ECC", new ("OT", 21,  "Wisdom Books") },
        {  "SNG", new ("OT", 22,  "Wisdom Books") },
        {  "ISA", new ("OT", 23,  "Major Prophets") },
        {  "JER", new ("OT", 24,  "Major Prophets") },
        {  "LAM", new ("OT", 25,  "Major Prophets") },
        {  "EZK", new ("OT", 26,  "Major Prophets") },
        {  "DAN", new ("OT", 27,  "Major Prophets") },
        {  "HOS", new ("OT", 28,  "Minor Prophets") },
        {  "JOL", new ("OT", 29,  "Minor Prophets") },
        {  "AMO", new ("OT", 30,  "Minor Prophets") },
        {  "OBA", new ("OT", 31,  "Minor Prophets") },
        {  "JON", new ("OT", 32,  "Minor Prophets") },
        {  "MIC", new ("OT", 33,  "Minor Prophets") },
        {  "NAM", new ("OT", 34,  "Minor Prophets") },
        {  "HAB", new ("OT", 35,  "Minor Prophets") },
        {  "ZEP", new ("OT", 36,  "Minor Prophets") },
        {  "HAG", new ("OT", 37,  "Minor Prophets") },
        {  "ZEC", new ("OT", 38,  "Minor Prophets") },
        {  "MAL", new ("OT", 39,  "Minor Prophets") },
        {  "TOB", new ("OT", 40,  "Apocrypha") },
        {  "JDT", new ("OT", 41,  "Apocrypha") },
        {  "ESG", new ("OT", 42,  "Apocrypha") },
        {  "WIS", new ("OT", 43,  "Apocrypha") },
        {  "SIR", new ("OT", 44,  "Apocrypha") },
        {  "BAR", new ("OT", 45,  "Apocrypha") },
        {  "LJE", new ("OT", 46,  "Apocrypha") },
        {  "S3Y", new ("OT", 47,  "Apocrypha") },
        {  "SUS", new ("OT", 48,  "Apocrypha") },
        {  "BEL", new ("OT", 49,  "Apocrypha") },
        {  "1MA", new ("OT", 50,  "Apocrypha") },
        {  "2MA", new ("OT", 51,  "Apocrypha") },
        {  "1ES", new ("OT", 52,  "Apocrypha") },
        {  "MAN", new ("OT", 53,  "Apocrypha") },
        {  "PS2", new ("OT", 54,  "Apocrypha") },
        {  "ODA", new ("OT", 55,  "Apocrypha") },
        {  "3MA", new ("OT", 56,  "Apocrypha") },
        {  "2ES", new ("OT", 57,  "Apocrypha") },
        {  "4MA", new ("OT", 58,  "Apocrypha") },
        {  "PSS", new ("OT", 59,  "Apocrypha") },
        {  "EZA", new ("OT", 60,  "Apocrypha") },
        {  "5EZ", new ("OT", 61,  "Apocrypha") },
        {  "6EZ", new ("OT", 62,  "Apocrypha") },
        {  "JUB", new ("OT", 63,  "Apocrypha") },
        {  "DAG", new ("OT", 64,  "Apocrypha") },
        {  "PS3", new ("OT", 65,  "Apocrypha") },
        {  "2BA", new ("OT", 66,  "Apocrypha") },
        {  "LBA", new ("OT", 67,  "Apocrypha") },
        {  "ENO", new ("OT", 68,  "Apocrypha") },
        {  "1MQ", new ("OT", 69,  "Apocrypha") },
        {  "2MQ", new ("OT", 70,  "Apocrypha") },
        {  "3MQ", new ("OT", 71,  "Apocrypha") },
        {  "REP", new ("OT", 72,  "Apocrypha") },
        {  "4BA", new ("OT", 73,  "Apocrypha") },
        {  "LAO", new ("OT", 74,  "Apocrypha") },
        {  "FRT", new ("OT", 0,  "Introduction") },
        {  "INT", new ("OT", 0,  "Introduction") },
        {  "MAT", new ("NT", 1,  "The Gospels") },
        {  "MRK", new ("NT", 2,  "The Gospels") },
        {  "LUK", new ("NT", 3,  "The Gospels") },
        {  "JHN", new ("NT", 4,  "The Gospels") },
        {  "ACT", new ("NT", 5,  "Historical Books") },
        {  "ROM", new ("NT", 6,  "Pauline Epistles") },
        {  "1CO", new ("NT", 7,  "Pauline Epistles") },
        {  "2CO", new ("NT", 8,  "Pauline Epistles") },
        {  "GAL", new ("NT", 9,  "Pauline Epistles") },
        {  "EPH", new ("NT", 10,  "Pauline Epistles") },
        {  "PHP", new ("NT", 11,  "Pauline Epistles") },
        {  "COL", new ("NT", 12,  "Pauline Epistles") },
        {  "1TH", new ("NT", 13,  "Pauline Epistles") },
        {  "2TH", new ("NT", 14,  "Pauline Epistles") },
        {  "1TI", new ("NT", 15,  "Pauline Epistles") },
        {  "2TI", new ("NT", 16,  "Pauline Epistles") },
        {  "TIT", new ("NT", 17,  "Pauline Epistles") },
        {  "PHM", new ("NT", 18,  "Pauline Epistles") },
        {  "HEB", new ("NT", 19,  "General Epistles") },
        {  "JAS", new ("NT", 20,  "General Epistles") },
        {  "1PE", new ("NT", 21,  "General Epistles") },
        {  "2PE", new ("NT", 22,  "General Epistles") },
        {  "1JN", new ("NT", 23,  "General Epistles") },
        {  "2JN", new ("NT", 24,  "General Epistles") },
        {  "3JN", new ("NT", 25,  "General Epistles") },
        {  "JUD", new ("NT", 26,  "General Epistles") },
        {  "REV", new ("NT", 27,  "Apocalyptic") },
        {  "XXA", new ("NT", 28,  "Extra") },
        {  "XXB", new ("NT", 29,  "Extra") },
        {  "XXC", new ("NT", 30,  "Extra") },
        {  "XXD", new ("NT", 31,  "Extra") },
        {  "XXE", new ("NT", 32,  "Extra") },
        {  "XXF", new ("NT", 33,  "Extra") },
        {  "XXG", new ("NT", 34,  "Extra") },
        {  "BAK", new ("NT", 35,  "Extra") },
        {  "OTH", new ("NT", 36,  "Extra") },
        {  "CNC", new ("NT", 37,  "Extra") },
        {  "GLO", new ("NT", 38,  "Extra") },
        {  "TDX", new ("NT", 39,  "Extra") },
        {  "NDX", new ("NT", 40,  "Extra") }
    };

    private BookInfo? GetBookInfo()
    {
        return Book_id is null || !BookInfo.ContainsKey(Book_id) ? null : BookInfo [Book_id];
    }
    public string? Book_id { get; set; }  //"GEN"
    public string? Name { get; set; }
    public string? Name_long { get; set; }
    public string? Name_alt { get; set; }
    public int? Testament_order {
        get { return GetBookInfo()?.TestamentOrder; }   
        }
    public string? Book_order { // "A01"
        get {
            BookInfo? info = GetBookInfo();
            return (info?.Testament == "OT" ? "A" : "B") + info?.TestamentOrder.ToString();
        } 
    }
    public string? Book_group { // "The Law"
        get { return GetBookInfo()?.BookGroup; }
    } 
    public int [] Chapters { get; set; } = Array.Empty<int>();
    public string [] Movements { get; set; } = Array.Empty<string>();
    public Audio [] Audio { get; set; } = Array.Empty<Audio>();
    public Audio [] Audio_alt { get; set; } = Array.Empty<Audio>();
    public Image [] Images { get; set; } = Array.Empty<Image>();
    public string? Testament {
        get { return GetBookInfo()?.Testament; }
    }
}
