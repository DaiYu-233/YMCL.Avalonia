using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main;

public partial class MainWindow : Window
{
    readonly Pages.Download.DownloadPage downloadPage = new();
    readonly Pages.Launch.LaunchPage launchPage = new();
    readonly Pages.More.MorePage morePage = new();
    readonly Pages.Music.MusicPage musicPage = new();
    readonly Pages.Setting.SettingPage settingPage = new();
    public WindowTitleBarStyle titleBarStyle;

    public MainWindow()
    {
        InitializeComponent();
        EventBinding();
    }
    private void EventBinding()
    {
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
        Nav.SelectionChanged += (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "Launch":
                    launchPage.Root.IsVisible = false;
                    FrameView.Content = launchPage; break;
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
        Show();
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
    }
}