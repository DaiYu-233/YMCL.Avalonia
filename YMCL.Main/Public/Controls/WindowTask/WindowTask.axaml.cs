using Avalonia.Controls;
using static System.Net.Mime.MediaTypeNames;
using System;
using YMCL.Main.Public.Langs;
using Avalonia;
using YMCL.Main.Views.Main.Pages.Launch;

namespace YMCL.Main.Public.Controls.WindowTask
{
    public partial class WindowTask : Window
    {
        public WindowTask(string name, bool valueProgress = true)
        {
            InitializeComponent();
            TitleText.Text = name;
            if (!valueProgress)
            {
                ValueProgressRoot.IsVisible = false;
            }
            Closing += (s, e) =>
            {
                e.Cancel = true;
            };
            Loaded += (_, _) =>
            {
                if (Const.Platform != Platform.Windows)
                {
                    TitleBar.IsVisible = false;
                    TitleText.IsVisible = false;
                    Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                }
                else
                {
                    WindowState = WindowState.Maximized;
                    WindowState = WindowState.Normal;
                }
            };
            Show();
            Activate();
        }
        public void UpdateTextProgress(string text, bool time = true)
        {
            DateTime now = DateTime.Now;
            if (time)
            {
                ProgressTextBox.Text += $"[{now.ToString("HH:mm:ss")}] {text}\n";
            }
            else
            {
                ProgressTextBox.Text += $"{text}\n";
            }
            ProgressTextBox.Focus();
            ProgressTextBox.CaretIndex = ProgressTextBox.Text.Length;
            //ProgressTextBox.ScrollToLine((int)ProgressTextBox.LineHeight);
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
        }
    }
}
