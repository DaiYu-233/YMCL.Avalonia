using System.Collections.Generic;
using CurseForge.APIClient.Models.Files;
using CurseForge.APIClient.Models.Games;

namespace YMCL.Public.Classes.Data.ResourceFetcher;

public class CurseForgeResourceFileEntry : IResourceFileEntry
{
    public int Id { get; set; }

    public int GameId { get; set; }

    public int ModId { get; set; }

    public bool IsAvailable { get; set; }

    public string DisplayName { get; set; }

    public string FileName { get; set; }

    public FileReleaseType ReleaseType { get; set; }

    public FileStatus FileStatus { get; set; }

    public List<FileHash> Hashes { get; set; } = [];

    public DateTimeOffset FileDate { get; set; }

    public long FileLength { get; set; }

    public long? FileSizeOnDisk { get; set; }

    public string Name { get; set; }
    public List<string> McVersions { get; set; }
    public List<int> Dependency { get; set; }
    public string Loader { get; set; }

    ulong IResourceFileEntry.DownloadCount => Convert.ToUInt64(DownloadCount);

    public long DownloadCount { get; set; }

    public string DownloadUrl { get; set; }
    public DateTime UpdateTime => FileDate.DateTime;

    public List<string> GameVersions { get; set; } = [];

    public List<SortableGameVersion> SortableGameVersions { get; set; } = [];

    public List<FileDependency> Dependencies { get; set; } = [];

    public bool? ExposeAsAlternative { get; set; }

    public int? ParentProjectFileId { get; set; }

    public int? AlternateFileId { get; set; }

    public bool? IsServerPack { get; set; }

    public int? ServerPackFileId { get; set; }

    public bool? IsEarlyAccessContent { get; set; }

    public DateTimeOffset? EarlyAccessEndDate { get; set; }

    public long FileFingerprint { get; set; }

    public List<FileModule> Modules { get; set; } = [];
}