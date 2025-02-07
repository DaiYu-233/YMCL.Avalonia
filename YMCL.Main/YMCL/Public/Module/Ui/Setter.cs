using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Newtonsoft.Json;
using Ursa.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Value;
using YMCL.Views.Main;

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
        if (Data.DesktopType != DesktopRunnerType.Windows && Data.DesktopType != DesktopRunnerType.MacOs)
        {
            window.IsManagedResizerVisible = true;
            window.SystemDecorations = SystemDecorations.None;
        }
        window.FindControl<Controls.TitleBar>("TitleBar").IsVisible = true;
        window.FindControl<Border>("Root").CornerRadius = new CornerRadius(8);
        window.WindowState = WindowState.Maximized;
        window.WindowState = WindowState.Normal;
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
        Application.Current.Resources["MainOpacity"] = 1.0;
        if (YMCL.App.UiRoot == null) return;
        YMCL.App.UiRoot.BackGroundImg.Source = null;
        var topLevel = TopLevel.GetTopLevel(YMCL.App.UiRoot);
        Application.Current.TryGetResource("2x", Application.Current.ActualThemeVariant,
            out var c2);
        Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant,
            out var c1);
        YMCL.App.UiRoot.FrameView.Background = Data.Setting.CustomBackGround == Setting.CustomBackGroundWay.Default
            ? (SolidColorBrush)c2
            : null;
        if (topLevel is MainWindow window)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
            /*Const.Window.main.Root.Background = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#262626")
                : SolidColorBrush.Parse("#f6fafd");*/
            window.TitleBar.Root.Background = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#2c2c2c")
                : SolidColorBrush.Parse("#FFE9F6FF");
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
        }


        var setting = Const.Data.Setting;
        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Default)
        {
            return;
        }

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Image &&
            !string.IsNullOrWhiteSpace(setting.WindowBackGroundImgData))
        {
            Application.Current.Resources["MainOpacity"] = 0.875;
            try
            {
                var bitmap = Value.Converter.Base64ToBitmap(setting.WindowBackGroundImgData);
                YMCL.App.UiRoot.BackGroundImg.Source = bitmap;
            }
            catch
            {
                Const.Data.Setting.WindowBackGroundImgData = null;
                Toast(MainLang.LoadBackGroudFromPicFailTip, type: NotificationType.Error);
                Application.Current.Resources["MainOpacity"] = 1.0;
                YMCL.App.UiRoot.BackGroundImg.Source = null;
            }

            if (topLevel is not MainWindow window1) return;
            try
            {
                (Ui.Getter.FindControlByName(window1, "PART_PaneRoot") as Panel).Opacity =
                    (double)Application.Current.Resources["MainOpacity"]!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return;
        }

        if (topLevel is not MainWindow window2) return;
        window2.Background = Brushes.Transparent;
        Application.Current.Resources["MainOpacity"] = 0.75;
        window2.Root.Background = Brushes.Transparent;
        window2.TitleBar.Root.Background =
            Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#a8242424")
                : SolidColorBrush.Parse("#a8e7f5ff");
        (Ui.Getter.FindControlByName(window2, "PART_PaneRoot") as Panel).Opacity =
            (double)Application.Current.Resources["MainOpacity"]!;

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.AcrylicBlur)
        {
            window2.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
        }

        if (setting.CustomBackGround == Setting.CustomBackGroundWay.Transparent)
        {
            window2.TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
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
                DispatcherTimer timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(10) // 设置定时器的时间间隔
                };
                var _screenBounds = window.Screens.All.FirstOrDefault()?.WorkingArea ??
                                    new PixelRect(0, 0, 800, 600);
                timer.Tick += Timer_Tick!;
                timer.Start();

                void Timer_Tick(object sender, EventArgs e)
                {
                    if (TopLevel.GetTopLevel(App.UiRoot) is not MainWindow window) return;
                    var newPosition = new PixelPoint((int)(window.Position.X + velocityX),
                        (int)(window.Position.Y + velocityY));

                    // 检查窗口是否即将超出屏幕边界
                    if (newPosition.X < _screenBounds.X || newPosition.X + window.Width >
                        _screenBounds.X + _screenBounds.Width)
                    {
                        velocityX = -velocityX; // 改变水平方向
                    }

                    if (newPosition.Y < _screenBounds.Y || newPosition.Y + window.Height >
                        _screenBounds.Y + _screenBounds.Height)
                    {
                        velocityY = -velocityY; // 改变垂直方向
                    }

                    // 设置新位置
                    window.Position = newPosition;
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
}