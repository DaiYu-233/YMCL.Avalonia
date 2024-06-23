using Avalonia.Controls;
using static System.Net.Mime.MediaTypeNames;
using System;
using YMCL.Main.Public.Langs;
using Avalonia;
using YMCL.Main.Views.Main.Pages.Launch;
using System.Collections.Concurrent;
using System.Timers;
using Avalonia.Threading;

namespace YMCL.Main.Public.Controls.WindowTask
{
    public partial class WindowTask : Window
    {
        private readonly Timer _debounceTimer;
        private readonly ConcurrentQueue<string> _textQueue = new ConcurrentQueue<string>();
        private bool _isUpdating;
        public bool isFinish = false;
        public WindowTask(string name, bool valueProgress = true)
        {
            InitializeComponent();
            TitleText.Text = name;
            Title = name;
            if (!valueProgress)
            {
                ValueProgressRoot.IsVisible = false;
            }
            Closing += (s, e) =>
            {
                if (!isFinish)
                {
                    e.Cancel = true;
                }
            };
            Loaded += (_, _) =>
            {
                if (Const.Platform != Platform.Windows)
                {
                    TitleBar.IsVisible = false;
                    TitleText.IsVisible = false;
                    Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                    ValueProgressRoot.Margin = new Thickness(10, 10, 10, 5);
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

        //public void Finish()
        //{
        //    isFinish = true;
        //}
        public void Finish()
        {
            isFinish = true;
        }
        public void UpdateTextProgress(string text, bool includeTime = true)
        {
            _textQueue.Enqueue(GetTextToAdd(text, includeTime));

            if (!_isUpdating)
            {
                _isUpdating = true;
                _debounceTimer.Start();
            }
        }

        private string GetTextToAdd(string text, bool includeTime)
        {
            return includeTime ? $"[{DateTime.Now.ToString("HH:mm:ss")}] {text}\n" : $"{text}\n";
        }

        private void DebounceTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _isUpdating = false;

            string combinedText = string.Empty;
            while (_textQueue.TryDequeue(out string textToAdd))
            {
                combinedText += textToAdd;
            }

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
                ProgressBarText.Text = $"{Math.Round(progress, 1)}%";
            }
            catch { }
        }
        public void UpdateTitle(string title)
        {
            TitleText.Text = title;
            Title = title;
        }
    }
}
