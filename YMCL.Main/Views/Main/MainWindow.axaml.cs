using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Win32;
using MinecraftLaunch.Classes.Models.Game;
using NAudio.Wave;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Main.Pages.Download;
using YMCL.Main.Views.Main.Pages.Launch;
using YMCL.Main.Views.Main.Pages.More;
using YMCL.Main.Views.Main.Pages.Music;
using YMCL.Main.Views.Main.Pages.Search;
using YMCL.Main.Views.Main.Pages.Setting;
using YMCL.Main.Views.Main.Pages.TaskCenter;
using static YMCL.Main.Public.Classes.PlaySongListViewItemEntry;
using FileInfo = YMCL.Main.Public.Classes.FileInfo;

namespace YMCL.Main.Views.Main;

public partial class MainWindow : Window
{
    public readonly DownloadPage downloadPage = new();
    public readonly LaunchPage launchPage = new();
    public readonly MorePage morePage = new();
    public readonly MusicPage musicPage = new();
    public readonly SettingPage settingPage = new();
    public readonly SearchPage searchPage = new();
    public readonly TaskCenterPage taskCenterPage = new();
    public readonly Pages.Setting.Pages.Launcher.LauncherSettingPage aboutPage = new();
    public WindowTitleBarStyle titleBarStyle;
    public bool _firstLoad = true;
    public bool _needInit = false;

    public event EventHandler AppThemeChanged;

    public MainWindow()
    {
        InitializeComponent();
        EventBinding();
    }

    private void EventBinding()
    {
        Loaded += async (_, _) =>
        {
            if (!Const.Window.main._firstLoad) return;
            Const.Window.main._firstLoad = false;
            Method.Ui.CheckLauncher();
            await Task.Delay(200);
            _ = Const.Window.main.settingPage.launcherSettingPage.AutoUpdate();

            if (_needInit) LoadWindow();
        };
        Activated += (_, _) =>
        {
            if (settingPage.PluginSettingNavControl.IsSelected) settingPage.pluginSettingPage.ReloadPluginListUi();
        };
        TitleText.PointerPressed += (s, e) => { BeginMoveDrag(e); };
        PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == nameof(WindowState))
            {
                if (WindowState == WindowState.Maximized)
                {
                    launchPage.CloseVersionList();
                    launchPage.CloseVersionSetting(null, null);
                }
            }

