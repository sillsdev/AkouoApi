using System.ComponentModel.DataAnnotations.Schema;

namespace AkoúoApi.Models
{
    public partial class Passagetype : BaseModel
    {
        public string? Title { get; set; }
        public string Abbrev { get; set; } = "";
    }
}
