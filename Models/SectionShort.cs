namespace AkouoApi.Models;

public class SectionShort
{
    private readonly Section _section;
    public SectionShort(Section s) 
    {
        _section = s;
        Id = s.Id;
        Title = s.Name;
    }
    public int Id { get; }
    public string Title { get; } = "";
    public Section GetSection() => _section;
}
