using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Ursa.Controls;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using LogType = YMCL.Public.Enum.LogType;
using Setting = YMCL.Public.Classes.Setting.Setting;

namespace YMCL.Views.Log;

public partial class LogWindow : UrsaWindow
{
    public LogViewer Viewer { get; set; }
    public new bool CanClose { get; set; }

    public LogWindow()
    {
        InitializeComponent();
        Viewer = new LogViewer();
        ContentControl.Content = Viewer;
        Loaded += (_, _) =>
        {
            Public.Module.Ui.Setter.UpdateWindowStyle(this);
        };
        PropertyChanged += (_, _) =>
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    Root.Margin = new Thickness(0);
                    Root.BorderThickness = new Thickness(0);
                    break;
                case WindowState.Maximized:
                    Root.Margin = new Thickness(20);
                    Root.BorderThickness = new Thickness(2);
                    break;
            }
        };
        Application.Current.ActualThemeVariantChanged += (_, _) =>
        {
            Hide();
            Show();
        };
    }
    
    public void Append(string message, string time, LogType logType , string source)
    {
        var log = new LogItemEntry
        {
            Time = string.IsNullOrWhiteSpace(time) ? DateTime.Now.ToString("HH:mm:ss") : time,
            Message = string.IsNullOrWhiteSpace(message) ? "Empty Message" : message,
            Source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source,
            Type = string.IsNullOrWhiteSpace(logType.ToString()) ? LogType.Unknown : logType,
        };
        log.SetOriginal();
        Viewer.Model.LogItems.Add(log);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (CanClose)
        {
            base.OnClosing(e);
        }
        else
        {
            Hide();
            e.Cancel = true;
        }
    }

    public override void Show()
    {
        Public.Module.Ui.Setter.UpdateWindowStyle(this);
        base.Show();
    }

    public void Destory()
    {
        CanClose = true;
        Close();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        Dispose();
        base.OnClosed(e);
    }

    public void Dispose()
    {
        Viewer.Dispose();
        
        Content = null; 
        DataContext = null; 

        PlatformImpl?.Dispose();
    }

    ~LogWindow()
    {
        Dispose();
    }
    
}