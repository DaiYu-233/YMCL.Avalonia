using Avalonia.Media;

namespace YMCL.Public.Plugin;

public class BuildNavItemContent
{
    public static Control Build(string title, string iconPath)
    {
        var icon = new PathIcon
        {
            Data = Geometry.Parse(iconPath),
            Margin = new Thickness(0, 0, 6, 0),
            Width = 16,
            Height = 16
        };
        var text = new TextBlock
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            FontSize = 14,
            Text = title
        };
        var root = new DockPanel();
        root.Children.Add(icon);
        root.Children.Add(text);
        return root;
    }
    
    public static Control Build(string title, Control icon)
    {
        var text = new TextBlock
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            FontSize = 14,
            Text = title
        };
        var root = new DockPanel();
        root.Children.Add(icon);
        root.Children.Add(text);
        return root;
    }
}