using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using YMCL.Main.Public;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Personalize;

namespace YMCL.Main.Views.Main.Pages.Setting
{
    public partial class SettingPage : UserControl
    {
        Pages.Launch.LaunchSettingPage launchSettingPage = new();
        PersonalizeSettingPage personalizeSettingPage = new();
        Pages.Account.AccountSettingPage accountSettingPage = new();
        Pages.Launcher.LauncherSettingPage launcherSettingPage = new();
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
                Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            };
            Nav.SelectionChanged += (s, e) =>
            {
                switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
                {
                    case "launch":
                        launchSettingPage.Root.IsVisible = false;
                        FrameView.Content = launchSettingPage; break;
                    case "personalize":
                        personalizeSettingPage.Root.IsVisible = false;
                        FrameView.Content = personalizeSettingPage; break;
                    case "account":
                        accountSettingPage.Root.IsVisible = false;
                        FrameView.Content = accountSettingPage; break;
                    case "launcher":
                        launcherSettingPage.Root.IsVisible = false;
                        FrameView.Content = launcherSettingPage; break;
                }
            };
        }
    }
}
