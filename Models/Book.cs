using System.Text.RegularExpressions;

namespace AkouoApi.Models;

public partial class Book : BaseModel, IComparable<Book>
{
    private string extras = "extras";
    private static readonly Dictionary<string, BookInfo> BookInfoMap = new ()
    {
        {  "GEN", new ("OT", "A01", "The Law" ) },
        {  "EXO", new ("OT", "A02", "The Law" ) },
        {  "LEV", new ("OT", "A03", "The Law") },
        {  "NUM", new ("OT", "A04",  "The Law") },
        {  "DEU", new ("OT", "A05",  "The Law") },
        {  "JOS", new ("OT", "A06",  "Historical Books") },
        {  "JDG", new ("OT", "A07",  "Historical Books") },
        {  "RUT", new ("OT", "A08",  "Historical Books") },
        {  "1SA", new ("OT", "A09",  "Historical Books") },
        {  "2SA", new ("OT", "A10",  "Historical Books") },
        {  "1KI", new ("OT", "A11",  "Historical Books") },
        {  "2KI", new ("OT", "A12",  "Historical Books") },
        {  "1CH", new ("OT", "A13",  "Historical Books") },
        {  "2CH", new ("OT", "A14",  "Historical Books") },
        {  "EZR", new ("OT", "A15",  "Historical Books") },
        {  "NEH", new ("OT", "A16",  "Historical Books") },
        {  "EST", new ("OT", "A17",  "Historical Books") },
        {  "JOB", new ("OT", "A18",  "Wisdom Books") },
        {  "PSA", new ("OT", "A19",  "Wisdom Books") },
        {  "PRO", new ("OT", "A20",  "Wisdom Books") },
        {  "ECC", new ("OT", "A21",  "Wisdom Books") },
        {  "SNG", new ("OT", "A22",  "Wisdom Books") },
        {  "ISA", new ("OT", "A23",  "Major Prophets") },
        {  "JER", new ("OT", "A24",  "Major Prophets") },
        {  "LAM", new ("OT", "A25",  "Major Prophets") },
        {  "EZK", new ("OT", "A26",  "Major Prophets") },
        {  "DAN", new ("OT", "A27",  "Major Prophets") },
        {  "HOS", new ("OT", "A28",  "Minor Prophets") },
        {  "JOL", new ("OT", "A29",  "Minor Prophets") },
        {  "AMO", new ("OT", "A30",  "Minor Prophets") },
        {  "OBA", new ("OT", "A31",  "Minor Prophets") },
        {  "JON", new ("OT", "A32",  "Minor Prophets") },
        {  "MIC", new ("OT", "A33",  "Minor Prophets") },
        {  "NAM", new ("OT", "A34",  "Minor Prophets") },
        {  "HAB", new ("OT", "A35",  "Minor Prophets") },
        {  "ZEP", new ("OT", "A36",  "Minor Prophets") },
        {  "HAG", new ("OT", "A37",  "Minor Prophets") },
        {  "ZEC", new ("OT", "A38",  "Minor Prophets") },
        {  "MAL", new ("OT", "A39",  "Minor Prophets") },
        {  "TOB", new ("OT", "A40",  "Apocrypha") },
        {  "JDT", new ("OT", "A41",  "Apocrypha") },
        {  "ESG", new ("OT", "A42",  "Apocrypha") },
        {  "WIS", new ("OT", "A43",  "Apocrypha") },
        {  "SIR", new ("OT", "A44",  "Apocrypha") },
        {  "BAR", new ("OT", "A45",  "Apocrypha") },
        {  "LJE", new ("OT", "A46",  "Apocrypha") },
        {  "S3Y", new ("OT", "A47",  "Apocrypha") },
        {  "SUS", new ("OT", "A48",  "Apocrypha") },
        {  "BEL", new ("OT", "A49",  "Apocrypha") },
        {  "1MA", new ("OT", "A50",  "Apocrypha") },
        {  "2MA", new ("OT", "A51",  "Apocrypha") },
        {  "1ES", new ("OT", "A52",  "Apocrypha") },
        {  "MAN", new ("OT", "A53",  "Apocrypha") },
        {  "PS2", new ("OT", "A54",  "Apocrypha") },
        {  "ODA", new ("OT", "A55",  "Apocrypha") },
        {  "3MA", new ("OT", "A56",  "Apocrypha") },
        {  "2ES", new ("OT", "A57",  "Apocrypha") },
        {  "4MA", new ("OT", "A58",  "Apocrypha") },
        {  "PSS", new ("OT", "A59",  "Apocrypha") },
        {  "EZA", new ("OT", "A60",  "Apocrypha") },
        {  "5EZ", new ("OT", "A61",  "Apocrypha") },
        {  "6EZ", new ("OT", "A62",  "Apocrypha") },
        {  "JUB", new ("OT", "A63",  "Apocrypha") },
        {  "DAG", new ("OT", "A64",  "Apocrypha") },
        {  "PS3", new ("OT", "A65",  "Apocrypha") },
        {  "2BA", new ("OT", "A66",  "Apocrypha") },
        {  "LBA", new ("OT", "A67",  "Apocrypha") },
        {  "ENO", new ("OT", "A68",  "Apocrypha") },
        {  "1MQ", new ("OT", "A69",  "Apocrypha") },
        {  "2MQ", new ("OT", "A70",  "Apocrypha") },
        {  "3MQ", new ("OT", "A71",  "Apocrypha") },
        {  "REP", new ("OT", "A72",  "Apocrypha") },
        {  "4BA", new ("OT", "A73",  "Apocrypha") },
        {  "LAO", new ("OT", "A74",  "Apocrypha") },
        {  "JSA", new ("OT", "A75",  "Apocrypha") },
        {  "JDB", new ("OT", "A76",  "Apocrypha") },
        {  "TBS", new ("OT", "A77",  "Apocrypha") },
        {  "SST", new ("OT", "A78",  "Apocrypha") },
        {  "DNT", new ("OT", "A79",  "Apocrypha") },
        {  "BLT", new ("OT", "A80",  "Apocrypha") },
        {  "FRT", new ("OT", "010",  "Introduction") },
        {  "INT", new ("OT", "020",  "Introduction") },
        {  "MAT", new ("NT", "B01",  "The Gospels") },
        {  "MRK", new ("NT", "B02",  "The Gospels") },
        {  "LUK", new ("NT", "B03",  "The Gospels") },
        {  "JHN", new ("NT", "B04",  "The Gospels") },
        {  "ACT", new ("NT", "B05",  "Historical Books") },
        {  "ROM", new ("NT", "B06",  "Pauline Epistles") },
        {  "1CO", new ("NT", "B07",  "Pauline Epistles") },
        {  "2CO", new ("NT", "B08",  "Pauline Epistles") },
        {  "GAL", new ("NT", "B09",  "Pauline Epistles") },
        {  "EPH", new ("NT", "B10",  "Pauline Epistles") },
        {  "PHP", new ("NT", "B11",  "Pauline Epistles") },
        {  "COL", new ("NT", "B12",  "Pauline Epistles") },
        {  "1TH", new ("NT", "B13",  "Pauline Epistles") },
        {  "2TH", new ("NT", "B14",  "Pauline Epistles") },
        {  "1TI", new ("NT", "B15",  "Pauline Epistles") },
        {  "2TI", new ("NT", "B16",  "Pauline Epistles") },
        {  "TIT", new ("NT", "B17",  "Pauline Epistles") },
        {  "PHM", new ("NT", "B18",  "Pauline Epistles") },
        {  "HEB", new ("NT", "B19",  "General Epistles") },
        {  "JAS", new ("NT", "B20",  "General Epistles") },
        {  "1PE", new ("NT", "B21",  "General Epistles") },
        {  "2PE", new ("NT", "B22",  "General Epistles") },
        {  "1JN", new ("NT", "B23",  "General Epistles") },
        {  "2JN", new ("NT", "B24",  "General Epistles") },
        {  "3JN", new ("NT", "B25",  "General Epistles") },
        {  "JUD", new ("NT", "B26",  "General Epistles") },
        {  "REV", new ("NT", "B27",  "Apocalyptic") },
        {  "XXA", new ("NT", "B28",  "Extra") },
        {  "XXB", new ("NT", "B28",  "Extra") },
        {  "XXC", new ("NT", "B30",  "Extra") },
        {  "XXD", new ("NT", "B31",  "Extra") },
        {  "XXE", new ("NT", "B32",  "Extra") },
        {  "XXF", new ("NT", "B33",  "Extra") },
        {  "XXG", new ("NT", "B34",  "Extra") },
        {  "BAK", new ("NT", "B35",  "Extra") },
        {  "OTH", new ("NT", "B36",  "Extra") },
        {  "CNC", new ("NT", "B37",  "Extra") },
        {  "GLO", new ("NT", "B38",  "Extra") },
        {  "TDX", new ("NT", "B39",  "Extra") },
        {  "NDX", new ("NT", "B40",  "Extra") }
    };