            if (titleBarStyle == WindowTitleBarStyle.Ymcl && e.Property.Name == nameof(WindowState))
                switch (WindowState)
                {
                    case WindowState.Normal:
                        Root.Margin = new Thickness(0);
                        Root.BorderThickness = new Thickness(0);
                        break;
                    case WindowState.Maximized:
                        Root.Margin = new Thickness(20);
                        Root.BorderThickness = new Thickness(2);
                        break;
                }
        };
        AddHandler(DragDrop.DragLeaveEvent, (_, e_) => { DragTip.IsOpen = false; }, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DragEnterEvent, (_, _) => { DragTip.IsOpen = true; }, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DropEvent, (s, e) => { HandleDrop(s!, e); }, RoutingStrategies.Bubble);
        Nav.SelectionChanged += async (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "Launch":
                    launchPage.Root.IsVisible = false;
                    FrameView.Content = launchPage;
                    break;
                case "Setting":
                    settingPage.Root.IsVisible = false;
                    FrameView.Content = settingPage;
                    break;
                case "Download":
                    downloadPage.Root.IsVisible = false;
                    FrameView.Content = downloadPage;
                    break;
                case "Music":
                    musicPage.Root.IsVisible = false;
                    FrameView.Content = musicPage;
                    break;
                case "More":
                    morePage.Root.IsVisible = false;
                    FrameView.Content = morePage;
                    break;
                case "TaskManage":
                    taskCenterPage.Root.IsVisible = false;
                    FrameView.Content = taskCenterPage;
                    break;
                case "Search":
                    searchPage.Root.IsVisible = false;
                    FrameView.Content = searchPage;
                    break;
                case "About":
                    aboutPage.Root.IsVisible = false;
                    FrameView.Content = aboutPage;
                    break;
            }

            _ = FocusButton();
        };
        // NavTask.PointerPressed += (_, e) =>
        // {
        //     if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        //     {
        //         Const.Window.taskCenter.Show();
        //         Const.Window.taskCenter.Activate();
        //     }
        // };
    }

    public void LoadWindow()
    {
        Method.IO.ClearFolder(Const.String.TempFolderPath);

        SystemDecorations = SystemDecorations.Full;

        var setting = Const.Data.Setting;
        FrameView.Content = launchPage;
        titleBarStyle = setting.WindowTitleBarStyle;
        switch (setting.WindowTitleBarStyle)
        {
            case WindowTitleBarStyle.System:
                TitleBar.IsVisible = false;
                TitleRoot.IsVisible = false;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                ExtendClientAreaToDecorationsHint = false;
                break;
            case WindowTitleBarStyle.Ymcl:
                TitleBar.IsVisible = true;
                TitleRoot.IsVisible = true;
                Root.CornerRadius = new CornerRadius(8);
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                ExtendClientAreaToDecorationsHint = true;
                break;
        }

        Const.Window.initialize.Hide();
        Const.Window.main.Show();
        /*Const.Window.initialize.Close();*/

        _ = FetchJavaNews();
        _ = LoadCustomHomePage();

        _ = downloadPage.curseForgeFetcherPage.InitModFromCurseForge();
        Method.Ui.SetWindowBackGroundImg();
    }

    private async Task FetchJavaNews()
    {
        using var client = new HttpClient();
        var json = await client.GetStringAsync("https://launchercontent.mojang.com/javaPatchNotes.json");
        await File.WriteAllTextAsync(Const.String.JavaNewsDataPath, json);
        // _ = LoadCustomHomePage();
    }

    private async Task LoadCustomHomePage()
    {
        if (Const.Data.Setting.CustomHomePage == CustomHomePageWay.Local)
        {
            try
            {
                var c = (Control)AvaloniaRuntimeXamlLoader.Load(
                    File.ReadAllText(Const.String.CustomHomePageXamlDataPath));
                launchPage.CustomPageRoot.Child = c;
            }
            catch (Exception ex)
            {
                Method.Ui.ShowLongException(MainLang.CustomHomePageSourceCodeError, ex);
            }
        }
        else if (Const.Data.Setting.CustomHomePage == CustomHomePageWay.Presetting_JavaNews)
        {
            try
            {
                var viewer = new ScrollViewer();
                var container = new StackPanel()
                {
                    Spacing = 10
                };
                viewer.Content = container;
                launchPage.CustomPageRoot.Child = viewer;
                var news = JsonConvert.DeserializeObject<MojangJavaNews.Root>(
                    await File.ReadAllTextAsync(Const.String.JavaNewsDataPath));
                news.entries.ForEach(v =>
                    container.Children.Add(new JavaNewsEntry(v.image.url, v.body)));
            }
            catch (Exception ex)
            {
            }
        }
    }

    public async void HandleDrop(object sender, DragEventArgs e)
    {
        DragTip.IsOpen = false;
        if (!e.Data.GetDataFormats().Contains(DataFormats.Files) || null == e.Data.GetFiles()) return;
        var items = e.Data.GetFiles()!.ToList();
        var files = new List<FileInfo>();
        items.ForEach(item =>
        {
            var path = item.TryGetLocalPath();
            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                var files1 = dirInfo.GetFiles();
                foreach (var file in files1) files.Add(Method.IO.GetFileInfoFromPath(file.FullName));
            }
            else if (File.Exists(path))
            {
                files.Add(Method.IO.GetFileInfoFromPath(path));
            }
        });

        if (files.Count == 0) return;
        var jarFile = new List<FileInfo>();
        var zipFile = new List<FileInfo>();
        var audioFile = new List<FileInfo>();
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
                    Method.Ui.Toast($"{MainLang.UnsupportedFileType} - {file.Extension}\n{file.Path}",
                        type: NotificationType.Error);
                    break;
            }
        });
        if (jarFile.Count > 0)
        {
            var entry = launchPage.VersionListView.SelectedItem as GameEntry;
            if (entry.Type == "BedRock")
            {
                Method.Ui.Toast(MainLang.UnableToAddModsForBedrockEdition, type: NotificationType.Error);
                return;
            }

            if (null == entry)
            {
                Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, type: NotificationType.Error);
                return;
            }

            Nav.SelectedItem = NavLaunch;
            var text = string.Empty;
            jarFile.ForEach(jar => { text += $"{jar.FullName}\n"; });
            var result = await Method.Ui.ShowDialogAsync(MainLang.AddTheFollowingFilesAsModsToTheCurrentVersion + "?",
                text, b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
            if (result == ContentDialogResult.Primary)
            {
                Method.IO.TryCreateFolder(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, "mods"));
                jarFile.ForEach(jar =>
                {
                    File.Copy(jar.Path, Path.Combine(Path.GetDirectoryName(entry.JarPath)!, "mods", jar.FullName),
                        true);
                });
                Method.Ui.Toast(MainLang.SuccessAdd, type: NotificationType.Success);
            }
        }

        if (zipFile.Count > 0)
        {
            Nav.SelectedItem = NavLaunch;
            var text = string.Empty;
            zipFile.ForEach(zip => { text += $"{zip.FullName}\n"; });
            var result = await Method.Ui.ShowDialogAsync(
                MainLang.InstallTheFollowingFilesAsAnIntegrationPackageCurseforgeFormat + "?", text,
                b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
            if (result == ContentDialogResult.Primary)
                foreach (var file in zipFile)
                {
                    var importResult = await Method.Mc.ImportModPackFromLocal(file.Path);
                    if (!importResult)
                        Method.Ui.Toast($"{MainLang.ImportFailed}: {file.FullName}", type: NotificationType.Error);
                    else
                        Method.Ui.Toast($"{MainLang.ImportSuccess}: {file.FullName}", type: NotificationType.Success);
                }
        }

        if (audioFile.Count > 0)
        {
            Nav.SelectedItem = NavMusic;
            foreach (var file in audioFile)
                using (var reader = new MediaFoundationReader(file.Path))
                {
                    var time = Method.Value.MsToTime(reader.TotalTime.TotalMilliseconds);
                    var song = new PlaySongListViewItemEntry
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

            File.WriteAllText(Const.String.PlayerDataPath,
                JsonConvert.SerializeObject(musicPage.playSongList, Formatting.Indented));
            musicPage.PlayListView.SelectedIndex = musicPage.PlayListView.Items.Count - 1;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        Environment.Exit(0);
        base.OnClosing(e);
    }

    public async Task FocusButton()
    {
        await Task.Delay(500);
        FocusButton1.Focus();
        await Task.Delay(10);
        FocusButton2.Focus();
    }
}