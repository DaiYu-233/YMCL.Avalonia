using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ursa.Controls;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using LogType = YMCL.Public.Enum.LogType;
using Setting = YMCL.Public.Classes.Setting;

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
    
    public void Append(string message, string time, LogType logType)
    {
        var log = new LogItemEntry
        {
            Time = time,
            Message = message,
            Type = logType,
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
    
    private bool _disposed;

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        // 1. 释放托管资源
        Content = null; // 清空内容
        DataContext = null; // 解除数据绑定

        // 2. 释放原生资源
        PlatformImpl?.Dispose();

        // 3. 标记为已释放
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~LogWindow()
    {
        Dispose();
    }
    
}