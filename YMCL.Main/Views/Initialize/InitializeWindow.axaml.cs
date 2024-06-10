using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using YMCL.Main.Public;
using YMCL.Main.Views.Main;

namespace YMCL.Main.Views.Initialize
{
    public partial class InitializeWindow : Window
    {
        private void Init()
        {
            Method.SetAccentColor(Color.Parse("#0dc0a5"));
            Method.ToggleTheme(Public.Theme.Light);
            Method.CreateFolder(Const.UserDataRootPath);
            if (!File.Exists(Const.MinecraftFolderDataPath) || JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath)).Count == 0)
            {
                Method.CreateFolder(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft"));
                File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(new List<string>() { Path.Combine(Process.GetCurrentProcess().MainModule.FileName, ".minecraft") }, Formatting.Indented));
            }
        }

        public InitializeWindow()
        {
            InitializeComponent();
            DetectPlatform();
            Init();
            MainWindow mainWindow = new();
            mainWindow.Show();
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
