using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Download : UserControl
{
    public Download()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        MusicApiButton.Click += (_, _) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchUriAsync(new Uri("https://gitlab.com/Binaryify/neteasecloudmusicapi"));
        };
    }
}