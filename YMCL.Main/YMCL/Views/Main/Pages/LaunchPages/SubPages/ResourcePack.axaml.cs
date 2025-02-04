using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using JsonElement = System.Text.Json.JsonElement;
using SearchOption = Microsoft.VisualBasic.FileIO.SearchOption;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public partial class ResourcePack : UserControl, INotifyPropertyChanged
{
    private readonly ObservableCollection<LocalResourcePackEntry> _items = [];
    public ObservableCollection<LocalResourcePackEntry> FilteredItems { get; set; } = [];
    private readonly GameEntry _entry;
    private string _filter = string.Empty;

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public ResourcePack(GameEntry entry)
    {
        _entry = entry;
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
            var path = Public.Module.Mc.GameSetting.GetGameSpecialFolder(_entry, GameSpecialFolder.ResourcePacksFolder);
            YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var items = ModManageList.SelectedItems;
            if (items is null) return;
            var text = (from object? item in items select item as LocalResourcePackEntry).Aggregate(string.Empty,
                (current, mod) => current + $"• {Path.GetFileName(mod.Name)}\n");

            var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
                ? MainLang.MoveToRecycleBin
                : MainLang.DeleteSelect;
            var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog != ContentDialogResult.Primary) return;

            foreach (var item in items)
            {
                var file = item as LocalResourcePackEntry;
                if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
                {
                    try
                    {
                        FileSystem.DeleteFile(file.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
                else
                {
                    File.Delete(file.Path);
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

    private void LoadItems()
    {
        _items.Clear();

        var files = Directory.GetFiles(
            Public.Module.Mc.GameSetting.GetGameSpecialFolder(_entry, GameSpecialFolder.ResourcePacksFolder)
            , "*.*", System.IO.SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".zip")
                _items.Add(new LocalResourcePackEntry
                {
                    Name = Path.GetFileName(file)[..(Path.GetFileName(file).Length - 4)], Path = file,
                    Icon = GetIconFromZip(file), Description = GetDescriptionFromZip(file)
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

    public Bitmap GetIconFromZip(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            return null;
        }

        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName != "pack.png") continue;

            try
            {
                using var entryStream = entry.Open();
                using var memoryStream = new MemoryStream();
                entryStream.CopyTo(memoryStream);
                memoryStream.Position = 0;
                return new Bitmap(memoryStream);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public string GetDescriptionFromZip(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            return "string.Empty";
        }

        using var archive = ZipFile.OpenRead(zipFilePath);
        foreach (var entry in archive.Entries)
        {
            if (entry.FullName != "pack.mcmeta") continue;
            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var jsonContent = reader.ReadToEnd();
            var packData = JObject.Parse(jsonContent);
            var descriptionToken = packData["pack"]?["description"];
            return descriptionToken != null ? descriptionToken.ToString() : "string.Empty";
        }

        return "string.Empty";
    }
}