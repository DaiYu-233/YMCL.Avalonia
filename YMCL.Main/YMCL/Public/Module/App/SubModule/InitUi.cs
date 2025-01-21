using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Core;
using YMCL.Public.Module.Ui;

namespace YMCL.Public.Module.App.SubModule;

public class InitUi
{
    public static void Dispatch()
    {
        Ui.Setter.SetAccentColor(Data.Setting.AccentColor);
        Application.Current.Resources["MainOpacity"] = 1.0;
        DisplaceDefaultUi();
    }

    public static void DisplaceDefaultUi()
    {
        if (YMCL.App.UiRoot is null) return;
        try
        {
            (Getter.FindControlByName(YMCL.App.UiRoot, "ContentGridBorder") as Border).Background = null;
            (Getter.FindControlByName(YMCL.App.UiRoot, "ContentGridBorder") as Border).BorderThickness =
                new Thickness(0);
            (Getter.FindControlByName(YMCL.App.UiRoot, "PART_PaneRoot") as Panel).Background =
                Application.Current.ActualThemeVariant == ThemeVariant.Dark
                    ? SolidColorBrush.Parse("#2c2c2c")
                    : SolidColorBrush.Parse("#FFE9F6FF");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        FAUISettings.SetAnimationsEnabledAtAppLevel(false); //关闭 FA 动画
    }
}