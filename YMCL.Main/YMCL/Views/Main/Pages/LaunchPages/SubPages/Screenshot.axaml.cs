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
        Loaded += (_, _) => { LoadItems(); };
        DataContext = this;
        RefreshModBtn.Click += (_, _) => { LoadItems(); };
        OpenFolderBtn.Click += (_, _) =>
        {
            var path = Public.Module.Mc.GameSetting.GetGameSpecialFolder(_entry, GameSpecialFolder.ScreenshotsFolder);
            YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        };
    }

    private void LoadItems()
    {
        _items.Clear();

        var files = Directory.GetFiles(
            Public.Module.Mc.GameSetting.GetGameSpecialFolder(_entry, GameSpecialFolder.ScreenshotsFolder)
            , "*.*", System.IO.SearchOption.AllDirectories);
        foreach (var file in files)

            if (Path.GetExtension(file) == ".png")
                _items.Add(new LocalResourcePackEntry
                {
                    Name = Path.GetFileName(file)[..(Path.GetFileName(file).Length - 4)], Path = file,
                    Icon = null, Description = $"{MainLang.ImportTime}: {new FileInfo(file).CreationTime}"
                });

        FilterItems();
    }

    private void FilterItems()
    {
        Container.Children.Clear();
        Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant,
            out var c1);
        _items.Where(item => item.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(mod => mod.Name).ToList().ForEach(mod => Container.Children.Add(new Border
            {
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(10, 5, 10, 5),
                Background = (SolidColorBrush)c1,
                Child = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Text = mod.Name, Margin = new Thickness(5, 3, 5, 3),
                        },
                        new Border
                        {
                            ClipToBounds = true,
                            Margin = new Thickness(10, 0, 10, 10),
                            CornerRadius = new CornerRadius(5),
                            Width = 197, Height = 120,
                            Child = new Image
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Source = new Bitmap(mod.Path),
                            }
                        }
                    }
                }
            }));
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
}