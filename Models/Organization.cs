using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace AkoúoApi.Models;

public class Organization: BaseModel
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? DefaultParams { get; set; }
    //[Column(TypeName = "jsonb")]
    public bool Archived { get; set; }
}
