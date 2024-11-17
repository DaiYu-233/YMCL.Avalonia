using System;

namespace YMCL.Main.Public
{
    public class Plugin
    {
        public class PluginInfo
        {
            public string Name { set; get; } 
            public string Version { set; get; } 
            public string Author { set; get; }
            public string Description { set; get; } 
            public DateTime Time { set; get; } 
        }

        public interface IPlugin
        {
            PluginInfo GetPluginInformation();

            void OnLoad() { }
            
            void OnEnable() { }
            
            void OnDisable() { }
            
            void OnLaunch() { }
        }
    }
}