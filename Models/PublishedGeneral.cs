using System.ComponentModel.DataAnnotations.Schema;

namespace AkouoApi.Models;

public class PublishedGeneral: Published
{
    public override int? DestinationChapter()
    {
        return null;
    }

}
[Table("Vwpublishedgeneral")]
public class VwPublishedGeneral : PublishedGeneral
{ }
[Table("Vwobthelpsgeneral")]
public class VwOBTHelpsGeneral : PublishedGeneral
{ }
