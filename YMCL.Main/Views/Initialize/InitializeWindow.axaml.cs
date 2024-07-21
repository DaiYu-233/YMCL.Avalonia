using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Microsoft.Win32;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Main.Pages.Launch;


namespace YMCL.Main.Views.Initialize
{
    public partial class InitializeWindow : Window
    {
        public WindowTitleBarStyle titleBarStyle;
        private void Init()
        {
            Method.IO.TryCreateFolder(Const.UserDataRootPath);
            if (!File.Exists(Const.SettingDataPath))
            {
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(new Setting(), Formatting.Indented));
            }
            if (File.Exists(Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.win.exe")))
            {
                File.Delete(Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.win.exe"));
            }
            if (File.Exists(Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.linux")))
            {
                File.Delete(Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.linux"));
            }
            if (File.Exists(Path.Combine(Const.UserDataRootPath, "Update.exe")))
            {
                File.Delete(Path.Combine(Const.UserDataRootPath, "Update.exe"));
            }
            if (File.Exists(Path.Combine(Const.UserDataRootPath, "Update")))
            {
                File.Delete(Path.Combine(Const.UserDataRootPath, "Update"));
            }
            if (!File.Exists(Const.MinecraftFolderDataPath) || JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath)).Count == 0)
            {
                Method.IO.TryCreateFolder(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft"));
                File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(new List<string>() { Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft") }, Formatting.Indented));
                var setting1 = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
                setting1.MinecraftFolder = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft");
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting1, Formatting.Indented));
            }
            if (!File.Exists(Const.JavaDataPath))
            {
                File.WriteAllText(Const.JavaDataPath, JsonConvert.SerializeObject(new List<JavaEntry>(), Formatting.Indented));
            }
            if (!File.Exists(Const.PlayerDataPath))
            {
                File.WriteAllText(Const.PlayerDataPath, JsonConvert.SerializeObject(new List<PlaySongListViewItemEntry>(), Formatting.Indented));
            }
            if (!File.Exists(Const.AccountDataPath))
            {
                File.WriteAllText(Const.AccountDataPath, JsonConvert.SerializeObject(new List<AccountInfo>() { new AccountInfo { Name = "Steve", AccountType = AccountType.Offline, AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz") } }, Formatting.Indented));
            }
            if (!File.Exists(Const.CustomHomePageXamlDataPath))
            {
                string resourceName = "YMCL.Main.Public.Texts.CustomHomePageDefault.axaml";
                Assembly _assembly = Assembly.GetExecutingAssembly();
                Stream stream = _assembly.GetManifestResourceStream(resourceName);
                using (StreamReader reader = new StreamReader(stream!))
                {
                    string result = reader.ReadToEnd();
                    File.WriteAllText(Const.CustomHomePageXamlDataPath, result);
                }
            }
            File.WriteAllText(Const.AppPathDataPath, Process.GetCurrentProcess().MainModule.FileName!);
            if (Const.Platform == Platform.Linux) File.WriteAllText(Path.Combine(Const.UserDataRootPath, "launch.sh"), $"\"{Process.GetCurrentProcess().MainModule.FileName!}\"");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"chmod +777 {Path.Combine(Const.UserDataRootPath, "launch.sh")}", // ÷¥––ls√¸¡Ó  
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (Const.Platform == Platform.Linux) using (Process process = Process.Start(startInfo)) ;

            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.Language == null || setting.Language == "zh-CN")
            {
                LangHelper.Current.ChangedCulture("");
            }
            else
            {
                LangHelper.Current.ChangedCulture(setting.Language);
            }

            Const.Notification.main = new WindowNotificationManager(GetTopLevel(Const.Window.main)) { MaxItems = 3, Position = NotificationPosition.BottomRight /*FontFamily = (FontFamily)Application.Current.Resources["Font"]*/ };
            Method.Ui.SetAccentColor(setting.AccentColor);
            if (setting.Theme == Public.Theme.Light)
            {
                Method.Ui.ToggleTheme(Public.Theme.Light);
            }
            else if (setting.Theme == Public.Theme.Dark)
            {
                Method.Ui.ToggleTheme(Public.Theme.Dark);
            }
        }
        public InitializeWindow()
        {
            DetectPlatform();
            Init();
            InitializeComponent();
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
            IdentifyLanguageBtn.Click += (_, _) =>
            {
                foreach (ToggleButton item in Langs.Children)
                {
                    if (item.IsChecked == true)
                    {
                        var lang = (((StackPanel)item.Content).Children[0] as TextBlock).Text;
                        var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                        setting.Language = lang;
                        File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                        Method.Ui.RestartApp();
                        break;
                    }
                }
            };
        }

        private async void OnLoaded()
        {
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            titleBarStyle = setting.WindowTitleBarStyle;
            switch (setting.WindowTitleBarStyle)
            {
                case WindowTitleBarStyle.Unset:
                case WindowTitleBarStyle.System:
                    TitleBar.IsVisible = false;
                    Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                    ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
                    ExtendClientAreaToDecorationsHint = false;
                    break;
                case WindowTitleBarStyle.Ymcl:
                    TitleBar.IsVisible = true;
                    Root.CornerRadius = new CornerRadius(8);
                    WindowState = WindowState.Maximized;
                    WindowState = WindowState.Normal;
                    ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                    ExtendClientAreaToDecorationsHint = true;
                    break;
            }

            if (setting.Language == "Unset") { Show(); return; }
            LanguageRoot.IsVisible = false;

            if (setting.WindowTitleBarStyle == WindowTitleBarStyle.Unset)
            {
                Show();
                await Task.Delay(350);
                TitleBar.IsVisible = false;
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
                        Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                        ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
                        ExtendClientAreaToDecorationsHint = false;
                    }
                    else
                    {
                        TitleBar.IsVisible = true;
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


            }

            if (!setting.AlreadyWrittenIntoTheUrlScheme)
            {
                if (Const.Platform == Platform.Windows)
                {
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    { Show(); }
                    await Method.Ui.UpgradeToAdministratorPrivilegesAsync(Const.Window.initialize);
                    Method.IO.TryCreateFolder("C:\\ProgramData\\DaiYu.Platform.YMCL");
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
                }
                else if (Const.Platform == Platform.Linux)
                {
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME"))) return;
                    var path = Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".local/share/applications");
                    var deskPath = Path.Combine(Const.UserDataRootPath, "launch.sh");
                    File.WriteAllText(Path.Combine(path, "YMCL.desktop"), $"[Desktop Entry]  \r\nVersion=1.0  \r\nName=YMCL Protocol Handler  \r\nComment=Handle ymcl:// URLs  \r\nExec={deskPath}\r\nTerminal=true  \r\nType=Application  \r\nCategories=Network;  \r\nMIMEType=x-scheme-handler/ymcl;  ");
                    setting.AlreadyWrittenIntoTheUrlScheme = true;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));

                }
            }

            Const.Window.main.LoadWindow();
            Hide();
        }
        protected override void OnLoaded(RoutedEventArgs e)
        {
            Hide();
            base.OnLoaded(e);
            Hide();
            SystemDecorations = SystemDecorations.Full;
            OnLoaded();
        }
        public static void DetectPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Debug.WriteLine("Running on Windows");
                Const.Platform = Platform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Debug.WriteLine("Running on Linux");
                Const.Platform = Platform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Debug.WriteLine("Running on macOS");
                Const.Platform = Platform.MacOs;
            }
            else
            {
                Debug.WriteLine("Running on an unknown platform");
                Const.Platform = Platform.Unknown;
            }
        }
        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            foreach (ToggleButton item in Langs.Children)
            {
                item.IsChecked = false;
            }
            ((ToggleButton)sender).IsChecked = true;
        }
    }
}
