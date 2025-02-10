namespace YMCL.Plugin.Base;

public interface IPlugin
{
    string Name { get; }
    string Description { get; }
    string Version { get; }

    int Execute();
}