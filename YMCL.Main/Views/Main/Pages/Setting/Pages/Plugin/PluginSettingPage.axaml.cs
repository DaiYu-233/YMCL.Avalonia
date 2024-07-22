using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Plugin
{
    public partial class PluginSettingPage : UserControl
    {
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
            };
        }
        private void ControlProperty()
        {

        }
        public void LoadPlugin()
        {
            var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.PluginDataPath));
            DirectoryInfo directoryInfo = new DirectoryInfo(Const.PluginFolderPath);
            var dlls = directoryInfo.GetFiles();
            foreach (var item in dlls)
            {
                try
                {
                    Assembly asm = Assembly.LoadFrom(item.FullName);
                    var manifestModuleName = asm.ManifestModule.ScopeName;
                    Type type = asm.GetType("YMCL.Plugin.PluginRoot");
                    if (!typeof(IPlugin).IsAssignableFrom(type))
                    {
                        Debug.WriteLine("Î´¼Ì³Ð²å¼þ½Ó¿Ú");
                        continue;
                    }
                    var instance = Activator.CreateInstance(type) as IPlugin;
                    var protocolInfo = instance.GetPluginInformation();
                    var control = new Public.Controls.PluginInfo();
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
                        instance.Dispose();
                    }
                    instance = null;
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}
