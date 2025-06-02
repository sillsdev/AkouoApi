namespace AkouoApi.Models;

public class NoteCategoryInfo(Artifactcategory category, Audio? audio, Image [] images) : IComparable<NoteCategoryInfo>
{
    public Audio [] Title_audio { get; set; } = audio == null ? [] : [audio];
    public int Id { get; } = category.Id;
    public string Category { get; set; } = category.Categoryname ?? "";
    public string Color { get; set; } = category.Color == "" ? "#ed071d" : category.Color ?? "#ed071d";
    public string SpecialUse { get; set; } = category.Specialuse ?? "";
    public Image [] Images { get; set; } = images;
    public int CompareTo(NoteCategoryInfo? compare)
    {
        // A null value means that this object is greater.
        return compare == null ? 1 :
        SpecialUse.CompareTo(compare.SpecialUse) == 0 ?
        Category.CompareTo(compare.Category) :
        SpecialUse.CompareTo(compare.SpecialUse);
    }
}
