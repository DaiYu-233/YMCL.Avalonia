namespace YMCL.Plugin.Base;

public interface IPlugin
{
    string Name { get; }
    string Author { get; }
    string Description { get; }
    string Version { get; }

    int Execute(bool isEnable);
}