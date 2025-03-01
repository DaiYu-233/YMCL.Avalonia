using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DynamicData;
using Modrinth.Models.Enums.Project;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Classes.Data.ResourceFetcher.CurseForgeModFileUiEntry;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO.Network;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages.DownloadPages.ModrinthPages;

public partial class ModFileResult : UserControl, INotifyPropertyChanged
{
    private readonly ModrinthResourceEntry _entry;
    private ShortVersionEntry _selectedShortVersion;
    public ObservableCollection<ShortVersionEntry> Versions { get; } = [];

    public ShortVersionEntry SelectedShortVersion
    {
        get
        {
            ItemsControl.ItemsSource = null;
            ItemsControl.Items.Clear();
            ItemsControl.ItemsSource = _selectedShortVersion?.VersionEntries;
            return _selectedShortVersion;
        }
        set => SetField(ref _selectedShortVersion, value);
    }


    public ModFileResult(ModrinthResourceEntry entry, int type)
    {
        _entry = entry;
        InitializeComponent();
        FileInfo.DataContext = entry;
        Loaded += (_, _) => { _ = Animator.PageLoading.LevelTwoPage(this); };
        Versions.Add(new ShortVersionEntry { Version = "All", DisplayVersion = MainLang.All });
        entry.Versions.ToList().ForEach(x =>
        {
            var parts = x.Split('-').FirstOrDefault()?.Split('.');
            if (parts.Length < 2) return;
            var shortVersion = $"{parts[0]}.{parts[1]}";
            if (Versions.Any(y => y.Version == shortVersion)) return;
            var shortEntry = new ShortVersionEntry { Version = shortVersion, DisplayVersion = shortVersion };
            Versions.Add(shortEntry);
        });
        var shortList = Versions
            .OrderByDescending(s => s.Version == "All" ? 1 : 0)
            .ThenByDescending(s => Version.TryParse(s.Version, out var version) ? version : new Version(0, 0))
            .ToList();
        Versions = new ObservableCollection<ShortVersionEntry>(shortList);
        _ = GetFiles();
        InfoBorder.DataContext = this;
        ItemsControl.DataContext = this;
        SelectedShortVersion = Versions[0];
    }

    private async System.Threading.Tasks.Task GetFiles()
    {
        var versions = await Public.Module.IO.Network.Modrinth.GetVersionsById(_entry.ProjectId);
        Ring.IsVisible = false;
        versions.data.ForEach(y =>
        {
            foreach (var fileGroup in from version in y.game_versions
                     from loader in y.Loaders
                     let fileGroup = new VersionEntry
                     {
                         Version = version,
                         Loader = loader,
                         Expander = new ModrinthFileExpander($"{version} {loader}",
                             _projectToResourceMapping.GetValueOrDefault(_entry.ProjectType, ResourceType.Unknown))
                     }
                     where !Versions[0].VersionEntries.Any(z =>
                         z.Version == version && z.Loader == loader)
                     select fileGroup)
            {
                var disLoader = fileGroup.Loader;
                if (!string.IsNullOrWhiteSpace(fileGroup.Loader))
                {
                    var charArray = fileGroup.Loader.ToCharArray();
                    charArray[0] = char.ToUpper(charArray[0]);
                    disLoader = new string(charArray);
                }

                if (fileGroup.Expander is ModrinthFileExpander expander) expander.Expander.Header = $"{fileGroup.Version} {disLoader}";
                var files = new List<ModrinthFile>();
                versions.data
                    .Where(z => z.game_versions.Contains(fileGroup.Version) && z.Loaders.Contains(fileGroup.Loader))
                    .ToList().ForEach(
                        x =>
                        {
                            x.Files.ForEach(z =>
                            {
                                files.Add(new ModrinthFile
                                {
                                    Size = z.Size,
                                    FileName = z.Filename,
                                    Title = x.Name,
                                    Downloads = x.Downloads,
                                    UpdateTime = x.date_published,
                                    Url = z.Url
                                });
                            });
                        });
                (fileGroup.Expander as ModrinthFileExpander)?.Files.AddRange(files);
                Versions[0].VersionEntries.Add(fileGroup);
            }

            Versions.ToList().ForEach(x =>
            {
                if (x.Version == "All") return;
                foreach (var version in y.game_versions)
                {
                    foreach (var loader in y.Loaders)
                    {
                        var disLoader = loader;
                        if (!string.IsNullOrWhiteSpace(loader))
                        {
                            var charArray = loader.ToCharArray();
                            charArray[0] = char.ToUpper(charArray[0]);
                            disLoader = new string(charArray);
                        }
                        var fileGroup = new VersionEntry
                        {
                            Version = version,
                            Loader = loader,
                            Expander = new ModrinthFileExpander($"{version} {disLoader}",
                                _projectToResourceMapping.GetValueOrDefault(_entry.ProjectType, ResourceType.Unknown))
                        };
                        if (!x.VersionEntries.Any(z =>
                                z.Version == version && z.Loader == loader))
                            if (!y.game_versions.Any(s => s.Contains(x.Version)))
                                continue;
                        if (x.VersionEntries.Any(z =>
                                z.Version == version && z.Loader == loader)) continue;
                        var files = new List<ModrinthFile>();
                        versions.data
                            .Where(z => z.game_versions.Contains(fileGroup.Version) && z.Loaders.Contains(fileGroup.Loader))
                            .ToList().ForEach(
                                x =>
                                {
                                    x.Files.ForEach(z =>
                                    {
                                        files.Add(new ModrinthFile
                                        {
                                            Size = z.Size,
                                            FileName = z.Filename,
                                            Title = x.Name,
                                            Downloads = x.Downloads,
                                            UpdateTime = x.date_published,
                                            Url = z.Url
                                        });
                                    });
                                });
                        (fileGroup.Expander as ModrinthFileExpander)?.Files.AddRange(files);
                        x.VersionEntries.Add(fileGroup);
                    }
                }
            });
        });
        foreach (var shortVersionEntry in Versions)
        {
            var list = shortVersionEntry.VersionEntries.OrderByDescending(v => new Version(v.Version))
                .ThenByDescending(v => v.Loader).ToList();
            shortVersionEntry.VersionEntries = new ObservableCollection<VersionEntry>(list);
        }

        Ring.IsVisible = false;
    }

    public ModFileResult()
    {
        InitializeComponent();
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private static readonly Dictionary<ProjectType, ResourceType> _projectToResourceMapping = new()
    {
        { ProjectType.Mod, ResourceType.Mod },
        { ProjectType.Modpack, ResourceType.ModPack },
        { ProjectType.Resourcepack, ResourceType.ResourcePack },
        { ProjectType.Shader, ResourceType.ShaderPack },
        { ProjectType.Datapack, ResourceType.DataPack },
        { ProjectType.Plugin, ResourceType.Plugin },
        { ProjectType.Project, ResourceType.Unknown }
    };
}