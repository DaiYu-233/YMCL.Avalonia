using Avalonia.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Public.Controls
{
    public partial class PluginInfo : UserControl
    {
        public PluginInfo()
        {
            InitializeComponent();
            PluginSwitch.Click += (_, _) =>
            {
                Method.Ui.Toast(MainLang.NeedRestartApp, type: Avalonia.Controls.Notifications.NotificationType.Warning);
                var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.PluginDataPath));
                if (PluginSwitch.IsChecked == true)
                {
                    list.Add(PluginPath.Text!);
                }
                else
                {
                    list.Remove(PluginPath.Text!);
                }
                File.WriteAllText(Const.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
            };
        }
    }
}
