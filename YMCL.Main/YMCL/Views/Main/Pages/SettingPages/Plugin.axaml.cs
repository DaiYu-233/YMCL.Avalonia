using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Plugin : UserControl
{
    public Plugin()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        Data.IdentifiedPlugins.CollectionChanged += (_, _) =>
        {
            NoTasksTip.IsVisible = Data.IdentifiedPlugins.Count == 0;
        };
    }
}