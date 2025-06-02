namespace AkouoApi.Models;

public class SectionShort(Section s)
{
    private readonly Section _section = s;

    public int Id { get; } = s.Id;
    public string Title { get; } = s.Name;
    public Section GetSection() => _section;
}
