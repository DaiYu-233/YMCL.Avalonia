using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes;
using YMCL.Public.Module;

namespace YMCL.Views.Crash;

public partial class CrashWindow : Window
{
    public CrashWindow(string exception)
    {
        InitializeComponent();
        Public.Module.Ui.Setter.UpdateWindowStyle(this);
        if (Data.Setting != null)
        {
            Data.Setting.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(Setting.WindowTitleBarStyle)) return;
                Public.Module.Ui.Setter.UpdateWindowStyle(this);
            };
        }
        
        View.Info.Text = exception;
        View.Copy.Click += async (_, _) =>
        {
            var clipboard = GetTopLevel(this)?.Clipboard;
            await clipboard.SetTextAsync(exception);
        };
        View.Continue.Click += (_, _) => { Close(); };
        View.Restart.Click += (_, _) => { AppMethod.RestartApp(); };
        View.Exit.Click += (_, _) => { Environment.Exit(0); };
        Topmost = true;
        Show();
        Activate();
    }

    public sealed override void Show()
    {
        base.Show();
    }
}