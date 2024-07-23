using Avalonia.Controls;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Plugin  //Do not modify
{
    public class Main : IPlugin //Do not modify
    {
        public PluginInfo GetPluginInformation()
        {
            //PluginInfo
            return new PluginInfo()
            {
                Author = "DaiYu",//plugin author
                Name = "Test Plugin",//plugin name
                Version = "1.3.0",//plugin version
                Description = "This A Plugin of YMCL.",//Plugin Description
                Time = new DateTime(1970, 1, 1, 0, 0, 0)//Plugin release time, in the format of year month day hour minute second
            };
        }

        public void OnLoad()
        {
            //PluginBehavior
            //Triggered when the program is opened
            //In this example, change the display text of the "Version List" button on the main interface to "Plugin Test". The specific method can be found by browsing the source code
            var a = Const.Window.main.launchPage.GetControl<Button>(name: "VersionListBtn");
            a.Content = "Plugin Test";
        }

        public void OnDisable()
        {
            //When the plugin switch is turned off
            //In this example, a message box pops up to prompt the user
            Method.Ui.Toast("Plugin Off");
        }

        public void OnEnable()
        {
            //When the plugin switch is turned on
            //In this example, a message box pops up to prompt the user
            Method.Ui.Toast("Plugin On");
        }
    };
}