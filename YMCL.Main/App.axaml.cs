using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Crash;

namespace YMCL.Main;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Const.Data.Setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.String.SettingDataPath));
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (Const.Data.Setting.Language != "Unset" &&
                Const.Data.Setting.WindowTitleBarStyle != WindowTitleBarStyle.Unset &&
                Const.Data.Setting.IsCompleteMinecraftFolderInitialize &&
                Const.Data.Setting.IsCompleteJavaInitialize &&
                Const.Data.Setting.IsCompleteAccountInitialize)
            {
                desktop.MainWindow = Const.Window.main;
                Const.Window.main._needInit = true;
            }
            else
            {
                desktop.MainWindow = Const.Window.initialize;
            }
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
        }

        base.OnFrameworkInitializationCompleted();

        Current.Resources["Opacity"] = 1.0;
    }

    private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            var win = new CrashWindow(e.Exception.ToString());
            win.Hide();
            win.ShowDialog(Const.Window.main);
        }
        finally
        {
            e.Handled = true;
        }
    }

    private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            var win = new CrashWindow(e.ToString()!);
            win.Hide();
            _ = win.ShowDialog(Const.Window.main);
        }
        catch
        {
        }
    }
}