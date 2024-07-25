using System;
using System.Collections.Concurrent;
using System.IO;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Newtonsoft.Json;
using YMCL.Main.Public.Classes;

namespace YMCL.Main.Public.Controls.WindowTask;

public partial class WindowTask : Window
{
    private readonly Timer _debounceTimer;
    private readonly ConcurrentQueue<string> _textQueue = new();
    private bool _isUpdating;
    public TaskEntry entry;
    public bool isFinish;

    public WindowTask(string name, bool valueProgress = true)
    {
        InitializeComponent();
        TitleText.Text = name;
        Title = name;

        entry = new TaskEntry(name, valueProgress);

        if (!valueProgress) ValueProgressRoot.IsVisible = false;
        Closing += (s, e) =>
        {
            if (!isFinish) e.Cancel = true;
        };
        PropertyChanged += (s, e) =>
        {
            if (Const.Window.main.titleBarStyle == WindowTitleBarStyle.Ymcl && e.Property.Name == nameof(WindowState))
                switch (WindowState)
                {
                    case WindowState.Normal:
                        Root.Margin = new Thickness(0);
                        break;
                    case WindowState.Maximized:
                        Root.Margin = new Thickness(20);
                        break;
                }
        };
        Loaded += (_, _) =>
        {
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.WindowTitleBarStyle == WindowTitleBarStyle.System)
            {
                TitleBar.IsVisible = false;
                TitleText.IsVisible = false;
                ExtendClientAreaChromeHints = (ExtendClientAreaChromeHints)2;
                ExtendClientAreaToDecorationsHint = false;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                RootGrid.Margin = new Thickness(0, 10, 0, 0);
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
            }
        };

        _debounceTimer = new Timer(500); // 设置防抖时间间隔为0.5秒  
        _debounceTimer.Elapsed += DebounceTimerElapsed;
        _debounceTimer.AutoReset = false; // 不自动重置，以便我们可以控制何时再次启动它  

        Show();
        Activate();
    }

    public void Finish()
    {
        isFinish = true;
    }

    public void UpdateTextProgress(string text, bool includeTime = true)
    {
        entry.UpdateTextProgress(text, includeTime);
        _textQueue.Enqueue(GetTextToAdd(text, includeTime));

        if (!_isUpdating)
        {
            _isUpdating = true;
            _debounceTimer.Start();
        }
    }

    public void Destory()
    {
        Finish();
        Close();
        entry.Destory();
    }

    private string GetTextToAdd(string text, bool includeTime)
    {
        return includeTime ? $"[{DateTime.Now.ToString("HH:mm:ss")}] {text}\n" : $"{text}\n";
    }

    private void DebounceTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _isUpdating = false;

        var combinedText = string.Empty;
        while (_textQueue.TryDequeue(out var textToAdd)) combinedText += textToAdd;

        Dispatcher.UIThread.Post(() =>
        {
            ProgressTextBox.Text += combinedText;
            ProgressTextBox.Focus();
            ProgressTextBox.CaretIndex = ProgressTextBox.Text.Length;
        });
    }

    public void UpdateValueProgress(double progress)
    {
        try
        {
            ProgressBar.Value = progress;
            entry.UpdateValueProgress(progress);
            ProgressBarText.Text = $"{Math.Round(progress, 1)}%";
        }
        catch
        {
        }
    }

    public void UpdateTitle(string title)
    {
        TitleText.Text = title;
        Title = title;
        entry.UpdateTitle(title);
    }
}