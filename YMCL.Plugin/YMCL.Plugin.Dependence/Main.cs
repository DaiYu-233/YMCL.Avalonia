using System.Reflection;
using Avalonia.Controls;
using Newtonsoft.Json;
using YMCL.Plugin.Base;
using YMCL.Views.Main;

namespace YMCL.Plugin.Dependence;

public class ExamplePluginWithDependence : IPlugin
{
    public string Name => "Example Plugin With Dependence";
    public string Author => "DaiYu";
    public string Description => "A example plugin with dependence for YMCL.";
    public string Version => "1.0.0"; 

    public int Execute(bool isEnable)
    {
        Console.WriteLine("Example Plugin With Dependence loaded successfully !");
        Console.WriteLine("YMCL Setting Data: ");
        var obj = YMCL.Public.Const.Data.Setting;
        var serializeObject = JsonConvert.SerializeObject(obj, Formatting.Indented);
        Console.WriteLine(serializeObject);
        return 0;
    }
}