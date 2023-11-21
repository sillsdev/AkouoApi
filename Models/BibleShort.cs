namespace AkoúoApi.Models
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
    public class BibleShort : BaseModel
    {
        public BibleShort(Bible bible, Audio[] audio) { 
            BibleId = bible.BibleId;
            Iso = bible.Iso;
            Name = bible.BibleName;
            Audio = audio;
        }
        public string? BibleId { get; set; }
        public string? Abbr => BibleId?[3..] ?? "";
        public string? Iso { get; set; }
        public string? Name { get; set; }
        public Audio [] Audio { get; set; } = Array.Empty<Audio>();
    }
}
