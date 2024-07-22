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
        public Pages.Launch.LaunchSettingPage launchSettingPage = new();
        public Pages.Personalize.PersonalizeSettingPage personalizeSettingPage = new();
        public Pages.Account.AccountSettingPage accountSettingPage = new();
        public Pages.Launcher.LauncherSettingPage launcherSettingPage = new();
        public Pages.Plugin.PluginSettingPage pluginSettingPage = new();

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
                    case "plugin":
                        pluginSettingPage.Root.IsVisible = false;
                        FrameView.Content = pluginSettingPage; break;
                }
            };
        }
    }
}
