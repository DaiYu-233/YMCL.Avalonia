using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Classes.Data.ResourceFetcher.CurseForgeModFileUiEntry;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;
using YMCL.Public.Module.Util;

namespace YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;

public partial class ModFileResult : UserControl, INotifyPropertyChanged
{
    private readonly CurseForgeResourceEntry _entry;
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


    public ModFileResult(CurseForgeResourceEntry entry, int type)
    {
        _entry = entry;
        InitializeComponent();
        FileInfo.DataContext = entry;
        Loaded += (_, _) => { _ = Animator.PageLoading.LevelTwoPage(this); };
        // _ = GetFiles();
        Versions.Add(new ShortVersionEntry { Version = "All", DisplayVersion = MainLang.All });
        entry.LatestFilesIndexes.ForEach(x =>
        {
            var parts = x.GameVersion.Split('.');
            if (parts.Length < 2) return;
            var shortVersion = $"{parts[0]}.{parts[1]}";
            var loader = x.ModLoader != null ? (ModLoaderType)x.ModLoader : ModLoaderType.Any;
            var fileGroup = new VersionEntry
            {
                Version = x.GameVersion,
                Loader = loader.ToString(),
                Expander = new CurseForgeFileExpander(x.GameVersion, entry.Id,
                    type is 6 or 0 ? $"{x.GameVersion} {loader}" : x.GameVersion,
                    x.ModLoader, entry.Type)
            };
            if (!Versions[0].VersionEntries.Any(z => z.Version == x.GameVersion && z.Loader == loader.ToString()))
            {
                Versions[0].VersionEntries.Add(fileGroup);
            }

            if (Versions.Any(y => y.Version == shortVersion)) return;
            var shortEntry = new ShortVersionEntry { Version = shortVersion, DisplayVersion = shortVersion };
            Versions.Add(shortEntry);
        });
        var shortList = Versions
            .OrderByDescending(s => s.Version == "All" ? 1 : 0)
            .ThenByDescending(s => Version.TryParse(s.Version, out var version) ? version : new Version(0, 0))
            .ToList();
        Versions = new ObservableCollection<ShortVersionEntry>(shortList);
        Versions.ToList().ForEach(x =>
        {
            entry.LatestFilesIndexes.ForEach(y =>
            {
                var parts = y.GameVersion.Split('.');
                if (parts.Length < 2) return;
                var shortVersion = $"{parts[0]}.{parts[1]}";
                var loader = y.ModLoader != null ? (ModLoaderType)y.ModLoader : ModLoaderType.Any;
                var fileGroup = new VersionEntry
                {
                    Version = y.GameVersion,
                    Loader = loader.ToString(),
                    Expander = new CurseForgeFileExpander(y.GameVersion, entry.Id,
                        type is 6 or 0 ? $"{y.GameVersion} {loader}" : y.GameVersion,
                        y.ModLoader, entry.Type)
                };
                if (x.Version != shortVersion) return;
                if (x.VersionEntries.Any(z =>
                        z.Version == y.GameVersion && z.Loader == loader.ToString())) return;
                x.VersionEntries.Add(fileGroup);
            });
            var list = x.VersionEntries.OrderByDescending(v => new Version(v.Version))
                .ThenByDescending(v => v.Loader).ToList();
            x.VersionEntries = new ObservableCollection<VersionEntry>(list);
        });
        InfoBorder.DataContext = this;
        ItemsControl.DataContext = this;
        SelectedShortVersion = Versions[0];
    }

    // private async System.Threading.Tasks.Task GetFiles()
    // {
    //     var page = 1;
    //     var files = new List<IResourceFileEntry>();
    //     while (true)
    //     {
    //         var a = await Public.Module.IO.Network.CurseForge.GetFiles(_entry.Id, page);
    //         if (!a.success) break;
    //         if (a.data.Count == 0) break;
    //         files.AddRange(a.data);
    //         page++;
    //     }
    //
    //     List<string> mcVersions = [];
    //     files.ForEach(x => x.McVersions.ForEach(y =>
    //     {
    //         if (!mcVersions.Contains(y))
    //             mcVersions.Add(y);
    //     }));
    //     mcVersions.Sort(new VersionComparer());
    //     mcVersions.Reverse();
    //     mcVersions.ForEach(x =>
    //     {
    //         var control = new CurseForgeModFileExpander();
    //         control.Expander.Header = x;
    //         files.ForEach(y =>
    //         {
    //             if (y.McVersions.Contains(x))
    //             {
    //                 control.ListView.Items.Add(y);
    //             }
    //         });
    //     });
    // }

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
}