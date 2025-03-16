using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Ursa.Controls;
using YMCL.Views.Main;
using String = System.String;

namespace YMCL.Views;

public partial class NotificationWindow : Window
{
    public NotificationWindow(string? title, string? message = null, IImage? icon = null, double time = -1)
    {
        InitializeComponent();
        TitleTextBlock.Text = title ?? "Title";
        MessageTextBlock.Text = message ?? string.Empty;
        if (icon != null)
        {
            Panel.Margin = new Thickness(15, 10, 0, 0);
            Image.IsVisible = true;
            Image.Source = icon;
        }

        CloseButton.ClickMode = ClickMode.Release;
        CloseButton.Click += (_, _) => { CloseAction(); };
        Loaded += (_, _) =>
        {
            SystemDecorations = SystemDecorations.None;
            Root.Transitions.Clear();
            Root.Margin = new Thickness(Root.Bounds.Width, 0, -1 * Root.Bounds.Width, 0);
            Root.Transitions =
            [
                new ThicknessTransition()
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    Property = MarginProperty,
                    Easing = new ExponentialEaseOut()
                },
                new DoubleTransition()
                {
                    Duration = TimeSpan.FromMilliseconds(400),
                    Property = OpacityProperty,
                    Easing = new ExponentialEaseOut()
                }
            ];
            Root.Margin = new Thickness(0);
            Root.Opacity = 1;
        };
        var cTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(time > 0 ? time : 4000)
        };

        cTimer.Tick += Timer_Tick!;
        cTimer.Start();
        return;

        void Timer_Tick(object? sender, EventArgs e)
        {
            cTimer.Stop();
            CloseAction();
        }
    }


    public NotificationWindow()
    {
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        var screenSize = Screens.Primary.WorkingArea.Size;
        var windowSize = PixelSize.FromSize(ClientSize, Screens.Primary.Scaling);
        Root.Opacity = 0;

        Position = new PixelPoint(
            screenSize.Width - windowSize.Width - 10,
            screenSize.Height - windowSize.Height - 10);

        base.OnLoaded(e);
    }

    public async void  CloseAction()
    {
        Root.Opacity = 0;
        Icon.Opacity = 0;
        Image.Opacity = 0;
        CloseButton.Opacity = 0;
        TitleTextBlock.Opacity = 0;
        Viewer.Opacity = 0;
        await Task.Delay(450);
        Close();
    }
}