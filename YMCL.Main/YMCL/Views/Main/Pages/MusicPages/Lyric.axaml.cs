using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ReactiveUI;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.IO.Network;

namespace YMCL.Views.Main.Pages.MusicPages;

public partial class Lyric : UserControl
{
    public class Lyrics
    {
        public TimeSpan Time { get; set; }
        public string Text { get; set; }
        public double Index { get; set; }
    }

    public ObservableCollection<TextBlock> LyricRuns { get; set; } = [];
    private List<Lyrics> _lyrics = [];
    private DispatcherTimer _timerForLyric = new();
    private SolidColorBrush _solidColorBrush;

    public Lyric()
    {
        InitializeComponent();
        Application.Current.ActualThemeVariantChanged += (_, _) => { SetColor(); };
        SetColor();
        _lyrics.Clear();
        LyricRuns.Clear();
        LyricBlock.Children.Clear();
        Init();
        DataContext = this;
    }

    public void Init()
    {
        LyricBlock.Children.Add(new TextBlock
        {
            Text = MainLang.LyricTip + "\n",
            Height = 22,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            Foreground = _solidColorBrush
        });
    }

    public async System.Threading.Tasks.Task LoadLyric(RecordSongEntry song)
    {
        _lyrics.Clear();
        LyricRuns.Clear();
        LyricBlock.Children.Clear();
        Init();
        if (song.Type != SongType.Netease) return;
        var response = await NeteaseMusic.GetSongLyricById(song.SongId);
        if (response == null) return;
        _lyrics = ParseLyrics(response.lrc.lyric);
        LyricBlock.Height = LyricRuns.Count * 22 + 30;
        if(_lyrics.Count == 0) return;
        LyricBlock.Children.Clear();
        foreach (var lyric in _lyrics)
        {
            var tra = new Transitions();
            var run = new TextBlock { Text = lyric.Text + "\n", Height = 22 };
            var milliseconds = (long)lyric.Time.TotalMilliseconds;
            run.Tag = milliseconds.ToString();
            run.HorizontalAlignment = HorizontalAlignment.Center;
            run.Transitions = tra;
            run.Transitions.Add(new DoubleTransition
            {
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new SineEaseInOut(),
                Property = FontSizeProperty
            });
            run.Transitions.Add(new DoubleTransition
            {
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new SineEaseInOut(),
                Property = OpacityProperty
            });

            run.VerticalAlignment = VerticalAlignment.Center;
            run.TextAlignment = TextAlignment.Center;
            run.Foreground = _solidColorBrush;
            run.PointerPressed += RunOnPointerPressed;
            LyricRuns.Add(run);
            LyricBlock.Children.Add(run);
        }

        if (_timerForLyric != null) _timerForLyric.Stop();

        _timerForLyric = new DispatcherTimer();
        _timerForLyric.Interval = TimeSpan.FromSeconds(0.01);
        _timerForLyric.Tick += TimerForLyric_Tick;
        _timerForLyric.Start();
    }

    private async void TimerForLyric_Tick(object? sender, EventArgs e)
    {
        void DefaultUi(TextBlock x)
        {
            x.FontSize = 14;
            x.Height = 22;
            x.Foreground = _solidColorBrush;
        }

        void AccentUi(TextBlock x)
        {
            x.Foreground = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]!);
            x.FontSize = 18;
            x.Height = 30;
        }

        try
        {
            if (Data.UiProperty.SelectedRecordSong == null) return;
            if (LyricBlock.Children.Count == 0) return;
            TimeSpan currentTime;
            try
            {
                currentTime = TimeSpan.FromMilliseconds(Data.UiProperty.MusicCurrentTime);
            }
            catch
            {
                return;
            }

            for (var i = 1; i < _lyrics.Count; i++)
                if (_lyrics[i].Time > currentTime)
                {
                    if (i + 1 > LyricRuns.Count) return;
                    foreach (var x in LyricRuns)
                    {
                        if (x == LyricRuns[i - 1]) continue;

                        DefaultUi(x);
                    }

                    AccentUi(LyricRuns[i - 1]);
                    await System.Threading.Tasks.Task.Delay(210);
                    // var offset = LyricRoot.Bounds.Height / 2 - 22*i - 1;
                    // Console.WriteLine($"------\n{i - 1}\n{offset}\n");
                    // var offset = _lyricRuns[i - 1].Bounds.Top * -1;
                    var offset = LyricRoot.Bounds.Height / 2 - LyricRuns[i - 1].Bounds.Top;
                    LyricBlock.Margin = new Thickness(0, offset, 0, 0);

                    // if (last != i)
                    // {
                    //     last = i;
                    //     lyricsViewer.Progress = TimeSpan.FromMilliseconds(PlayerSlider.Value).TotalSeconds;
                    // }

                    break;
                }
        }
        catch
        {
        }
    }

    private void RunOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        AudioPlayer.Instance.UpdateProgress(Convert.ToInt64(((TextBlock)sender).Tag));
    }

    public List<Lyrics> ParseLyrics(string lyricsText)
    {
        var lines = lyricsText.Split('\n');
        var lyrics = new List<Lyrics>();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(']');
            if (parts.Length < 2) continue;
            var timeText = parts[0].TrimStart('[');
            var time = ParseTime(timeText);
            var text = parts[1];
            if (!string.IsNullOrWhiteSpace(text))
            {
                lyrics.Add(new Lyrics { Time = time, Text = text, Index = i });
            }
        }

        // lyricsViewer.Lyrics = lyrics1;
        return lyrics;
    }
    
    private TimeSpan ParseTime(string timeText)
    {
        var parts = timeText.Split(':');
        var minutes = int.Parse(parts[0]);
        var secondsAndMilliseconds = parts[1].Split('.');
        var seconds = int.Parse(secondsAndMilliseconds[0]);
        var milliseconds = int.Parse(secondsAndMilliseconds[1]);
        return new TimeSpan(0, 0, minutes, seconds, milliseconds);
    }

    private void SetColor()
    {
        _solidColorBrush = Application.Current.ActualThemeVariant == ThemeVariant.Light
            ? new SolidColorBrush(Color.FromArgb((byte)(255 * 0.3), 0x33, 0x33, 0x33))
            : new SolidColorBrush(Color.FromArgb((byte)(255 * 0.3), 255, 255, 255));
    }
}