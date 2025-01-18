using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;

namespace YMCL.Browser;

internal sealed class Program
{
    private static Task Main() => BuildAvaloniaApp()
        .WithInterFont()
        .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}