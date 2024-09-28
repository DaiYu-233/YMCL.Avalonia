using System;
using System.IO;
using Avalonia;
using Avalonia.Dialogs;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;

namespace YMCL.Main;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseManagedSystemDialogs()
            .WithInterFont()
            // .UsePlatformDetect()
            .LogToTrace();
    }
}