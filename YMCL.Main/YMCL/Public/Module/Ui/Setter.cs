using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Newtonsoft.Json;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;
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

    public static void UpdateWindowStyle(Window window, Setting.WindowTitleBarStyle? windowTitleBarStyle = null,
        Action? systemAction = null, Action? launcherAction = null)
    {
        if (window == null) return;
        var style = windowTitleBarStyle ?? Data.Setting.WindowTitleBarStyle;
        if (style == Setting.WindowTitleBarStyle.System)
        {
            window.FindControl<Controls.TitleBar>("TitleBar").IsVisible = false;
            window.FindControl<Border>("Root").CornerRadius = new CornerRadius(0, 0, 8, 8);
            window.FindControl<Border>("Root").Margin = new Thickness(0);
            window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            window.ExtendClientAreaToDecorationsHint = false;
            systemAction?.Invoke();
        }
        else if (style == Setting.WindowTitleBarStyle.Ymcl)
        {
            window.FindControl<Controls.TitleBar>("TitleBar").IsVisible = true;
            window.FindControl<Border>("Root").CornerRadius = new CornerRadius(8);
            window.WindowState = WindowState.Maximized;
            window.WindowState = WindowState.Normal;
            window.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            window.ExtendClientAreaToDecorationsHint = true;
            launcherAction?.Invoke();
        }
    }

    public static void SetBackGround()
    {
        Application.Current.Resources["MainOpacity"] = 1.0;
        if (YMCL.App.UiRoot == null) return;
        YMCL.App.UiRoot.BackGroundImg.Source = null;
        var topLevel = TopLevel.GetTopLevel(YMCL.App.UiRoot);
        Application.Current.TryGetResource("2x", Application.Current.ActualThemeVariant,
            out var c1);
        YMCL.App.UiRoot.FrameView.Background = Data.Setting.CustomBackGround == Setting.CustomBackGroundWay.Default
            ? (SolidColorBrush)c1 : null;
        if (topLevel is MainWindow window)
        {
            window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
            /*Const.Window.main.Root.Background = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#262626")
                : SolidColorBrush.Parse("#f6fafd");*/
            window.TitleBar.Root.Background = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#2c2c2c")
                : SolidColorBrush.Parse("#FFE9F6FF");
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
}