using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
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
using Brushes = System.Drawing.Brushes;
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

    public MainWindow()
    {
        InitializeComponent();
        EventBinding();
    }

    private void EventBinding()
    {
        /*Nav.PaneOpened += (_, _) => { NavRoot.Width = Nav.OpenPaneLength; };
        Nav.PaneClosed += (_, _) => { NavRoot.Width = Nav.CompactPaneLength; };*/
        Loaded += async (_, _) =>
        {
            if (!Const.Window.main._firstLoad) return;

            if (_needInit) LoadWindow();
            Nav.IsPaneOpen = true;
            _ = Method.App.AppLoaded();
            Const.Window.main._firstLoad = false;
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

            if (Const.Data.Setting.WindowTitleBarStyle == WindowTitleBarStyle.Ymcl && e.Property.Name == nameof(WindowState))
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
        /*AddHandler(DragDrop.DragLeaveEvent, (_, e_) => { DragTip.IsOpen = false; }, RoutingStrategies.Bubble);
        AddHandler(DragDrop.DragEnterEvent, (_, _) => { DragTip.IsOpen = true; }, RoutingStrategies.Bubble);*/
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
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            NavMusic.IsVisible = false;
        }

        _ = Method.App.MainWindowLoading();
        
        Const.Window.initialize.Hide();
        Const.Window.main.Show();
        /*Const.Window.initialize.Close();*/
    }


    public void LoadCustomHomePage()
    {
        if (Const.Data.Setting.CustomHomePage != CustomHomePageWay.Local) return;
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

    public async void HandleDrop(object sender, DragEventArgs e)
    {
        DragTip.IsOpen = false;
        if (null == e.Data) return;
        var text = e.Data.GetText();
        if (!string.IsNullOrWhiteSpace(text))
        {
            await Method.IO.HandleTextDrop(text);
        }
        var files = e.Data.GetFiles();
        if (files != null)
        {
            await Method.IO.HandleFileDrop(files.ToList());
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