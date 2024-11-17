using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Main.Public.Controls;

public partial class PluginInfo : UserControl
{
    public PluginInfo()
    {
        InitializeComponent();
        PluginSwitch.Click += (_, _) =>
        {
            var asm = Assembly.LoadFrom(PluginPath.Text!);
            var type = asm.GetType("YMCL.Plugin.Main");
            var instance = Activator.CreateInstance(type!) as IPlugin;
            var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
            if (PluginSwitch.IsChecked == true)
            {
                list.Add(PluginPath.Text!);
                var method = type.GetMethod("OnEnable");
                if (method != null)
                {
                    _ = Task.Run(() => { Dispatcher.UIThread.Invoke(() => { method.Invoke(instance, null); }); });
                }
            }
            else
            {
                list.Remove(PluginPath.Text!);
                var method = type.GetMethod("OnDisable");
                if (method != null)
                {
                    _ = Task.Run(() => { Dispatcher.UIThread.Invoke(() => { method.Invoke(instance, null); }); });
                }
            }

            File.WriteAllText(Const.String.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
        };
    }
}