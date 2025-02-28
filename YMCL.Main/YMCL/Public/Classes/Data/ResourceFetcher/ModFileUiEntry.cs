using System.Collections.Generic;
using System.Collections.ObjectModel;
using CurseForge.APIClient.Models.Files;
using YMCL.Public.Controls;
using YMCL.Public.Enum;

namespace YMCL.Public.Classes.Data.ResourceFetcher.CourseForgeModFileUiEntry;

public record ShortVersionEntry
{
    public ObservableCollection<VersionEntry> VersionEntries { get; set; } = [];
    public string Version { get; set; }
    public string DisplayVersion { get; set; }
}

public record VersionEntry
{
    public string Name => $"{Version} {Loader}";
    public string Version { get; set; }
    public ModLoaderType? Loader { get; set; }
    public CourseForgeModFileExpander Expander { get; set; }
}

public record ModFile
{
    public string GameVersion { get; set; }
    public int FileId { get; set; }
    public string Filename { get; set; }
    public FileReleaseType ReleaseType { get; set; }
    public ModLoaderType Loader { get; set; }
}