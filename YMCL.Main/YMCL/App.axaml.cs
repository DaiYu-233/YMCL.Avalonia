using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using YMCL.Public.Module.App;
using YMCL.ViewModels;
using YMCL.Views;
using YMCL.Views.Initialize;
using MainView = YMCL.Views.Main.MainView;
using MainWindow = YMCL.Views.Main.MainWindow;

namespace YMCL;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        InitDispatcher.BeforeCreateUi();
        var ifShowInit = Decision.WhetherToShowInitView();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            if (ifShowInit.ifShow)
            {
                desktop.MainWindow = new InitializeWindow(ifShowInit.page);
            }
            else
            {
                desktop.MainWindow = new MainWindow();
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            if (ifShowInit.ifShow)
            {
                singleViewPlatform.MainView = new InitializeView(ifShowInit.page);
            }
            else
            {
                singleViewPlatform.MainView = new MainView();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}