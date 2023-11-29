using AkoúoApi.Models;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace AkouoApi.Models;

public class BibleFull : BibleShort
{
    public BibleFull(Bible bible, Audio [] audio): base(bible, audio)
    {
        Description = bible.Description;
        if (bible.PublishingData != null)
        {
            try
            {
                dynamic root = JObject.Parse(bible.PublishingData);
                Alphabet = root["alphabet"]?.Value;
                Mark = root ["mark"]?.Value;
                Publishers = root ["publishers"]?.Value;
                string? lp = root["langProps"]?.Value;
                if (lp != null)
                {
                    dynamic langProps = JObject.Parse(lp);
                    Language = langProps ["languageName"]?.Value;
                }
                Date = root ["date"]?.Value;
                Country = root ["country"]?.Value;
                Attributions = root ["attributions"]?.Value;
                Links = root ["links"]?.Value;
                Filesets = root ["filesets"]?.Value;
            }
            catch (Exception)
            { //leave them null;
            }
        }   
    }
    public string? Description { get; set; }
    public string? Alphabet { get; set; }
    public string? Mark { get; set; }
    public string? Publishers { get; set; }
    public string? Language { get; set; }
    public string? Date { get; set; }
    public string? Country { get; set; }
    public string? Attributions { get; set; }
    public string? Links { get; set; }
    public string? Filesets { get; set; }
    public string? Vname => $"{Name}";
    public string? Vdescription => $"{Description}";

}