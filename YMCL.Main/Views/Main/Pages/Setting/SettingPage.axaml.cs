using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting
{
    public partial class SettingPage : UserControl
    {
        Pages.Launch.LaunchSettingPage launchSettingPage = new();
        Pages.Launcher.LauncherSettingPage launcherSettingPage = new();
        Pages.Account.AccountSettingPage accountSettingPage = new();
        public SettingPage()
        {
            InitializeComponent();
            ControlProperty();
            BindingEvent();
        }

        private void ControlProperty()
        {
            FrameView.Content = launchSettingPage;
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
                    case "launch":
                        launchSettingPage.Root.IsVisible = false;
                        FrameView.Content = launchSettingPage; break;
                    case "launcher":
                        launcherSettingPage.Root.IsVisible = false;
                        FrameView.Content = launcherSettingPage; break;
                    case "account":
                        accountSettingPage.Root.IsVisible = false;
                        FrameView.Content = accountSettingPage; break;
                }
            };
        }
    }
}
