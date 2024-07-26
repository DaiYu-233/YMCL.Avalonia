using System;
using System.Collections.Concurrent;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Threading;

namespace YMCL.Main.Public.Controls.PageTaskEntry;

public partial class TaskEntry : UserControl
{
    private readonly Timer _debounceTimer;
    private readonly ConcurrentQueue<string> _textQueue = new();
    private bool _isUpdating;
    public bool isFinish;
    public WindowTaskEntry.TaskEntry _WindowTaskEntry;

    public TaskEntry(string name, bool valueProgress = true, bool textProgress = true)
    {
        InitializeComponent();
        TaskName.Text = name;
        if (!valueProgress)
        {
            TaskProgressBar.IsVisible = false;
            TaskProgressBarText.IsVisible = false;
        }

        if (!textProgress)
            TaskTextBox.IsVisible = false;
        else
            Height = 210;

        _debounceTimer = new Timer(500); // ���÷���ʱ����Ϊ0.5��  
        _debounceTimer.Elapsed += DebounceTimerElapsed;
        _debounceTimer.AutoReset = false; // ���Զ����ã��Ա����ǿ��Կ��ƺ�ʱ�ٴ�������  

        _WindowTaskEntry = new WindowTaskEntry.TaskEntry(name, valueProgress, textProgress);

        Const.Window.main.taskCenterPage.TaskContainer.Children.Add(this);
    }

    public void Finish()
    {
        isFinish = true;
        _WindowTaskEntry.Finish();
    }

    public void UpdateTextProgress(string text, bool includeTime = true)
    {
        _WindowTaskEntry.UpdateTextProgress(text, includeTime);
        
        _textQueue.Enqueue(GetTextToAdd(text, includeTime));

        if (!_isUpdating)
        {
            _isUpdating = true;
            _debounceTimer.Start();
        }
    }

    public void Destory()
    {
        _WindowTaskEntry.Destory();
        Finish();
        Const.Window.main.taskCenterPage.TaskContainer.Children.Remove(this);
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
            TaskTextBox.Text += combinedText;
            TaskTextBox.Focus();
            TaskTextBox.CaretIndex = TaskTextBox.Text.Length;
        });
    }

    public void UpdateValueProgress(double progress)
    {
        _WindowTaskEntry.UpdateValueProgress(progress);
        try
        {
            TaskProgressBar.Value = progress;
            TaskProgressBarText.Text = $"{Math.Round(progress, 1)}%";
        }
        catch
        {
        }
    }

    public void UpdateTitle(string title)
    {
        _WindowTaskEntry.UpdateTitle(title);
        TaskName.Text = title;
    }
}