using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Input;
using System.Linq;
using Avalonia.Media.TextFormatting;
using System.Globalization;
using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Styling;
using System.Threading.Tasks;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using LyricsPlayerControl;

namespace YMCL.Main.Public.Controls
{
    public class LyricsViewer : Control
    {
        private int _currentLyricIndex = 0;
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private bool _isScrolling = false;
        private bool offsetChangedByMouse = false;

        public static readonly StyledProperty<IImmutableSolidColorBrush> CurrentLyricColorProperty =
            AvaloniaProperty.Register<LyricsViewer, IImmutableSolidColorBrush>(
                nameof(CurrentLyricColor),
                Brushes.Red);

        public IImmutableSolidColorBrush CurrentLyricColor
        {
            get => GetValue(CurrentLyricColorProperty);
            set => SetValue(CurrentLyricColorProperty, value);
        }

        public FontFamily FontFamily
        {
            get { return GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            AvaloniaProperty.Register<LyricsViewer, FontFamily>(nameof(FontFamily), new FontFamily("Microsoft YaHei"));

        public static readonly StyledProperty<List<LyricStruct>> LyricsProperty =
            AvaloniaProperty.Register<LyricsViewer, List<LyricStruct>>(
                nameof(Lyrics), []);

        public List<LyricStruct> Lyrics
        {
            get => GetValue(LyricsProperty);
            set => SetValue(LyricsProperty, value);
        }

        public static readonly StyledProperty<double> LyricOpacityProperty =
            AvaloniaProperty.Register<LyricsViewer, double>(
                nameof(LyricOpacity),
                1.0);

        public double LyricOpacity
        {
            get => GetValue(LyricOpacityProperty);
            set => SetValue(LyricOpacityProperty, value);
        }

        public static readonly StyledProperty<double> FontSizeProperty =
            AvaloniaProperty.Register<LyricsViewer, double>(
                nameof(FontSize),
                24.0);

        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set
            {
                SetValue(FontSizeProperty, value);
                InvalidateVisual();
            }
        }

        public static readonly StyledProperty<HorizontalAlignment> ContentHorizontalAlignmentProperty =
            AvaloniaProperty.Register<LyricsViewer, HorizontalAlignment>(
                nameof(ContentHorizontalAlignment),
                HorizontalAlignment.Left);

        public HorizontalAlignment ContentHorizontalAlignment
        {
            get => GetValue(ContentHorizontalAlignmentProperty);
            set => SetValue(ContentHorizontalAlignmentProperty, value);
        }

        public static readonly StyledProperty<double> ScrollOffsetProperty =
            AvaloniaProperty.Register<LyricsViewer, double>(
                nameof(ScrollOffset),
                0d);

        public double ScrollOffset
        {
            get => GetValue(ScrollOffsetProperty);
            set => SetValue(ScrollOffsetProperty, value);
        }

        public static readonly StyledProperty<double> ProgressProperty =
            AvaloniaProperty.Register<LyricsViewer, double>(
                nameof(Progress),
                0d);

        public double Progress
        {
            get => GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        // 在构造函数中添加类处理程序来响应属性更改
        static LyricsViewer()
        {
            try
            {
                CurrentLyricColorProperty.Changed.AddClassHandler<LyricsViewer>((viewer, e) =>
                    Dispatcher.UIThread.Invoke(() => Dispatcher.UIThread.Invoke(viewer.InvalidateVisual)));
                LyricsProperty.Changed.AddClassHandler<LyricsViewer>((viewer, e) =>
                    Dispatcher.UIThread.Invoke(() => Dispatcher.UIThread.Invoke(viewer.InvalidateVisual)));
                LyricOpacityProperty.Changed.AddClassHandler<LyricsViewer>((viewer, e) =>
                    Dispatcher.UIThread.Invoke(() => Dispatcher.UIThread.Invoke(viewer.InvalidateVisual)));
                ContentHorizontalAlignmentProperty.Changed.AddClassHandler<LyricsViewer>((viewer, e) =>
                    Dispatcher.UIThread.Invoke(() => Dispatcher.UIThread.Invoke(viewer.InvalidateVisual)));
                ScrollOffsetProperty.Changed.AddClassHandler<LyricsViewer>((viewer, e) =>
                    Dispatcher.UIThread.Invoke(() => Dispatcher.UIThread.Invoke(viewer.InvalidateVisual)));
                ProgressProperty.Changed.AddClassHandler<LyricsViewer>((viewer, e) =>
                    viewer.UpdateCurrentTime((long)(viewer.Progress * TimeSpan.TicksPerSecond)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public LyricsViewer()
        {
            _ = UpdateColor();
            Lyrics = [];
        }

        private async Task UpdateColor()
        {
            while (true)
            {
                CurrentLyricColor =
                    new ImmutableSolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]!);
                await Task.Delay(1000);
            }
        }

        public void UpdateCurrentTime(long currentTime)
        {
            int newIndex = FindCurrentLyricIndex(currentTime);
            if (newIndex != _currentLyricIndex)
            {
                SetCurrentLyricIndex(newIndex);
            }
        }

        private int FindCurrentLyricIndex(long currentTime)
        {
            int left = 0;
            int right = Lyrics.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (Lyrics[mid].Ticks == currentTime)
                {
                    return mid;
                }
                else if (Lyrics[mid].Ticks < currentTime)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return left - 1 >= 0 ? left - 1 : 0;
        }

        public void LoadLyricsFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            Lyrics.Clear();
            foreach (string line in lines)
            {
                string[] parts = line.Split(']');
                if (parts.Length == 2)
                {
                    long ticks = TimeSpan.Parse(parts[0][1..]).Ticks;
                    Lyrics.Add(new LyricStruct { Ticks = ticks, Text = parts[1].Trim() });
                }
            }

            _currentLyricIndex = 0;
            InvalidateVisual();
        }

        /// <summary>
        /// 限制滚动
        /// </summary>
        private void ClampScrollOffset()
        {
            double maxScrollOffset = Math.Max((Lyrics.Count - _currentLyricIndex - 1) * (FontSize * 1.5), 0);
            double minScrollOffset = -_currentLyricIndex * (FontSize * 1.5);
            SetCurrentValue(ScrollOffsetProperty, Math.Clamp(ScrollOffset, minScrollOffset, maxScrollOffset));
        }

        public override void Render(DrawingContext drawingContext)
        {
            base.Render(drawingContext);
            double centerY = Bounds.Height / 2;
            double lineHeight = FontSize * 1.5;

#if DEBUG
            // 计算列表中最长的文本宽度
            int maxTextWidth = Lyrics.Count > 0
                ? Lyrics.Max(lyric => (int)GetFormattedTextWidth(lyric.Text, FontSize))
                : 0;
            // 设置靠拢位置
            int maxXPos = ContentHorizontalAlignment switch
            {
                HorizontalAlignment.Left => 10,
                HorizontalAlignment.Center => (int)((Bounds.Width - maxTextWidth) / 2),
                HorizontalAlignment.Right => (int)(Bounds.Width - maxTextWidth - 10),
                _ => 10
            };
#endif

            ClampScrollOffset();
            Span<LyricStruct> lyricsSpan = CollectionsMarshal.AsSpan(Lyrics);

            // 开始绘制
            for (int i = 0; i < lyricsSpan.Length; i++)
            {
                double yPos = centerY + ((i - _currentLyricIndex) * lineHeight) - ScrollOffset;
                double distanceFromCenter = Math.Abs(yPos - centerY);
                double maxDistance = Bounds.Height / 2;
                double opacity = LyricOpacity - (distanceFromCenter / maxDistance); // 逐渐透明

                // 如果小于等于0，则跳过，不做任何动作，这也是为了高性能考虑，否则整个歌词列表都得绘制
                if (opacity <= 0)
                {
                    continue;
                }

                Application.Current.TryGetResource("TextColor", Application.Current.ActualThemeVariant,
                    out var c);

                // 选择歌词颜色，当前歌词使用当前行颜色，其余行使用默认颜色
                Brush lyricBrush = (i == _currentLyricIndex)
                    ? new SolidColorBrush(Color.FromArgb((byte)(opacity * 255), CurrentLyricColor.Color.R,
                        CurrentLyricColor.Color.G, CurrentLyricColor.Color.B))
                    : c as SolidColorBrush;

                // 根据水平对齐设置X坐标
                double xPos = ContentHorizontalAlignment switch
                {
                    HorizontalAlignment.Left => 10,
                    HorizontalAlignment.Center => (Bounds.Width -
                                                   GetFormattedTextWidth(lyricsSpan[i].Text, FontSize)) / 2,
                    HorizontalAlignment.Right => Bounds.Width -
                                                 GetFormattedTextWidth(lyricsSpan[i].Text, FontSize) - 10,
                    _ => 10
                };

                DrawLyricText(drawingContext, lyricsSpan[i].Text, lyricBrush, new Point(xPos, yPos));

// #if DEBUG
//                 string debugText = $"L({xPos:#.##}, {yPos:#.##}) O({ScrollOffset:0.##})";
//                 DrawLyricText(drawingContext, debugText, new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
//                     new Point(
//                         ContentHorizontalAlignment == HorizontalAlignment.Right
//                             ? maxXPos - (debugText.Length * FontSize / 2) - 30
//                             : maxXPos + maxTextWidth + 30, yPos));
// #endif
            }
        }

        /// <summary>
        /// 使用 DrawingContext 绘制文本
        /// </summary>
        private void DrawLyricText(DrawingContext drawingContext, string text, Brush brush, Point position)
        {
            drawingContext.DrawText(
                new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(FontFamily),
                    FontSize, brush), position);
        }

        /// <summary>
        /// 使用 TextFormatter 计算文本宽度
        /// </summary>
        private double GetFormattedTextWidth(string text, double fontSize)
        {
            var typeface = new Typeface(FontFamily); // 使用适当的字体
            var textFormat = new TextLayout(
                text,
                typeface,
                fontSize,
                Brushes.Black
            );

            return textFormat.Width;
        }

        public double ConvertToCenterCoordinates(double y)
        {
            // ������ת��Ϊ����Ļ����Ϊԭ�������
            double centerY = Bounds.Height / 2;
            double newY = centerY - y;
            return newY;
        }

        public async void SetCurrentLyricIndex(int index)
        {
            if (index >= 0 && index < Lyrics.Count)
            {
                int lastLyricIndex = _currentLyricIndex;
                _ = ScrollOffset;

                _currentLyricIndex = index;
                SetCurrentValue(ScrollOffsetProperty, 0d);
                SetCurrentValue(ProgressProperty, (double)Lyrics[_currentLyricIndex].Ticks / TimeSpan.TicksPerSecond);

                await AnimateScrollOffset(
                    offsetChangedByMouse
                        ? ConvertToCenterCoordinates(_lastMousePosition.Y)
                        : (lastLyricIndex - _currentLyricIndex) * (FontSize * 1.5), 0);
            }

            offsetChangedByMouse = false;
        }

        private async Task AnimateScrollOffset(double from, double to)
        {
            var a = ScrollOffset;
            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                Easing = new QuadraticEaseOut(), // 使用 Avalonia 的 Easing
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(ScrollOffsetProperty, from)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(ScrollOffsetProperty, to)
                        }
                    }
                }
            };

