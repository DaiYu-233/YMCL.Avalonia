using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher
{
    public partial class LauncherSettingPage : UserControl
    {
        public LauncherSettingPage()
        {
            InitializeComponent();
            ControlProperty();
            BindingEvent();
        }

        private void BindingEvent()
        {
            Loaded += (s, e) =>
            {
                Method.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            };
            OpenUserDataFolderBtn.Click += async (s, e) =>
            {
                var launcher = TopLevel.GetTopLevel(this).Launcher;
                await launcher.LaunchDirectoryInfoAsync(new System.IO.DirectoryInfo(Const.UserDataRootPath));
            };
        }

        private void ControlProperty()
        {
            UserDataFolderPath.Text = Const.UserDataRootPath;
            var userDataSize = Math.Round(Method.GetDirectoryLength(Const.UserDataRootPath) / 1024, 2) >= 512 ? $"{Math.Round(Method.GetDirectoryLength(Const.UserDataRootPath) / 1024 / 1024, 2)} Mib" : $"{Math.Round(Method.GetDirectoryLength(Const.UserDataRootPath) / 1024, 2)} Kib";
            UserDataSize.Text = userDataSize;
        }
    }
}
