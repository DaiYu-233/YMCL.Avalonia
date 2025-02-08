using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;

namespace YMCL.Public.Classes;

public class AggregateSearchEntry()
{
    public string Tag { get; set; }
    public int Order { get; set; }
    public string Type { get; set; }
    public string Summary { get; set; }
    public string Text { get; set; }
    public string? Target { get; set; }
    public MinecraftEntry? MinecraftEntry { get; set; }
    public VersionManifestEntry? VersionManifestEntry { get; set; }
    public string? Keyword { get; set; }
    public AccountInfo? Account { get; set; }
}