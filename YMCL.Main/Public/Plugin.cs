using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YMCL.Main.Public
{
    public class Plugin
    {
        public class PluginInfo
        {
            public string Name { set; get; } = "YMCLPlugin";
            public string Version { set; get; } = "1.0.0";
            public string Author { set; get; } = "DaiYu";
            public string Description { set; get; } = "A Plugin of YMCL";
            public DateTime Time { set; get; } = DateTime.Now;
        }
        public interface IPlugin : IDisposable
        {
            PluginInfo GetPluginInformation();
        }
    }
}
