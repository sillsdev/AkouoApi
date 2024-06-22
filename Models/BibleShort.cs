namespace AkouoApi.Models
{
    /* 
{ 
    "data": 
    [
    {
        "bible_id": "ENGSEB",
        "abbr": "SEB",
        "name": "Spoken English Bible®",
        "language_id": 6414,
        "iso": "eng",
        "audio": [
            {
            "format": "MP3",
            "timestamp": "2023-08-03T12:00:00.000000",
            "audio_filename": null,
            "url": null
            }
        ]
    }
    ]
}
*/
    public class BibleShort : BaseModel, IComparable<BibleShort>
    {
        public BibleShort(Bible bible, Audio[] audio) {
            Id = bible.Id;
            Bible_id = bible.BibleId??"";
            Iso = bible.Iso;
            Name = bible.BibleName;
            Title_audio = audio;
        }
        public string Bible_id { get; set; }
        public string? Abbr => Bible_id? [3..] ?? "";
        public string? Iso { get; set; }
        public string? Name { get; set; }
        public Audio [] Title_audio { get; set; } = Array.Empty<Audio>();
        public List<AudioNote> Audio_notes { get; set; } = new();
        public int CompareTo(BibleShort? compare)
        {
            // A null value means that this object is greater.
            return compare == null ? 1 :
            Bible_id.CompareTo(compare.Bible_id);
        }
    }
}
