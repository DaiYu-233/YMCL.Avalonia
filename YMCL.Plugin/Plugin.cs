using Avalonia.Controls;
using Avalonia.VisualTree;
using YMCL.Main.Public;
using static YMCL.Main.Public.Plugin;

namespace YMCL.Plugin
{
    public class PluginRoot : IPlugin
    {
        public PluginInfo GetPluginInformation()
        {
            //PluginInfo
            return new PluginInfo()
            {
                Author = "DaiYu awa",
                Name = "Test Plugin",
                Version = "1.3.0",
                Description = "This a plugin for ymcl QAQ",
                Time = new DateTime(1970, 1, 1, 0, 0, 0)
            };
        }

        public void Dispose()
        {
            //PluginBehavior
            var a = Const.Window.main.launchPage.GetControl<Button>(name:"VersionListBtn");
            a.Content = "Plugin Test";
        }
    };
}

