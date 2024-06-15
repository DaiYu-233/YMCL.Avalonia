using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
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
            OpenFileWayComboBox.SelectionChanged += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                setting.OpenFileWay = (OpenFileWay)OpenFileWayComboBox.SelectedIndex;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            };
        }

        private void ControlProperty()
        {
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            OpenFileWayComboBox.SelectedIndex = (int)setting.OpenFileWay;
        }
    }
}
