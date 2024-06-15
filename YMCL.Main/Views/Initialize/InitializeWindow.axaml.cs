using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using YMCL.Main.Public;


namespace YMCL.Main.Views.Initialize
{
    public partial class InitializeWindow : Window
    {
        private void Init()
        {
            Method.CreateFolder(Const.UserDataRootPath);
            if (!File.Exists(Const.SettingDataPath))
            {
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(new Public.Classes.Setting(),Formatting.Indented));
            }
            if (!File.Exists(Const.MinecraftFolderDataPath) || JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath)).Count == 0)
            {
                Method.CreateFolder(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft"));
                File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(new List<string>() { Path.Combine(Process.GetCurrentProcess().MainModule.FileName, ".minecraft") }, Formatting.Indented));
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                setting.MinecraftFolder = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft");
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
            if (!File.Exists(Const.JavaDataPath))
            {
                File.WriteAllText(Const.JavaDataPath, JsonConvert.SerializeObject(new List<JavaEntry>(), Formatting.Indented));
            }

            Const.notification = new WindowNotificationManager(GetTopLevel(Const.Window.main)) { MaxItems = 5, Position = NotificationPosition.BottomRight, /*FontFamily = (FontFamily)Application.Current.Resources["Font"]*/ };
            Method.SetAccentColor(Color.Parse("#0dc0a5"));
            Method.ToggleTheme(Public.Theme.Light);
        }

        public InitializeWindow()
        {
            InitializeComponent();
            Init();
            DetectPlatform();
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
