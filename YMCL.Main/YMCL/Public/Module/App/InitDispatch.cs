using YMCL.Public.Const;

namespace YMCL.Public.Module.App;

public static class Init
{
    public static void BeforeCreateUi()
    {
        ConfigPath.InitPath();
        InitConfig.CreateFolder();
        InitConfig.CreateFile();
    }
}