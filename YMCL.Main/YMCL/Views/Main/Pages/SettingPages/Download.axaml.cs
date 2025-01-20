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
    }
}