using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;
using System.Linq;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;

namespace YMCL.Main.Views.Main;

public partial class MainWindow : Window
{
    readonly Pages.Download.DownloadPage downloadPage = new();
    readonly Pages.Launch.LaunchPage launchPage = new();
    readonly Pages.More.MorePage morePage = new();
    readonly Pages.Music.MusicPage musicPage = new();
    readonly Pages.Setting.SettingPage settingPage = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Const.Platform != Platform.Windows)
            {
                TitleBar.IsVisible = false;
                TitleText.IsVisible = false;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
            }
            EventBinding();
            FrameView.Content = launchPage;
            Title = Const.UserDataRootPath;
        };
    }
    private void EventBinding()
    {
        TitleText.PointerPressed += (s, e) =>
        {
            BeginMoveDrag(e);
        };
        PropertyChanged += (s, e) =>
        {
            if (Const.Platform == Platform.Windows && e.Property.Name == nameof(WindowState))
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
            var item = ((NavigationView)s!).SelectedItem!;
            var tag = ((NavigationViewItem)item).Tag;
            switch (tag)
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
}