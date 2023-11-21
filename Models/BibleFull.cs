using AkoúoApi.Models;
using System.Xml.Linq;

namespace AkouoApi.Models;

public class BibleFull : BibleShort
{
    public BibleFull(Bible bible, Audio [] audio): base(bible, audio)
    {
        Description = bible.Description;
    }
    public string? Description { get; set; }
    public string? Alphabet { get; set; }
    public string? Mark { get; set; }
    public string? Publishers { get; set; }
    public string? Language { get; set; }
    public DateTime? Date { get; set; }
    public string? Country { get; set; }
    public string? Attributions { get; set; }
    public string? Links { get; set; }
    public string? Filesets { get; set; }

}