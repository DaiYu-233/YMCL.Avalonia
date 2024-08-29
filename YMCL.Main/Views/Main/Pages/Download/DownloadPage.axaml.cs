using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using YMCL.Main.Public;
using YMCL.Main.Views.Main.Pages.Download.Pages.AutoInstall;
using YMCL.Main.Views.Main.Pages.Download.Pages.CurseForgeFetcher;

namespace YMCL.Main.Views.Main.Pages.Download;

public partial class DownloadPage : UserControl
{
    public AutoInstallPage autoInstallPage = new();
    public CurseForgeFetcher curseForgeFetcherPage = new();

    public DownloadPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void ControlProperty()
    {
        FrameView.Content = autoInstallPage;
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
        };
        Nav.SelectionChanged += (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "auto":
                    autoInstallPage.Root.IsVisible = false;
                    FrameView.Content = autoInstallPage;
                    break;
                case "curseforgefetcher":
                    curseForgeFetcherPage.Root.IsVisible = false;
                    FrameView.Content = curseForgeFetcherPage;
                    break;
            }
            _ = Const.Window.main.FocusButton();
        };
    }
}