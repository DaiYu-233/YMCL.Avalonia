using YMCL.Views.Main.Pages.DownloadPages;

namespace YMCL.Public.Module.App.SubModule.NetAndUiLoader;

public class Dispatcher
{
    public static void Dispatch()
    {
        _ = YMCL.Public.Module.App.SubModule.NetAndUiLoader.InstallableGame.Load();
    }
}