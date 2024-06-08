using Avalonia.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Views.Main;

namespace YMCL.Main.Views.Initialize
{
    public partial class InitializeWindow : Window
    {
        MainWindow mainWindow = new();
        public InitializeWindow()
        {
            InitializeComponent();
            DetectPlatform();
            mainWindow.Show();
            Loaded += (_, _) => { Close(); };
        }
        public void DetectPlatform()
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
