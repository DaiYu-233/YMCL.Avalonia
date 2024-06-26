using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System;
using YMCL.Main.Public;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Account;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Launch;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Personalize;

namespace YMCL.Main.Views.Main.Pages.Download
{
    public partial class DownloadPage : UserControl
    {
        Pages.AutoInstall.AutoInstallPage autoInstallPage = new();
        Pages.Mod.Mod modPage = new();
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
                Method.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            };
            Nav.SelectionChanged += (s, e) =>
            {
                switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
                {
                    case "auto":
                        autoInstallPage.Root.IsVisible = false;
                        FrameView.Content = autoInstallPage; break;
                    case "mod":
                        modPage.Root.IsVisible = false;
                        FrameView.Content = modPage; break;
                }
            };
        }
    }
}