    private BookInfo? GetBookInfo()
    {
        BookInfoMap.TryGetValue(Book_id ?? "", out BookInfo? info);
        return info;
    }

    public static string GetBookOrder(string? bookId)
    {
        return BookInfoMap.TryGetValue(bookId ?? "", out BookInfo? info)
            ? info.TestamentOrder
            : (bookId ?? "");
    }
    public required string Bible_id { get; set; }
    public required string Book_id { get; set; }  //"GEN"
    private bool _story;
    public required bool Story {
        get {
            return Book_group == extras && _story;
        }
        set {
            _story = value;
        }
    }
    public string? Name { get; set; }
    public string? Name_long { get; set; }
    public string? Name_alt { get; set; }
    public int? Testament_order {
        get {
            Match match = FindNumber().Match(GetBookInfo()?.TestamentOrder??"100");
            return match.Success ? int.Parse(match.Value) : 100;
        }
    }
    public string Book_order { // "A01"
        get {
            return GetBookOrder(Book_id);
        }
    }
    public string? Book_group { // "The Law"
        get { return (GetBookInfo()?.BookGroup) ?? extras; }
    }
    public Audio [] Title_audio { get; set; } = [];
    public Audio [] Title_audio_alt { get; set; } = [];
    public ChapterShort [] Chapters { get; set; } = [];
    public MovementShort [] Movements { get; set; } = [];
    public Image [] Images { get; set; } = [];
    public AudioNote [] Audio_notes { get; set; } = [];
    public string? Testament {
        get { return GetBookInfo()?.Testament; }
    }
    public string Obt_type { get { return OBTTypeEnum.book.ToString(); } }
    public int CompareTo(Book? compare)
    {
        // A null value means that this object is greater.
        return compare == null ? 1 : Book_order.CompareTo(compare.Book_order);
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex FindNumber();
}
