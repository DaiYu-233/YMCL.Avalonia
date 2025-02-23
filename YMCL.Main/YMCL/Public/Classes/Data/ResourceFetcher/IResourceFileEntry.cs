using System.Collections.Generic;

namespace YMCL.Public.Classes.Data.ResourceFetcher;

public interface IResourceFileEntry
{
    public string Name { get; set; }
    public List<string> McVersions { get; set; }
    public List<int> Dependency { get; set; }
    public string Loader { get; set; }
    public ulong DownloadCount { get; }
    public string DownloadUrl { get; set; }
    public DateTime UpdateTime { get; }
}