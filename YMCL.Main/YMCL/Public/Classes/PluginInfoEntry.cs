using System.IO;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Plugin.Base;

namespace YMCL.Public.Classes;

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
                Data.EnablePlugins.Add(Path);
            }
            else
            {
                Data.EnablePlugins.Remove(Path);
            }

            File.WriteAllText(ConfigPath.PluginDataPath,
                JsonConvert.SerializeObject(Data.EnablePlugins, Formatting.Indented));

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