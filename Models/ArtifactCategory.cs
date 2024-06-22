namespace AkouoApi.Models;

    public class Artifactcategory : BaseModel, IArchive, IComparable<Artifactcategory>
{
    public string Categoryname { get; set; } = "";
        public bool Discussion { get; set; }
        public bool Resource { get; set; }
        public bool Note { get; set; }
        public string? Color { get; set; }
        public int? TitleMediafileId { get; set; }
       
        public Mediafile? TitleMediafile { get; set; }
        public string? Specialuse { get; set; }
        public bool Archived { get; set; }

        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    public int CompareTo(Artifactcategory? compare)
    {
        // A null value means that this object is greater.
        return compare == null ? 1 :
            (Specialuse ?? "").CompareTo(compare.Specialuse ?? "") == 0 ?
            Categoryname.CompareTo(compare.Categoryname) : (Specialuse ?? "").CompareTo(compare.Specialuse ?? "");
    }
}
