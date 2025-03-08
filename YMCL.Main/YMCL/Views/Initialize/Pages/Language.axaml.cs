using CommunityToolkit.Mvvm.ComponentModel;
using YMCL.Public.Langs;

namespace YMCL.Views.Initialize.Pages;

public partial class Language : UserControl
{
    public Language()
    {
        InitializeComponent(); 
        DataContext = Data.SettingEntry;
    }
}