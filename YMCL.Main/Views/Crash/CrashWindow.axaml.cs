using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Crash;

public partial class CrashWindow : Window
{
    public CrashWindow(string exception)
    {
        InitializeComponent();
        Info.Text = exception;
        if (Const.Data.LastCrashInfoWindow != null)
        {
            Info.Text += "\n\n---------\n\n" + Const.Data.LastCrashInfoWindow.Info.Text;
            Const.Data.LastCrashInfoWindow.Close();
        }
        Const.Data.LastCrashInfoWindow = this;
        switch (Const.Data.Setting.WindowTitleBarStyle)
        {
            case WindowTitleBarStyle.System:
                TitleBar.IsVisible = false;
                TitleRoot.IsVisible = false;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                ExtendClientAreaToDecorationsHint = false;
                break;
            case WindowTitleBarStyle.Ymcl:
                TitleBar.IsVisible = true;
                TitleRoot.IsVisible = true;
                Root.CornerRadius = new CornerRadius(8);
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
                ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                ExtendClientAreaToDecorationsHint = true;
                break;
        }

        Copy.Click += async (_, _) =>
        {
            var clipboard = GetTopLevel(this)?.Clipboard;
            await clipboard.SetTextAsync(Info.Text);
        };
        Continue.Click += (_, _) => { Close(); };
        Restart.Click += (_, _) => { Method.Ui.RestartApp(); };
        Exit.Click += (_, _) => { Environment.Exit(0); };
    }
}