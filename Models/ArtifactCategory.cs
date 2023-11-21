using AkoúoApi.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkoúoApi.Models;

    public class Artifactcategory : BaseModel, IArchive
    {
        public string? Categoryname { get; set; }
        public bool Discussion { get; set; }
        public bool Resource { get; set; }
        public bool Note { get; set; }

        public int? TitleMediafileId { get; set; }

        public Mediafile? TitleMediafile { get; set; }
        public bool Archived { get; set; }

        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
