using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Personalize : UserControl
{
    private ComboBox[] List;
    public Personalize()
    {
        InitializeComponent();
        List =
        [
            ThemeComboBox,
            LauncherVisibilityComboBox,
            LyricAlignComboBox,
            CustomHomePageComboBox,
            CustomBackGroundImgComboBox,
            WindowTitleBarStyleComboBox
        ];
    }
}