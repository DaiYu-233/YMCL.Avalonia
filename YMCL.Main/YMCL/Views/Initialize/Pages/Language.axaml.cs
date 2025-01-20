using CommunityToolkit.Mvvm.ComponentModel;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

namespace YMCL.Views.Initialize.Pages;

public partial class Language : UserControl
{
    public Language()
    {
        InitializeComponent(); 
        DataContext = Data.Setting;
    }
}