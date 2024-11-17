using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;
using PluginInfo = YMCL.Main.Public.Controls.PluginInfo;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Plugin;

public partial class PluginSettingPage : UserControl
{
    private bool _firstLoad = true;

    public PluginSettingPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);

            if (_firstLoad)
                _firstLoad = false;
            else
                ReloadPluginListUi();
        };
    }

    private static void ControlProperty()
    {
    }

    public void LoadPlugin()
    {
        var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
        var list1 = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
        var directoryInfo = new DirectoryInfo(Const.String.PluginFolderPath);
        var dlls = directoryInfo.GetFiles();
        var paths = dlls.Select(item => item.FullName).ToList();
        list1.ForEach(x =>
        {
            if (!paths.Contains(x)) list.Remove(x);
        });
        File.WriteAllText(Const.String.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
        foreach (var item in dlls)
            try
            {
                var asm = Assembly.LoadFrom(item.FullName);
                var type = asm.GetType("YMCL.Plugin.Main");
                if (type == null || !typeof(IPlugin).IsAssignableFrom(type)) continue;
                if (Activator.CreateInstance(type) is not IPlugin instance) continue;
                var protocolInfo = instance.GetPluginInformation();
                var control = new PluginInfo
                {
                    PluginName =
                    {
                        Text = protocolInfo.Name
                    },
                    PluginPath =
                    {
                        Text = item.FullName
                    },
                    PluginAuthor =
                    {
                        Text = protocolInfo.Author
                    },
                    PluginDescription =
                    {
                        Text = protocolInfo.Description
                    },
                    PluginVersion =
                    {
                        Text = protocolInfo.Version
                    },
                    PluginTime =
                    {
                        Text = protocolInfo.Time.ToString("yyyy-MM-ddTHH:mm:sszzz")
                    }
                };
                Container.Children.Add(control);

                if (!list.Contains(item.FullName)) continue;
                control.PluginSwitch.IsChecked = true;
                var onLoadMethod = type.GetMethod("OnLoad");
                if (onLoadMethod != null)
                {
                    _ = Task.Run(() => { Dispatcher.UIThread.Invoke(() => { onLoadMethod.Invoke(instance, null); }); });
                }
            }
            catch
            {
            }
    }

    public void ReloadPluginListUi()
    {
        Container.Children.Clear();
        var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
        var list1 = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
        var directoryInfo = new DirectoryInfo(Const.String.PluginFolderPath);
        var dlls = directoryInfo.GetFiles();
        var paths = new List<string>();
        foreach (var item in dlls) paths.Add(item.FullName);
        list1.ForEach(x =>
        {
            if (!paths.Contains(x)) list.Remove(x);
        });
        File.WriteAllText(Const.String.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
        foreach (var item in dlls)
            try
            {
                var asm = Assembly.LoadFrom(item.FullName);
                var type = asm.GetType("YMCL.Plugin.Main");
                if (!typeof(IPlugin).IsAssignableFrom(type)) continue;
                var instance = Activator.CreateInstance(type) as IPlugin;
                var protocolInfo = instance.GetPluginInformation();
                var control = new PluginInfo
                {
                    PluginName =
                    {
                        Text = protocolInfo.Name
                    },
                    PluginPath =
                    {
                        Text = item.FullName
                    },
                    PluginAuthor =
                    {
                        Text = protocolInfo.Author
                    },
                    PluginDescription =
                    {
                        Text = protocolInfo.Description
                    },
                    PluginVersion =
                    {
                        Text = protocolInfo.Version
                    },
                    PluginTime =
                    {
                        Text = protocolInfo.Time.ToString("yyyy-MM-ddTHH:mm:sszzz")
                    }
                };
                Container.Children.Add(control);
                if (list.Contains(item.FullName)) control.PluginSwitch.IsChecked = true;
            }
            catch
            {
            }
    }
}