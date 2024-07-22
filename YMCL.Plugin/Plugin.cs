using Avalonia.Controls;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Plugin_Test
{
    public class Plugin_DaiYu : IPlugin
    {
        public PluginInfo GetPluginInformation()
        {
            //PluginInfo
            return new PluginInfo()
            {
                Author = "DaiYu",
                Name = "Test-Plugin",
                Version = "1.3.0",
                Description = "This A Plugin of YMCL.",
                Time = new DateTime(1970, 1, 1, 0, 0, 0)
            };
        }

        public void OnDisable()
        {
            //When the plugin switch is turned off
            Method.Ui.Toast("Plugin Off");
        }

        public void OnEnable()
        {
            //When the plugin switch is turned on
            Method.Ui.Toast("Plugin On");
        }

        public void OnLoad()
        {
            //PluginBehavior
            //When the app startup and the plugin is enabled
            var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
            a.Content = "Plugin Test";
        }
    };
}

