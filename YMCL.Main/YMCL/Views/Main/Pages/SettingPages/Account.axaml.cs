using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Semi.Avalonia.Locale;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Account : UserControl
{
    public Account()
    {
        InitializeComponent();
        BindingEvent();
        DataContext = Data.Instance;
    }

    private void BindingEvent()
    {
        AddAccountBtn.Click += (_, _) => { _ = Public.Module.Op.Account.AddByUi(this); };
        DelSelectedAccountBtn.Click += (_, _) => { Public.Module.Op.Account.RemoveSelected(); };
        RefreshMicrosoftSkinBtn.Click += (_, _) =>
        {
            _ = Public.Module.Op.Account.RefreshSelectedMicrosoftAccountSkin();
        };
    }
}