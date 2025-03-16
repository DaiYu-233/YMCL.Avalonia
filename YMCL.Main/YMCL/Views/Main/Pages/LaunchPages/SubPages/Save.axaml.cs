using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using Newtonsoft.Json.Linq;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO.Disk;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public partial class Save : UserControl, INotifyPropertyChanged
{
    private readonly ObservableCollection<LocalSaveEntry> _items = [];
    public ObservableCollection<LocalSaveEntry> FilteredItems { get; set; } = [];
    private readonly MinecraftEntry _entry;
    private string _filter = string.Empty;

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public Save(MinecraftEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        LoadItems();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Filter))
            {
                FilterItems();
            }
        };
        Loaded += (_, _) => { LoadItems(); };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_entry, GameSpecialFolder.SavesFolder);
            YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
            _ = Opener.OpenFolder(path);
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = (from object? item in items select item as LocalSaveEntry).Aggregate(string.Empty,
                (current, mod) => current + $"• {Path.GetFileName(mod.Name)}\n");

            var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in items)
            {
                var file = item as LocalSaveEntry;
                if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
                {
                    FileSystem.DeleteDirectory(file.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                }
                else
                {
                    Directory.Delete(file.Path);
                }
            }

            LoadItems();
        };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
        };
        SelectedModCount.Text = $"{MainLang.SelectedItem} 0";
    }

    private async void LoadItems()
    {
        _items.Clear();

        var saves = await GetSaves(_entry);
        saves.ForEach(save =>
        {
            _items.Add(new LocalSaveEntry
            {
                Name = save.FolderName, Path = Path.Combine(Public.Module.Mc.Utils.GetMinecraftSpecialFolder
                    (_entry, GameSpecialFolder.SavesFolder), save.FolderName),
                Icon = save.IconBitmap, Callback = LoadItems, SaveInfo = save,
                Description =
                    $"{MainLang.CreateTime}: {save.CreationTime}, {MainLang.LastModifiedTime}: {save.LastWriteTime}"
            });
        });
        FilterItems();
    }

    private void FilterItems()
    {
        FilteredItems.Clear();
        _items.Where(item => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.Name).ToList().ForEach(mod => FilteredItems.Add(mod));
        NoMatchResultTip.IsVisible = FilteredItems.Count == 0;
        SelectedModCount.Text = $"{MainLang.SelectedItem} {ModManageList.SelectedItems.Count}";
    }

    public static async Task<List<SaveInfo>> GetSaves(MinecraftEntry entry)
    {
        var folderInfos = new List<SaveInfo>();
        var parentPath = Public.Module.Mc.Utils.GetMinecraftSpecialFolder(entry, GameSpecialFolder.SavesFolder);
        var folders = Directory.GetDirectories(parentPath);
        foreach (var folderPath in folders)
        {
            try
            {
                var folderName = Path.GetFileName(folderPath);
                if (!File.Exists(Path.Combine(folderPath, "level.dat"))) continue;
                var creationTime = Directory.GetCreationTime(folderPath);
                var lastWriteTime = Directory.GetLastWriteTime(folderPath);
                Avalonia.Media.Imaging.Bitmap iconBitmap = null;
                SaveEntry? saveNBTEntry = null;
                string iconPath = null;
                try
                {
                    saveNBTEntry = await entry.GetNBTParser().ParseSaveAsync(folderName);
                    iconPath = saveNBTEntry.IconFilePath;
                }
                catch
                {
                }

                if (File.Exists(iconPath ?? Path.Combine(folderPath, "icon.png")))
                {
                    try
                    {
                        using var stream =
                            new System.IO.MemoryStream(
                                await File.ReadAllBytesAsync(iconPath ?? Path.Combine(folderPath, "icon.png")));
                        iconBitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                    }
                    catch
                    {
                        iconBitmap = null;
                    }
                }

                var datFileCount = 0;
                var playerDataPath = Path.Combine(folderPath, "playerdata");
                if (Directory.Exists(playerDataPath))
                {
                    try
                    {
                        datFileCount = Directory.GetFiles(playerDataPath, "*.dat").Length;
                    }
                    catch
                    {
                        datFileCount = 0;
                    }
                }

                var zipFileCount = 0;
                var datapacksPath = Path.Combine(folderPath, "datapacks");
                if (Directory.Exists(datapacksPath))
                {
                    try
                    {
                        zipFileCount = Directory.GetFiles(datapacksPath, "*.zip").Length;
                    }
                    catch
                    {
                        zipFileCount = 0;
                    }
                }

                if (saveNBTEntry == null)
                {
                    saveNBTEntry = new SaveEntry
                    {
                        LastPlayed = new DateTime(1970, 1, 1, 0, 0, 0),
                        Version = "Unknown",
                        AllowCommands = false,
                        GameType = -1,
                    };
                }

                folderInfos.Add(new SaveInfo
                {
                    FolderName = folderName,
                    CreationTime = creationTime,
                    LastWriteTime = lastWriteTime,
                    LastPlayTime = saveNBTEntry.LastPlayed,
                    Version = saveNBTEntry.Version,
                    Seed = saveNBTEntry.Seed,
                    AllowCommands = saveNBTEntry.AllowCommands,
                    GameType = saveNBTEntry.GameType,
                    IconBitmap = iconBitmap,
                    DatFileCount = datFileCount,
                    ZipFileCount = zipFileCount,
                    FolderPath = folderPath
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing folder {folderPath}: {ex.Message}");
            }
        }

        return folderInfos;
    }


    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    public class SaveInfo
    {
        public string FolderName { get; set; }
        public string FolderPath { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastPlayTime { get; set; }
        public Avalonia.Media.Imaging.Bitmap IconBitmap { get; set; }
        public int DatFileCount { get; set; }
        public int ZipFileCount { get; set; }
        public string Version { get; set; }
        public long Seed { get; set; }
        public int GameType { get; set; }
        public bool AllowCommands { get; set; }
    }

    public Save()
    {
    }
}