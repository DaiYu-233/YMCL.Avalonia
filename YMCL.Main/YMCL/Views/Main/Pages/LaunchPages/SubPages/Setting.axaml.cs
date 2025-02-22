using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public partial class Setting : UserControl
{
    public Setting(GameSettingModel model)
    {
        InitializeComponent();
        DataContext = model;
    }

    public Setting()
    {
    }
}