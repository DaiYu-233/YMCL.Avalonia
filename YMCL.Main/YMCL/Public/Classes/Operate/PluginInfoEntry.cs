using System.IO;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Plugin.Base;

namespace YMCL.Public.Classes.Operate;

public class PluginInfoEntry : ReactiveObject
{
    public IPlugin Plugin { get; }
    [Reactive] public bool IsEnable { get; set; }
    public string Path { get; }

    public PluginInfoEntry(IPlugin plugin, string path, bool isEnable = false)
    {
        Plugin = plugin;
        Path = path;
        IsEnable = isEnable;
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(IsEnable)) return;
            if (IsEnable)
            {
                Const.Data.EnablePlugins.Add(Path);
            }
            else
            {
                Const.Data.EnablePlugins.Remove(Path);
            }

            File.WriteAllText(ConfigPath.PluginDataPath,
                JsonConvert.SerializeObject(Const.Data.EnablePlugins, Formatting.Indented));

            try
            {
                Plugin.Execute(IsEnable);
            }
            catch
            {
            }
        };
    }
}