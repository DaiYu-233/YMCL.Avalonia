using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.Win32;
using MinecraftLaunch.Classes.Models.Game;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;
using static YMCL.Main.Public.Classes.PlaySongListViewItemEntry;

namespace YMCL.Main.Views.Main;

public partial class MainWindow : Window
{
    public readonly Pages.Download.DownloadPage downloadPage = new();
    public readonly Pages.Launch.LaunchPage launchPage = new();
    public readonly Pages.More.MorePage morePage = new();
    public readonly Pages.Music.MusicPage musicPage = new();
    public readonly Pages.Setting.SettingPage settingPage = new();
    public WindowTitleBarStyle titleBarStyle;
    bool _isOpeningTaskCenter = false;
    public MainWindow()
    {
        InitializeComponent();
        EventBinding();
    }
    private void EventBinding()
    {
        Activated += (_, _) =>
        {
            if (settingPage.PluginSettingNavControl.IsSelected)
            {
                settingPage.pluginSettingPage.ReloadPluginListUi();
            }
        };
        TitleText.PointerPressed += (s, e) =>
        {
            BeginMoveDrag(e);
        };
        PropertyChanged += (s, e) =>
        {
            if (titleBarStyle == WindowTitleBarStyle.Ymcl && e.Property.Name == nameof(WindowState))
            {
                switch (WindowState)
                {
                    case WindowState.Normal:
                        Root.Margin = new Thickness(0);
                        break;
                    case WindowState.Maximized:
                        Root.Margin = new Thickness(20);
                        break;
                }
            }
        };
        AddHandler(DragDrop.DragLeaveEvent, (_, e_) =>
        {
            DragTip.IsOpen = false;
        }, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DragEnterEvent, (_, _) =>
        {
            DragTip.IsOpen = true;
        }, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, (s, e) =>
        {
            HandleDrop(s!, e);
        }, RoutingStrategies.Bubble);
        Nav.SelectionChanged += async (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "Launch":
                    if (!_isOpeningTaskCenter)
                    {
                        launchPage.Root.IsVisible = false;
                        FrameView.Content = launchPage;
                    }
                    _isOpeningTaskCenter = false;
                    break;
                case "Setting":
                    settingPage.Root.IsVisible = false;
                    FrameView.Content = settingPage; break;
                case "Download":
                    downloadPage.Root.IsVisible = false;
                    FrameView.Content = downloadPage; break;
                case "Music":
                    musicPage.Root.IsVisible = false;
                    FrameView.Content = musicPage; break;
                case "More":
                    morePage.Root.IsVisible = false;
                    FrameView.Content = morePage; break;
                case "Task":
                    Const.Window.taskCenter.Show();
                    Const.Window.taskCenter.Activate();
                    _isOpeningTaskCenter = true;
                    Nav.SelectedItem = Nav.MenuItems[0];
                    FrameView.Content = settingPage;
                    FrameView.Content = launchPage;
                    break;
            }
        };
    }
    public void LoadWindow()
    {
        SystemDecorations = SystemDecorations.Full;

        var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
        FrameView.Content = launchPage;
        titleBarStyle = setting.WindowTitleBarStyle;
        switch (setting.WindowTitleBarStyle)
        {
            case WindowTitleBarStyle.System:
                TitleBar.IsVisible = false;
                TitleRoot.IsVisible = false;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
                ExtendClientAreaToDecorationsHint = false;
                break;
            case WindowTitleBarStyle.Ymcl:
                TitleBar.IsVisible = true;
                TitleRoot.IsVisible = true;
                Root.CornerRadius = new CornerRadius(8);
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
                ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                ExtendClientAreaToDecorationsHint = true;
                break;
        }
        Const.Window.initialize.Hide();
        Const.Window.main.Show();
        Const.Window.initialize.Close();

        if (setting.CustomHomePage == CustomHomePageWay.Local)
        {
            try
            {
                var c = (Control)AvaloniaRuntimeXamlLoader.Load(File.ReadAllText(Const.CustomHomePageXamlDataPath));
                launchPage.CustomPageRoot.Child = c;
            }
            catch (Exception ex)
            {
                Method.Ui.ShowLongException(MainLang.CustomHomePageSourceCodeError, ex);
            }
        }

        downloadPage.curseForgeFetcherPage.SearchModFromCurseForge();
    }
    public async void HandleDrop(object sender, DragEventArgs e)
    {
        DragTip.IsOpen = false;
        if (!e.Data.GetDataFormats().Contains(DataFormats.Files) || null == e.Data.GetFiles()) return;
        var items = e.Data.GetFiles()!.ToList();
        var files = new List<Public.Classes.FileInfo>();
        items.ForEach(item =>
        {
            string path = item.TryGetLocalPath();
            if (Directory.Exists(path))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                System.IO.FileInfo[] files1 = dirInfo.GetFiles();
                foreach (System.IO.FileInfo file in files1)
                {
                    files.Add(Method.IO.GetFileInfoFromPath(file.FullName));
                }
            }
            else if (File.Exists(path))
            {
                files.Add(Method.IO.GetFileInfoFromPath(path));
            }
        });

        if (files.Count == 0) return;
        var jarFile = new List<Public.Classes.FileInfo>();
        var zipFile = new List<Public.Classes.FileInfo>();
        var audioFile = new List<Public.Classes.FileInfo>();
        files.ForEach(file =>
        {
            switch (file.Extension)
            {
                case ".jar":
                    jarFile.Add(file);
                    break;
                case ".zip":
                    zipFile.Add(file);
                    break;
                case ".mp3":
                case ".ogg":
                case ".flac":
                case ".wav":
                    audioFile.Add(file);
                    break;
                default:
                    Method.Ui.Toast($"{MainLang.UnsupportedFileType} - {file.Extension}\n{file.Path}", type: Avalonia.Controls.Notifications.NotificationType.Error);
                    break;
            }
        });
        if (jarFile.Count > 0)
        {
            var entry = launchPage.VersionListView.SelectedItem as GameEntry;
            if (entry.Type == "BedRock")
            {
                Method.Ui.Toast(MainLang.UnableToAddModsForBedrockEdition, type: Avalonia.Controls.Notifications.NotificationType.Error);
                return;
            }
            if (null == entry)
            {
                Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, type: Avalonia.Controls.Notifications.NotificationType.Error);
                return;
            }
            Nav.SelectedItem = Nav.MenuItems[0];
            var text = string.Empty;
            jarFile.ForEach(jar =>
            {
                text += $"{jar.FullName}\n";
            });
            var result = await Method.Ui.ShowDialogAsync(MainLang.AddTheFollowingFilesAsModsToTheCurrentVersion + "?", text, b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
            if (result == ContentDialogResult.Primary)
            {
                Method.IO.TryCreateFolder(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, "mods"));
                jarFile.ForEach(jar =>
                {
                    File.Copy(jar.Path, Path.Combine(Path.GetDirectoryName(entry.JarPath)!, "mods", jar.FullName), true);
                });
                Method.Ui.Toast(MainLang.SuccessAdd, type: Avalonia.Controls.Notifications.NotificationType.Success);
            }
        }
        if (zipFile.Count > 0)
        {
            Nav.SelectedItem = Nav.MenuItems[0];
            var text = string.Empty;
            zipFile.ForEach(zip =>
            {
                text += $"{zip.FullName}\n";
            });
            var result = await Method.Ui.ShowDialogAsync(MainLang.InstallTheFollowingFilesAsAnIntegrationPackageCurseforgeFormat + "?", text, b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
            if (result == ContentDialogResult.Primary)
            {
                foreach (var file in zipFile)
                {
                    var importResult = await Method.Mc.ImportModPack(file.Path);
                    if (!importResult)
                    {
                        Method.Ui.Toast($"{MainLang.ImportFailed}: {file.FullName}", type: Avalonia.Controls.Notifications.NotificationType.Error);
                    }
                    else
                    {
                        Method.Ui.Toast($"{MainLang.ImportSuccess}: {file.FullName}", type: Avalonia.Controls.Notifications.NotificationType.Success);
                    }
                }
            }
        }
        if (audioFile.Count > 0)
        {
            Nav.SelectedItem = Nav.MenuItems[3];
            foreach (var file in audioFile)
            {
                using (var reader = new MediaFoundationReader(file.Path))
                {
                    var time = Method.Value.MsToTime(reader.TotalTime.TotalMilliseconds);
                    var song = new PlaySongListViewItemEntry()
                    {
                        DisplayDuration = time,
                        Duration = reader.TotalTime.TotalMilliseconds,
                        Img = null,
                        SongName = file.Name,
                        Authors = file.Extension.TrimStart('.'),
                        Path = file.Path,
                        Type = PlaySongListViewItemEntryType.Local
                    };
                    musicPage.playSongList.Add(song);
                    musicPage.PlayListView.Items.Add(song);
                }
            }
            File.WriteAllText(Const.PlayerDataPath, JsonConvert.SerializeObject(musicPage.playSongList, Formatting.Indented));
            musicPage.PlayListView.SelectedIndex = musicPage.PlayListView.Items.Count - 1;
        }
    }
}