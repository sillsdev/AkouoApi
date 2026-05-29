namespace AkouoApi.Models;

public class UpdatedInfo(string obtType, int Id, string Bibleid)
{
    public int Id { get; } = Id;
    public string Obt_type { get; set; } = obtType;
    public string Bibleid { get; set; } = Bibleid;
}
