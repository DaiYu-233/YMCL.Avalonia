using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;


namespace YMCL.Main.Views.Initialize
{
    public partial class InitializeWindow : Window
    {
        private void Init()
        {
            Method.IO.CreateFolder(Const.UserDataRootPath);
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
                Method.IO.CreateFolder(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft"));
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
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.Language == null || setting.Language == "zh-CN")
            {
                LangHelper.Current.ChangedCulture("");
            }
            else
            {
                LangHelper.Current.ChangedCulture(setting.Language);
            }

            Const.Notification.main = new WindowNotificationManager(GetTopLevel(Const.Window.main)) { MaxItems = 3, Position = NotificationPosition.BottomRight, /*FontFamily = (FontFamily)Application.Current.Resources["Font"]*/ };
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
            Init();
            DetectPlatform();
            InitializeComponent();
            Const.Window.main.Show();
            Loaded += (_, _) =>
            {
                Close();
            };
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
    }
}
