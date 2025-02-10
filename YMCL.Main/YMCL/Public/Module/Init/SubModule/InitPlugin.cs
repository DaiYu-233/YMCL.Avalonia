using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using YMCL.Plugin.Base;

namespace YMCL.Public.Module.Init.SubModule;

public class InitPlugin
{
    public static void Dispatch()
    {
        string[] pluginPaths =
        [
            // "E:\\YMCL.Avalonia\\YMCL.Plugin.Simple\\bin\\Debug\\net8.0\\YMCL.Plugin.Simple.dll"
            // "E:\\YMCL.Avalonia\\YMCL.Plugin\\YMCL.Plugin.Dependence\\bin\\Debug\\net8.0\\YMCL.Plugin.Dependence.dll"
        ];

        IEnumerable<IPlugin> commands = pluginPaths.SelectMany(pluginPath =>
        {
            var pluginAssembly = YMCL.Public.Plugin.Loader.LoadPlugin(pluginPath);
            return YMCL.Public.Plugin.Loader.CreateCommands(pluginAssembly);
        }).ToList();

        foreach (var command in commands)
        {
            Console.WriteLine($"{command.Name}\t - {command.Description}");
            command.Execute();
        }
    }
}