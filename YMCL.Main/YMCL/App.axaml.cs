using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Threading;
using YMCL.Public.Module.Init;
using YMCL.Public.Module.Init.SubModule;
using YMCL.ViewModels;
using YMCL.Views;
using YMCL.Views.Crash;
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

    public override async void OnFrameworkInitializationCompleted()
    {
        if (!Debugger.IsAttached)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
        }
        try
        {
            if (!await InitDispatcher.BeforeCreateUi()) return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var win = new CrashWindow(e.ToString());
            win.Show();
            return;
        }

        var ifShowInit = Decision.WhetherToShowInitView();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            if (ifShowInit.ifShow)
            {
                var view = new InitializeWindow(ifShowInit.page);
                Data.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(view));
                desktop.MainWindow = view;
            }
            else
            {
                var view = new MainWindow(out var mainView);
                UiRoot = mainView;
                Data.Notification = new WindowNotificationManager(TopLevel.GetTopLevel(view));
                desktop.MainWindow = view;
                view.Show();
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

        Data.Notification.Position = NotificationPosition.BottomRight;

        base.OnFrameworkInitializationCompleted();
    }

    private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Console.WriteLine(e.Exception);
        try
        {
            var win = new CrashWindow(e.Exception.ToString());
            win.Show();
        }
        finally
        {
            e.Handled = true;
        }
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine(e);
        try
        {
            var win = new CrashWindow(e.ToString() ?? "Unhandled Exception");
            win.Show();
        }
        catch
        {
            // ignored
        }
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