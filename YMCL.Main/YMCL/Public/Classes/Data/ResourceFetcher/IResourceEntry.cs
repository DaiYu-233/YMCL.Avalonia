using YMCL.Public.Enum;

namespace YMCL.Public.Classes.Data.ResourceFetcher;

public interface IResourceEntry
{
    public string Name { get; }
    public string Summary { get; }
    public string Pic { get; }
    public string DisplayType { get; }
    public ulong DownloadCount { get; }
    public DateTime LastUpdateTime { get; }
    public ResourceSource Source { get; }
}