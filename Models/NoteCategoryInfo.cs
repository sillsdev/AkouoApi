using System.Drawing;

namespace AkouoApi.Models;

public class NoteCategoryInfo: IComparable<NoteCategoryInfo>
{
    public NoteCategoryInfo(Artifactcategory category, Audio? audio, Image [] images)
    {
        Id = category.Id;
        Category = category.Categoryname ?? "";
        Color = category.Color == "" ? "#ed071d" : category.Color ?? "#ed071d";
        Title_audio = audio != null ? new Audio [] { audio } : Array.Empty<Audio>();
        Images = images;
        SpecialUse = category.Specialuse ?? "";
    }
    public int Id { get; }
    public string Category { get; set; } = "";
    public string Color { get; set; } = "";
    public string SpecialUse { get; set; } = "";
    public Audio [] Title_audio { get; set; } = Array.Empty<Audio>();
    public Image [] Images { get; set; } = Array.Empty<Image>();
    public int CompareTo(NoteCategoryInfo? compare)
    {
        // A null value means that this object is greater.
        return compare == null ? 1 : 
        SpecialUse.CompareTo(compare.SpecialUse) == 0 ? 
        Category.CompareTo(compare.Category) : 
        SpecialUse.CompareTo(compare.SpecialUse);
    }
}
