namespace AkouoApi.Models;

public class Sharedresourcereference : BaseModel, IArchive
{
    public int SharedResourceId { get; set; }
    public Sharedresource? SharedResource { get; set; }

    public string Book { get; set; } = "";

    public int Chapter { get; set; }
    public string? Verses { get; set; }

    public bool Archived { get; set; }

}