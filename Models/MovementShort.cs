namespace AkouoApi.Models;

public class MovementShort : BaseInfoShort
{
    public MovementShort(Section? s, SectionShort [] sectionInfo) : base(s?.Id??0, s?.Name ?? "",sectionInfo)
    {
    }

}  
