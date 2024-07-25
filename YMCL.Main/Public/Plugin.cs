using System;

namespace YMCL.Main.Public;

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

    public interface IPlugin
    {
        PluginInfo GetPluginInformation();

        /// <summary>
        ///     当插件加载时
        /// </summary>
        public void OnLoad();

        /// <summary>
        ///     当插件启用时
        /// </summary>
        /// <returns></returns>
        public void OnEnable();

        /// <summary>
        ///     当插件禁用
        /// </summary>
        public void OnDisable();

        /// <summary>
        ///     当游戏启动
        /// </summary>
        public void OnLaunch();
    }
}