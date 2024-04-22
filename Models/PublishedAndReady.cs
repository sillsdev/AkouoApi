namespace AkouoApi.Models;
public class PublishedAndReady
{
    public PublishedAndReady(Bible bible, Project project, Section section, Passage passage, Mediafile mediafile)
    {
        Bible = bible;
        Project = project;
        Section = section;
        Passage = passage;
        Mediafile = mediafile;
    }
    public Bible Bible { get; set; } = null!;
    public Project Project { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Passage Passage { get; set; } = null!;
    public Mediafile Mediafile { get; set; } = null!;
}
