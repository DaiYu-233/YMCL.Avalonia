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
        Loaded += async (_, _) =>
        {
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            EventBinding();
            FrameView.Content = launchPage;
            titleBarStyle = setting.WindowTitleBarStyle;
            switch (setting.WindowTitleBarStyle)
            {
                case WindowTitleBarStyle.Unset:
                    await Task.Delay(350);
                    TitleBar.IsVisible = false;
                    TitleRoot.IsVisible = false;
                    Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                    ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
                    ExtendClientAreaToDecorationsHint = false;
                    var comboBox = new ComboBox()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                    };
                    comboBox.Items.Add("System");
                    comboBox.Items.Add("Ymcl");
                    comboBox.SelectedIndex = 0;
                    ContentDialog dialog = new()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Title = MainLang.WindowTitleBarStyle,
                        PrimaryButtonText = MainLang.Ok,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = comboBox
                    };
                    comboBox.SelectionChanged += (_, _) =>
                    {
                        if (comboBox.SelectedIndex == 0)
                        {
                            TitleBar.IsVisible = false;
                            TitleRoot.IsVisible = false;
                            Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
                            ExtendClientAreaToDecorationsHint = false;
                        }
                        else
                        {
                            TitleBar.IsVisible = true;
                            TitleRoot.IsVisible = true;
                            Root.CornerRadius = new CornerRadius(8);
                            WindowState = WindowState.Maximized;
                            WindowState = WindowState.Normal;
                            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                            ExtendClientAreaToDecorationsHint = true;
                        }
                    };
                    dialog.PrimaryButtonClick += (_, _) =>
                    {
                        setting.WindowTitleBarStyle = comboBox.SelectedIndex == 0 ? WindowTitleBarStyle.System : WindowTitleBarStyle.Ymcl;
                        File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                    };
                    await dialog.ShowAsync();
                    break;
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
            if (!setting.AlreadyWrittenIntoTheUrlScheme)
            {
                switch (Const.Platform)
                {
                    case Platform.Windows:
                        await Method.Ui.UpgradeToAdministratorPrivilegesAsync();
                        Method.IO.CreateFolder("C:\\ProgramData\\DaiYu.Platform.YMCL");
                        var bat = "set /p ymcl=<%USERPROFILE%\\AppData\\Roaming\\DaiYu.Platform.YMCL\\YMCL.AppPath.DaiYu\r\necho %ymcl%\r\necho %1\r\nstart %ymcl% %1";
                        var path = "C:\\ProgramData\\DaiYu.Platform.YMCL\\launch.bat";
                        File.WriteAllText(path, bat);
                        try { Registry.ClassesRoot.DeleteSubKey("YMCL"); } catch { }
                        try
                        {
                            RegistryKey keyRoot = Registry.ClassesRoot.CreateSubKey("YMCL", true);
                            keyRoot.SetValue("", "Yu Minecraft Launcher");
                            keyRoot.SetValue("URL Protocol", path);
                            RegistryKey registryKeya = Registry.ClassesRoot.OpenSubKey("YMCL", true).CreateSubKey("DefaultIcon");
                            registryKeya.SetValue("", path);
                            RegistryKey registryKeyb = Registry.ClassesRoot.OpenSubKey("YMCL", true).CreateSubKey(@"shell\open\command");
                            registryKeyb.SetValue("", $"\"{path}\" \"%1\"");

                            string resourceName = "YMCL.Main.Public.Bins.YMCL.Starter.win.exe";
                            Assembly assembly = Assembly.GetExecutingAssembly();
                            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                            {
                                string outputFilePath = "C:\\Windows\\ymcl.exe";
                                using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                                {
                                    resourceStream.CopyTo(fileStream);
                                }
                            }
                            setting.AlreadyWrittenIntoTheUrlScheme = true;
                            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                        }
                        catch { }
                        break;
                }
            }
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
}