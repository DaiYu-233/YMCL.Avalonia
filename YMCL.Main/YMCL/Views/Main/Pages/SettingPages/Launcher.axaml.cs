using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Langs;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Launcher : UserControl
{
    public Launcher()
    {
        DataContext = Data.Instance;
        InitializeComponent();
        BindingEvent();
    }

    private void BindingEvent()
    {
        ExportButton.Click += async (_, _) =>
        {
            var ui = new CheckBox() { Content = "Ui" };
            var net = new CheckBox() { Content = "Net" };
            var launch = new CheckBox() { Content = "Launch" };
            var Other = new CheckBox() { Content = "Other" };
            var panel = new StackPanel()
            {
                Spacing = 5,
                Children =
                {
                    ui,
                    net,
                    launch,
                    Other
                }
            };
            var cr = await ShowDialogAsync(MainLang.Export, MainLang.ExportSettingTip, panel, b_primary: MainLang.Ok,
                b_cancel: MainLang.Cancel);
        };
    }
}