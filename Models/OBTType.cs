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
    extra,
    title
};

public class OBTType : BaseModel, IComparable<OBTType>
{
    public OBTType(OBTTypeEnum type)
    {
        Type = type;
        Id = (int)type;
    }
    private readonly string [] OBTTypeDescription = {"Chapter", "Book of the Bible", "Movement - large portion of scripture consisting of one or more sections", "Introduction - such as to the book or to a movement", "Scripture passage", "Audio notes - may be associated with a particular passage of scripture or stand-alone", "Appendix materials - outside of a scripture verse, usually at the end of a book", "Extra", "Title"};
    private OBTTypeEnum Type { get; set; }

    public string Obt_type { get { return Type.ToString(); } }
    public string Description { get { return OBTTypeDescription [((int)Type)-1];  } }

    public int CompareTo(OBTType? compare)
    {
        // A null value means that this object is greater.
        return compare == null ? 1 : Id.CompareTo(compare.Id);
    }
}
