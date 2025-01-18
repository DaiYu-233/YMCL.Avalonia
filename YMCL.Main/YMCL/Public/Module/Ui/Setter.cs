using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using YMCL.Public.Const;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.Ui;

public class Setter
{
    public static void SetAccentColor(Color color)
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
        Application.Current.Resources["SystemAccentColorLight1"] = Value.Calculator.ColorVariant(color, 0.15f);
        Application.Current.Resources["SystemAccentColorLight2"] = Value.Calculator.ColorVariant(color, 0.30f);
        Application.Current.Resources["SystemAccentColorLight3"] = Value.Calculator.ColorVariant(color, 0.45f);
        Application.Current.Resources["SystemAccentColorDark1"] = Value.Calculator.ColorVariant(color, -0.15f);
        Application.Current.Resources["SystemAccentColorDark2"] = Value.Calculator.ColorVariant(color, -0.30f);
        Application.Current.Resources["SystemAccentColorDark3"] = Value.Calculator.ColorVariant(color, -0.45f);
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
}