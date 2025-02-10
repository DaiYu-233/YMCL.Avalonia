using YMCL.Plugin.Base;

namespace YMCL.Plugin.Example;

public class ExamplePlugin : IPlugin
{
    public string Name => "Example Plugin"; 
    public string Author => "DaiYu";
    public string Description => "A example plugin for YMCL.";
    public string Version => "1.0.0"; 

    public int Execute(bool isEnable)
    {
        Console.WriteLine("Example plugin loaded successfully !");
        return 0;
    }
}