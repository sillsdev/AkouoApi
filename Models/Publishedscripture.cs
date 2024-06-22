using static AkouoApi.Utility.ResourceHelpers;
using System.Text.Json;
namespace AkouoApi.Models;

public class PublishedScripture : Published
{
    private int? destinationChapter;
    private bool destinationChapterSet = false;

    public override int? DestinationChapter()
    {
        if (!destinationChapterSet)
        {
            if (Startchapter is null || Startchapter == Endchapter)
            {
                destinationChapter = Startchapter;
            }
            else
            {
                string verses = LoadResource("eng-vrs.json");
                Dictionary<string, int []>? versemap = JsonSerializer.Deserialize<Dictionary<string, int[]>>(verses);
                int lastverse = versemap?[Book]?[(Startchapter??1)-1] ?? 1000;
                destinationChapter = (Endverse > lastverse - Startverse + 1 ? Endchapter : Startchapter) ?? 0;
            }
            destinationChapterSet = true;
        }
        return destinationChapter;
    }
    
}
