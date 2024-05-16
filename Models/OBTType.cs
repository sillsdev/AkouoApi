namespace AkouoApi.Models;

public enum OBTTypeEnum
{
    chapter = 1,
    book,
    movement,
    introduction,
    scripture,
    audio_note,
    appendix,
    extra
};

public class OBTType : BaseModel
{
    public OBTType(OBTTypeEnum type)
    {
        Type = type;
        Id = (int)type;
    }
    private readonly string [] OBTTypeDescription = {"Chapter", "Book of the Bible", "Movement - large portion of scripture consisting of one or more sections", "Introduction - such as to the book or to a movement", "Scripture passage", "Audio notes - may be associated with a particular passage of scripture or stand-alone", "Appendix materials - outside of a scripture verse, usually at the end of a book", "Extra"};
    private OBTTypeEnum Type { get; set; }

    public string Obt_type { get { return Type.ToString(); } }
    public string Description { get { return OBTTypeDescription [((int)Type)-1];  } }
}
