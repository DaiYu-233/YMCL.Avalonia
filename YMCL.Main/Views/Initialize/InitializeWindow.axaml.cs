using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using Microsoft.Win32;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Main;

namespace YMCL.Main.Views.Initialize;

public partial class InitializeWindow : Window
{
    public WindowTitleBarStyle titleBarStyle;
    public bool _exit = true;

    public InitializeWindow()
    {
        DetectPlatform();
        Init();
        InitializeComponent();
        EventBinding();
    }

    private void EventBinding()
    {
        PropertyChanged += (s, e) =>
        {
            if (titleBarStyle != WindowTitleBarStyle.Ymcl || e.Property.Name != nameof(WindowState)) return;
            Root.Margin = WindowState switch
            {
                WindowState.Normal => new Thickness(0),
                WindowState.Maximized => new Thickness(20),
                _ => Root.Margin
            };
        };
    }

    private void Init()
    {
        Method.IO.TryCreateFolder(Const.String.UserDataRootPath);
        Method.IO.TryCreateFolder(Const.String.PluginFolderPath);
        Method.IO.TryCreateFolder(Const.String.TempFolderPath);
        Method.IO.TryCreateFolder(Const.String.UpdateFolderPath);
        if (!File.Exists(Const.String.SettingDataPath))
            File.WriteAllText(Const.String.SettingDataPath,
                JsonConvert.SerializeObject(new Setting(), Formatting.Indented));
        if (!File.Exists(Const.String.MinecraftFolderDataPath) || JsonConvert
                .DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath)).Count == 0)
        {
            var path = Path.Combine(Const.String.UserDataRootPath, ".minecraft");
            Method.IO.TryCreateFolder(path);
            File.WriteAllText(Const.String.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(new List<string> { path }, Formatting.Indented));
            var setting1 = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.String.SettingDataPath));
            setting1.MinecraftFolder = path;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting1, Formatting.Indented));
        }

        Const.Data.Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.String.SettingDataPath));
        if (!File.Exists(Const.String.JavaDataPath))
            File.WriteAllText(Const.String.JavaDataPath,
                JsonConvert.SerializeObject(new List<JavaEntry>(), Formatting.Indented));
        if (!File.Exists(Const.String.PluginDataPath))
            File.WriteAllText(Const.String.PluginDataPath,
                JsonConvert.SerializeObject(new List<string>(), Formatting.Indented));
        if (!File.Exists(Const.String.JavaNewsDataPath))
            File.WriteAllText(Const.String.JavaNewsDataPath,
                "{}");
        if (!File.Exists(Const.String.PlayerDataPath))
            File.WriteAllText(Const.String.PlayerDataPath,
                JsonConvert.SerializeObject(new List<PlaySongListViewItemEntry>(), Formatting.Indented));
        if (!File.Exists(Const.String.AccountDataPath))
            File.WriteAllText(Const.String.AccountDataPath,
                JsonConvert.SerializeObject(
                    new List<AccountInfo>
                    {
                        new()
                        {
                            Name = "Steve", AccountType = AccountType.Offline,
                            AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                        }
                    }, Formatting.Indented));
        if (!File.Exists(Const.String.CustomHomePageXamlDataPath))
        {
            var resourceName = "YMCL.Main.Public.Texts.CustomHomePageDefault.axaml";
            var _assembly = Assembly.GetExecutingAssembly();
            var stream = _assembly.GetManifestResourceStream(resourceName);
            using (var reader = new StreamReader(stream!))
            {
                var result = reader.ReadToEnd();
                File.WriteAllText(Const.String.CustomHomePageXamlDataPath, result);
            }
        }

        File.WriteAllText(Const.String.AppPathDataPath, Process.GetCurrentProcess().MainModule.FileName!);
        if (Const.Data.Platform == Platform.Linux)
        {
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME"))) return;
                var path = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".local/share/applications");
                File.WriteAllText(Path.Combine(path, "YMCL.desktop"),
                    $"[Desktop Entry]  \r\nVersion=1.0  \r\nName=YMCL Protocol Handler  \r\nComment=Handle ymcl:// URLs  \r\nExec={Process.GetCurrentProcess().MainModule.FileName!}\r\nTerminal=true  \r\nType=Application  \r\nCategories=Network;  \r\nMIMEType=x-scheme-handler/ymcl;  ");
            }
            catch
            {
            }
        }

        var setting = Const.Data.Setting;
        if (setting.Language == null || setting.Language == "zh-CN")
            LangHelper.Current.ChangedCulture("");
        else
            LangHelper.Current.ChangedCulture(setting.Language);

        var window = new MainWindow();
        Const.Window.main = window;
        Const.Notification.main = new WindowNotificationManager(GetTopLevel(window))
        {
            MaxItems = 3, MaxHeight = 99999, FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
            CornerRadius = new CornerRadius(8), FontSize = 14, Margin = new Thickness(0),
            Position = NotificationPosition.BottomRight
        };
        Const.Notification.initialize = new WindowNotificationManager(GetTopLevel(this))
        {
            MaxItems = 2, MaxHeight = 99999, FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
            CornerRadius = new CornerRadius(8), FontSize = 14, Margin = new Thickness(0),
            Position = NotificationPosition.BottomRight
        };
        Method.Ui.SetAccentColor(setting.AccentColor);
        if (setting.Theme == Public.Theme.Light)
            Method.Ui.ToggleTheme(Public.Theme.Light);
        else if (setting.Theme == Public.Theme.Dark) Method.Ui.ToggleTheme(Public.Theme.Dark);
    }

    public static void DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Running on Windows");
            Const.Data.Platform = Platform.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("Running on Linux");
            Const.Data.Platform = Platform.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("Running on macOS");
            Const.Data.Platform = Platform.MacOs;
        }
        else
        {
            Console.WriteLine("Running on an unknown platform");
            Const.Data.Platform = Platform.Unknown;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_exit)
        {
            Environment.Exit(0);
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        YMCL.Main.Views.Initialize.Pages.Main.Main mianPage = new();
        Frame.Content = mianPage;
        var setting = Const.Data.Setting;
        mianPage.WindowTitleBarStyleListBox.SelectedIndex = setting.WindowTitleBarStyle switch
        {
            WindowTitleBarStyle.System => 0,
            WindowTitleBarStyle.Ymcl => 1,
            _ => 0
        };

        Hide();
        base.OnLoaded(e);
        Hide();
        switch (setting.WindowTitleBarStyle)
        {
            case WindowTitleBarStyle.Unset:
            case WindowTitleBarStyle.System:
                TitleBar.IsVisible = false;
                MaxHeight = 480 - 30;
                MinHeight = 480 - 30;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                ExtendClientAreaToDecorationsHint = false;
                break;
            case WindowTitleBarStyle.Ymcl:
                TitleBar.IsVisible = true;
                MaxHeight = 480;
                MinHeight = 480;
                Root.CornerRadius = new CornerRadius(8);
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                ExtendClientAreaToDecorationsHint = true;
                break;
        }
    }
}