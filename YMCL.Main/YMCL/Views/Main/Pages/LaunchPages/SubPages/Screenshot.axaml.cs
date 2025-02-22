using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Base.Models.Game;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public partial class Screenshot : UserControl, INotifyPropertyChanged
{
    private readonly ObservableCollection<LocalResourcePackEntry> _items = [];
    private readonly MinecraftEntry _entry;
    private string _filter = string.Empty;

    public string Filter
    {
        get => _filter;
        set => SetField(ref _filter, value);
    }

    public Screenshot(MinecraftEntry entry)
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
        Loaded += (_, _) =>
        {
            LoadItems();
            Viewer.TranslateY = 0;
            Viewer.TranslateX = 0;
            Viewer.Scale = 0.6;
        };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_entry, GameSpecialFolder.ScreenshotsFolder);
            YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        };
        CloseButton.Click += (_, _) =>
        {
            _ = Public.Module.Ui.Animator.PageLoading.LevelTwoPage(ListView);
            ListView.IsVisible = true;
            ListView.Opacity = 1.0;
            ViewerRoot.Opacity = 0;
            ViewerRoot.IsVisible = false;
        };
    }

    private void LoadItems()
    {
        _items.Clear();

        var files = Directory.GetFiles(
            Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_entry, GameSpecialFolder.ScreenshotsFolder)
            , "*.*", System.IO.SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".png")
                _items.Add(new LocalResourcePackEntry
                {
                    Name = Path.GetFileName(file), Path = file,
                    Icon = null, Description = $"{MainLang.ImportTime}: {new FileInfo(file).CreationTime}"
                });

        FilterItems();
    }

    private void FilterItems()
    {
        Container.Children.Clear();
        _items.Where(item => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.Name).ToList().ForEach(mod => Container.Children.Add(
                new ScreenshotEntry(mod.Name, mod.Path, LoadItems, () => { ShowImageViewer(mod.Path); })));
        NoMatchResultTip.IsVisible = Container.Children.Count == 0;
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

    private void ShowImageViewer(string path)
    {
        FileName.Text = Path.GetFileName(path);
        Viewer.Source = new Bitmap(path);
        Viewer.TranslateY = 0;
        Viewer.TranslateX = 0;
        Viewer.Scale = 0.6;
        _ = Public.Module.Ui.Animator.PageLoading.LevelTwoPage(ViewerRoot);
        ViewerRoot.IsVisible = true;
        ViewerRoot.Opacity = 1.0;
        ListView.Opacity = 0;
    }
}