using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using YMCL.Public.Module.Init;
using YMCL.Public.Module.Init.SubModule;
using YMCL.ViewModels;
using YMCL.Views;
using YMCL.Views.Initialize;
using MainView = YMCL.Views.Main.MainView;
using MainWindow = YMCL.Views.Main.MainWindow;

namespace YMCL;

public class App : Application
{
    public static MainView? UiRoot { get; private set; }
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
                var view = new InitializeWindow(ifShowInit.page) { IsVisible = false };
                Data.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(view));
                desktop.MainWindow = view;
            }
            else
            {
                var view = new MainWindow(out var mainView);
                UiRoot = mainView;
                Data.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(view));
                desktop.MainWindow = view;
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            if (ifShowInit.ifShow)
            {
                var view = new InitializeView(ifShowInit.page);
                Data.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(view));
                singleViewPlatform.MainView = view;
            }
            else
            {
                var view = new MainView();
                UiRoot = view;
                Data.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(view));
                singleViewPlatform.MainView = view;
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