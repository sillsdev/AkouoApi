using System.IO.IsolatedStorage;

namespace AkoúoApi.Models;

public class Language
{
    public Language(string iso, string name, Audio [] audios, int bibles)
    {
        this.Iso = iso;
        this.Name = name;
        this.Autonym = name;
        this.Audio = audios;
        this.Bibles = bibles;
    }
    public string Iso { get; set; } = "";
    public string Id { get => Iso; }
    public string Name { get; set; } = "";
    public string Autonym { get; set; } = "";
    public Audio [] Audio { get; set; } = Array.Empty<Audio>();
    public int Bibles { get; set; } = 0;
}