using System.Collections.ObjectModel;
using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Public.Classes;

public sealed class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;
    public static ObservableCollection<VersionManifestEntry> AllInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> FilteredAllInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> ReleaseInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> SnapshotInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> OldInstallableGames { get; set; } = [];
    public ObservableCollection<AggregateSearchEntry> FilteredAggregateSearchEntries { get; } = [];

    [Reactive]
    public VersionManifestEntry LatestReleaseGame { get; set; } = new()
        { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.Loading, Type = "null" };

    [Reactive]
    public VersionManifestEntry LatestSnapshotGame { get; set; } = new()
        { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.Loading, Type = "null" };

    [Reactive] public bool InstallableRingIsVisible { get; set; } = true;
    [Reactive] public double TaskEntryHeaderWidth { get; set; }
    [Reactive] public double SystemMaxMem { get; set; }
    [Reactive] public string AggregateSearchFilter { get; set; } = string.Empty;
    [Reactive] public double MusicTotalTime { get; set; } 
    [Reactive] public double MusicCurrentTime { get; set; } 
    [Reactive] public string InstallableGameSearchFilter { get; set; } = string.Empty;
    [Reactive] public RecordSongEntry? SelectedRecordSong { get; set; } 
    [Reactive] public RecordSongEntry? SelectedSearchSong { get; set; }

    public UiProperty()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AggregateSearchFilter))
            {
                YMCL.Public.Module.Ui.Special.AggregateSearchUi.Filter(AggregateSearchFilter);
            }
        };
    }
    
    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }
}