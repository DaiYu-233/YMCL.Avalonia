using Avalonia.Media;
using Avalonia.Styling;
using Newtonsoft.Json.Serialization;

namespace YMCL.Public.Module.App;

public class InitUi
{
    public static void Dispatch()
    {
        Ui.Setter.SetAccentColor(Data.Instance.Setting.AccentColor);
        DisplaceDefaultUi();
    }

    public static void DisplaceDefaultUi()
    {
        if (YMCL.App.UiRoot is null) return;
        
            (Ui.Getter.FindControlByName(YMCL.App.UiRoot, "ContentGridBorder") as Border).Background = null;
            (Ui.Getter.FindControlByName(YMCL.App.UiRoot, "ContentGridBorder") as Border).BorderThickness =
                new Thickness(0);
            (Ui.Getter.FindControlByName(YMCL.App.UiRoot, "PART_PaneRoot") as Panel).Background =
                Application.Current.ActualThemeVariant == ThemeVariant.Dark
                    ? SolidColorBrush.Parse("#2c2c2c")
                    : SolidColorBrush.Parse("#FFE9F6FF");
        
    }
}