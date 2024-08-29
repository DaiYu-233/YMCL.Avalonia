using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using YMCL.Main.Public;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Account;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Download;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Launch;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Personalize;
using YMCL.Main.Views.Main.Pages.Setting.Pages.Plugin;

namespace YMCL.Main.Views.Main.Pages.Setting;

public partial class SettingPage : UserControl
{
    public AccountSettingPage accountSettingPage = new();
    public LauncherSettingPage launcherSettingPage = new();
    public LaunchSettingPage launchSettingPage = new();
    public PersonalizeSettingPage personalizeSettingPage = new();
    public PluginSettingPage pluginSettingPage = new();
    public DownloadSettingPage downloadSettingPage = new();

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
        Nav.SelectionChanged += async (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "launch":
                    launchSettingPage.Root.IsVisible = false;
                    FrameView.Content = launchSettingPage;
                    break;
                case "personalize":
                    personalizeSettingPage.Root.IsVisible = false;
                    FrameView.Content = personalizeSettingPage;
                    break;
                case "account":
                    accountSettingPage.Root.IsVisible = false;
                    FrameView.Content = accountSettingPage;
                    break;
                case "launcher":
                    launcherSettingPage.Root.IsVisible = false;
                    FrameView.Content = launcherSettingPage;
                    break;
                case "plugin":
                    pluginSettingPage.Root.IsVisible = false;
                    FrameView.Content = pluginSettingPage;
                    break;
                case "download":
                    downloadSettingPage.Root.IsVisible = false;
                    FrameView.Content = downloadSettingPage;
                    break;
            }
            _ = Const.Window.main.FocusButton();
        };
    }
}