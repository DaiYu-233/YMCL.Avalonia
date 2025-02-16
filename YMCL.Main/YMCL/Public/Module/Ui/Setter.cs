using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Ursa.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Value;
using YMCL.Views.Main;
using NotificationCard = Ursa.Controls.NotificationCard;

namespace YMCL.Public.Module.Ui;

public class Setter
{
    public static void SetAccentColor(Color color)
    {
        try
        {
            Application.Current.Resources["SystemAccentColor"] = color;
            Application.Current.Resources["ButtonDefaultPrimaryForeground"] = color;
            Application.Current.Resources["TextBoxFocusBorderBrush"] = color;
            Application.Current.Resources["ComboBoxSelectorPressedBorderBrush"] = color;
            Application.Current.Resources["ComboBoxSelectorFocusBorderBrush"] = color;
            Application.Current.Resources["TextBoxSelectionBackground"] = color;
            Application.Current.Resources["ProgressBarPrimaryForeground"] = color;
            Application.Current.Resources["ProgressBarIndicatorBrush"] = color;
            Application.Current.Resources["SliderThumbBorderBrush"] = color;
            Application.Current.Resources["SliderTrackForeground"] = color;
            Application.Current.Resources["HyperlinkButtonOverForeground"] = color;
            Application.Current.Resources["SliderThumbPressedBorderBrush"] = color;
            Application.Current.Resources["SliderThumbPointeroverBorderBrush"] = color;
            Application.Current.Resources["SystemAccentColorLight1"] = Calculator.ColorVariant(color, 0.15f);
            Application.Current.Resources["SystemAccentColorLight2"] = Calculator.ColorVariant(color, 0.30f);
            Application.Current.Resources["SystemAccentColorLight3"] = Calculator.ColorVariant(color, 0.45f);
            Application.Current.Resources["SystemAccentColorDark1"] = Calculator.ColorVariant(color, -0.15f);
            Application.Current.Resources["SystemAccentColorDark2"] = Calculator.ColorVariant(color, -0.30f);
            Application.Current.Resources["SystemAccentColorDark3"] = Calculator.ColorVariant(color, -0.45f);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public static void UpdateWindowStyle(UrsaWindow window, Action? action = null)
    {
        if (window == null) return;
        if (Data.DesktopType == DesktopRunnerType.Linux ||
            Data.DesktopType == DesktopRunnerType.FreeBSD ||
            (Data.DesktopType == DesktopRunnerType.Windows &&
             Environment.OSVersion.Version.Major < 10))
        {
            window.IsManagedResizerVisible = true;
            window.SystemDecorations = SystemDecorations.None;
        }

        window.FindControl<Controls.TitleBar>("TitleBar").IsVisible = true;
        window.FindControl<Border>("Root").CornerRadius = new CornerRadius(8);
        // window.WindowState = WindowState.Maximized;
        // window.WindowState = WindowState.Normal;
        window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        window.ExtendClientAreaToDecorationsHint = true;
        action?.Invoke();
    }

    public static void ToggleTheme(Setting.Theme theme)
    {
        if (theme == Setting.Theme.Light)
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
        }
        else if (theme == Setting.Theme.Dark)
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
        }
        else if (theme == Setting.Theme.System)
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
        }
    }

    public static void SetBackGround()
    {
        if (Data.Setting is null) return;
        Application.Current.Resources["MainOpacity"] =
            Data.Setting.CustomBackGround == Setting.CustomBackGroundWay.Default
                ? 1.0
                : Data.Setting.TranslucentBackgroundOpacity;
        if (YMCL.App.UiRoot == null) return;
        var topLevel = TopLevel.GetTopLevel(YMCL.App.UiRoot);
        Application.Current.TryGetResource("2x", Application.Current.ActualThemeVariant,
            out var c2);
        Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant,
            out var c1);
        YMCL.App.UiRoot.FrameView.Background = Data.Setting.CustomBackGround == Setting.CustomBackGroundWay.Default
            ? (SolidColorBrush)c2
            : null;

        var setting = Const.Data.Setting;

        if (topLevel is not MainWindow window) return;

        window.BackGroundImg.Source = null;

        window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
        window.Root.Background = (SolidColorBrush)c1;
        try
        {
            (Ui.Getter.FindControlByName(window, "PART_PaneRoot") as Panel).Opacity =
                (double)Application.Current.Resources["MainOpacity"]!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Default)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.None];
            App.UiRoot.Nav.Background = (SolidColorBrush)c1;
        }
        else
        {
            App.UiRoot.Nav.Background = Brushes.Transparent;
        }

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Image &&
            !string.IsNullOrWhiteSpace(setting.WindowBackGroundImgData))
        {
            try
            {
                var bitmap = Value.Converter.Base64ToBitmap(setting.WindowBackGroundImgData);
                window.BackGroundImg.Source = bitmap;
            }
            catch
            {
                Const.Data.Setting.WindowBackGroundImgData = null;
                Notice(MainLang.LoadBackGroudFromPicFailTip, type: NotificationType.Error);
                window.BackGroundImg.Source = null;
            }

            return;
        }

        window.Background = Brushes.Transparent;
        window.Root.Background = Brushes.Transparent;
        (Ui.Getter.FindControlByName(window, "PART_PaneRoot") as Panel).Opacity =
            (double)Application.Current.Resources["MainOpacity"]!;

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.AcrylicBlur)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
        }

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Transparent)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        }

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Mica)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
        }
    }

    public static void AppStrangeEffect()
    {
        List<Action> methods =
        [
            //NeverGiveUp
            () =>
            {
                var launcher = TopLevel.GetTopLevel(App.UiRoot).Launcher;
                launcher.LaunchUriAsync(new Uri("https://www.bilibili.com/video/BV1GJ411x7h7/"));
            },
            //Transform180deg
            () =>
            {
                if (TopLevel.GetTopLevel(App.UiRoot) is not MainWindow window) return;
                var rotateTransform = new RotateTransform(180);
                window.Root.RenderTransform = rotateTransform;
            },
            //WindowMove
            () =>
            {
                if (TopLevel.GetTopLevel(App.UiRoot) is not MainWindow window) return;
                double velocityX = 20; // 水平速度
                double velocityY = 20; // 垂直速度

                // 使用DispatcherTimer来周期性地更新窗口位置
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(10) // 设置定时器的时间间隔
                };
                Screen? first = window.Screens.All.FirstOrDefault();

                var _screenBounds = first?.WorkingArea ??
                                    new PixelRect(0, 0, 800, 600);
                timer.Tick += Timer_Tick!;
                timer.Start();
                return;

                void Timer_Tick(object sender, EventArgs e)
                {
                    if (TopLevel.GetTopLevel(App.UiRoot) is not MainWindow mainWindow) return;
                    var newPosition = new PixelPoint((int)(mainWindow.Position.X + velocityX),
                        (int)(mainWindow.Position.Y + velocityY));

                    // 检查窗口是否即将超出屏幕边界
                    if (newPosition.X < _screenBounds.X || newPosition.X + mainWindow.Width >
                        _screenBounds.X + _screenBounds.Width)
                    {
                        velocityX = -velocityX; // 改变水平方向
                    }

                    if (newPosition.Y < _screenBounds.Y || newPosition.Y + mainWindow.Height >
                        _screenBounds.Y + _screenBounds.Height)
                    {
                        velocityY = -velocityY; // 改变垂直方向
                    }

                    // 设置新位置
                    mainWindow.Position = newPosition;
                }
            },
            //KeepSpinning
            async () =>
            {
                var deg = 0;
                if (TopLevel.GetTopLevel(App.UiRoot) is not MainWindow window) return;
                while (true)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (deg > 360)
                        {
                            deg = 0;
                        }

                        var rotateTransform = new RotateTransform(deg);
                        window.RenderTransform = rotateTransform;
                        deg += 2;
                    });
                    await Task.Delay(10);
                }
            },
        ];

        Random random = new Random();
        //methods[^1]();
        methods[random.Next(methods.Count)]();
    }

    public static class DynamicStyle
    {
        private static readonly StyleInclude NotificationBubbleStyle =
            new(new Uri("avares://YMCL/Public/Styles/Dynamic/NotificationBubble.axaml"))
                { Source = new Uri("avares://YMCL/Public/Styles/Dynamic/NotificationBubble.axaml") };

        private static readonly StyleInclude NotificationCardStyle =
            new(new Uri("avares://YMCL/Public/Styles/Dynamic/NotificationCard.axaml"))
                { Source = new Uri("avares://YMCL/Public/Styles/Dynamic/NotificationCard.axaml") };

        private static readonly StyleInclude PopupStyle =
            new(new Uri("avares://YMCL/Public/Styles/Dynamic/Popup.axaml"))
                { Source = new Uri("avares://YMCL/Public/Styles/Dynamic/Popup.axaml") };

        private static readonly StyleInclude ContentDialogStyle =
            new(new Uri("avares://YMCL/Public/Styles/Dynamic/ContentDialog.axaml"))
                { Source = new Uri("avares://YMCL/Public/Styles/Dynamic/ContentDialog.axaml") };

        public static void SetDynamicStyle()
        {
            if (Data.Setting is null) return;
            var list = Data.Setting.SpecialControlEnableTranslucent.Split(',').ToList();
            //NotificationBubble,NotificationCard,Popup,ContentDialog
            if (list.Contains("NotificationBubble"))
            {
                if (!Application.Current.Styles.Contains(NotificationBubbleStyle))
                    Application.Current.Styles.Add(NotificationBubbleStyle);
            }
            else
            {
                Application.Current.Styles.Remove(NotificationBubbleStyle);
            }

            if (list.Contains("NotificationCard"))
            {
                if (!Application.Current.Styles.Contains(NotificationCardStyle))
                    Application.Current.Styles.Add(NotificationCardStyle);
            }
            else
            {
                Application.Current.Styles.Remove(NotificationCardStyle);
            }

            if (list.Contains("Popup"))
            {
                if (!Application.Current.Styles.Contains(PopupStyle))
                    Application.Current.Styles.Add(PopupStyle);
            }
            else
            {
                Application.Current.Styles.Remove(PopupStyle);
            }

            if (list.Contains("ContentDialog"))
            {
                if (!Application.Current.Styles.Contains(ContentDialogStyle))
                    Application.Current.Styles.Add(ContentDialogStyle);
            }
            else
            {
                Application.Current.Styles.Remove(ContentDialogStyle);
            }
        }
    }
}