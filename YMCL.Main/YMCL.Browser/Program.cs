using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using YMCL.Public.Const;
using YMCL.Public.Enum;

namespace YMCL.Browser;

internal sealed class Program
{
    private static Task Main() => BuildAvaloniaApp()
        .WithInterFont()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
    {
        Data.RunnerType = RunnerType.Android;
        return AppBuilder.Configure<App>();
    }
}