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

        public async void Dispose()
        {
            //PluginBehavior
            var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
            a.Content = "Plugin Test";
        }
    };
}