            // 执行动画
            await animation.RunAsync(this);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            Point point = e.GetPosition(this);
            double centerY = Bounds.Height / 2;
            double lineHeight = FontSize * 1.5;

            for (int i = 0; i < Lyrics.Count; i++)
            {
                double yPos = centerY + ((i - _currentLyricIndex) * lineHeight) - ScrollOffset;

                // 检测点击位置是否在某一行歌词上
                if (point.Y >= yPos && point.Y <= yPos + lineHeight)
                {
                    e.Handled = true;
                }
            }

            e.Handled = false;


            var pointerProperty = e.GetCurrentPoint(this).Properties;
            if (pointerProperty.IsLeftButtonPressed)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(this);
                e.Pointer.Capture(this);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            e.Pointer.Capture(null);

            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                if (!_isScrolling)
                {
                    Point clickPoint = e.GetPosition(this);
                    double centerY = Bounds.Height / 2;
                    double lineHeight = FontSize * 1.5;

                    for (int i = 0; i < Lyrics.Count; i++)
                    {
                        double yPos = centerY + ((i - _currentLyricIndex) * lineHeight) - ScrollOffset;

                        if (clickPoint.Y >= yPos && clickPoint.Y <= yPos + lineHeight)
                        {
                            // 高亮当前歌词
                            SetCurrentLyricIndex(i);
                            try
                            {
                                Const.Window.main.musicPage._waveStream.Position =
                                    TimeSpan.FromTicks(Lyrics[i].Ticks).Milliseconds;
                            }
                            catch
                            {
                            }

                            break;
                        }
                    }
                }

                _isDragging = false;
                _isScrolling = false;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isDragging)
            {
                Point currentMousePosition = e.GetPosition(this);
                double deltaY = currentMousePosition.Y - _lastMousePosition.Y;
                SetCurrentValue(ScrollOffsetProperty, ScrollOffset - deltaY);
                Dispatcher.UIThread.Invoke(InvalidateVisual);
                _lastMousePosition = currentMousePosition; // 记录鼠标上一次的位置
                _isScrolling = true; // 设置滚动状态
                offsetChangedByMouse = true;
            }
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            Dispatcher.UIThread.Invoke(InvalidateVisual);
        }
    }
}