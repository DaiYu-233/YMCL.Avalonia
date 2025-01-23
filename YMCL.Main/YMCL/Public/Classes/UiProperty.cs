using System.Collections.ObjectModel;
using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.App;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Public.Classes;

public sealed class UiProperty : ReactiveObject
{
    private static UiProperty? _instance;
    public static ObservableCollection<VersionManifestEntry> AllInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> ReleaseInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> SnapshotInstallableGames { get; set; } = [];
    public static ObservableCollection<VersionManifestEntry> OldInstallableGames { get; set; } = [];

    [Reactive]
    public VersionManifestEntry LatestReleaseGame { get; set; } = new()
        { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.Loading, Type = "null" };

    [Reactive]
    public VersionManifestEntry LatestSnapshotGame { get; set; } = new()
        { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.Loading, Type = "null" };

    [Reactive] public bool InstallableRingIsVisible { get; set; } = true;
    [Reactive] public double TaskEntryHeaderWidth { get; set; }
    [Reactive] public double SystemMaxMem { get; set; }

    public static UiProperty Instance
    {
        get { return _instance ??= new UiProperty(); }
    }
}