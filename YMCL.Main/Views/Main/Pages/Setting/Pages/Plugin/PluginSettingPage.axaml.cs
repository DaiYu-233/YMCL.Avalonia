using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
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

    private void ControlProperty()
    {
    }

    public void LoadPlugin()
    {
        var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.PluginDataPath));
        var list1 = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.PluginDataPath));
        var directoryInfo = new DirectoryInfo(Const.PluginFolderPath);
        var dlls = directoryInfo.GetFiles();
        var paths = new List<string>();
        foreach (var item in dlls) paths.Add(item.FullName);
        list1.ForEach(x =>
        {
            if (!paths.Contains(x)) list.Remove(x);
        });
        File.WriteAllText(Const.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
        foreach (var item in dlls)
            try
            {
                var asm = Assembly.LoadFrom(item.FullName);
                var manifestModuleName = asm.ManifestModule.ScopeName;
                var type = asm.GetType("YMCL.Plugin.Main");
                if (!typeof(IPlugin).IsAssignableFrom(type))
                {
                    Console.WriteLine("¦Ä??§Ó?????");
                    continue;
                }

                var instance = Activator.CreateInstance(type) as IPlugin;
                var protocolInfo = instance.GetPluginInformation();
                var control = new PluginInfo();
                control.PluginName.Text = protocolInfo.Name;
                control.PluginPath.Text = item.FullName;
                control.PluginAuthor.Text = protocolInfo.Author;
                control.PluginDescription.Text = protocolInfo.Description;
                control.PluginVersion.Text = protocolInfo.Version;
                control.PluginTime.Text = protocolInfo.Time.ToString("yyyy-MM-ddTHH:mm:sszzz");
                Container.Children.Add(control);
                if (list.Contains(item.FullName))
                {
                    control.PluginSwitch.IsChecked = true;
                    _ = Task.Run(() => { instance.OnLoad(); });
                }
            }
            catch
            {
            }
    }

    public void ReloadPluginListUi()
    {
        Container.Children.Clear();
        var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.PluginDataPath));
        var list1 = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.PluginDataPath));
        var directoryInfo = new DirectoryInfo(Const.PluginFolderPath);
        var dlls = directoryInfo.GetFiles();
        var paths = new List<string>();
        foreach (var item in dlls) paths.Add(item.FullName);
        list1.ForEach(x =>
        {
            if (!paths.Contains(x)) list.Remove(x);
        });
        File.WriteAllText(Const.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
        foreach (var item in dlls)
            try
            {
                var asm = Assembly.LoadFrom(item.FullName);
                var manifestModuleName = asm.ManifestModule.ScopeName;
                var type = asm.GetType("YMCL.Plugin.Main");
                if (!typeof(IPlugin).IsAssignableFrom(type))
                {
                    Console.WriteLine("¦Ä??§Ó?????");
                    continue;
                }

                var instance = Activator.CreateInstance(type) as IPlugin;
                var protocolInfo = instance.GetPluginInformation();
                var control = new PluginInfo();
                control.PluginName.Text = protocolInfo.Name;
                control.PluginPath.Text = item.FullName;
                control.PluginAuthor.Text = protocolInfo.Author;
                control.PluginDescription.Text = protocolInfo.Description;
                control.PluginVersion.Text = protocolInfo.Version;
                control.PluginTime.Text = protocolInfo.Time.ToString("yyyy-MM-ddTHH:mm:sszzz");
                Container.Children.Add(control);
                if (list.Contains(item.FullName)) control.PluginSwitch.IsChecked = true;
                instance = null;
            }
            catch
            {
            }
    }
}