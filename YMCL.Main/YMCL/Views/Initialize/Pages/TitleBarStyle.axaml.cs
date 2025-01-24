using YMCL.Public.Enum;

namespace YMCL.Views.Initialize.Pages;

public partial class TitleBarStyle : UserControl
{
    public TitleBarStyle()
    {
        InitializeComponent();
        WindowTitleBarStyleListBox.SelectedIndex = Data.Setting.WindowTitleBarStyle == Setting.WindowTitleBarStyle.Ymcl ? 1 : 0;
        WindowTitleBarStyleListBox.SelectionChanged += (_, _) =>
        {
            var toplevel = TopLevel.GetTopLevel(this);
            if (toplevel is not Window window) return;
            Public.Module.Ui.Setter.UpdateWindowStyle(window,
                WindowTitleBarStyleListBox.SelectedIndex == 1
                    ? Setting.WindowTitleBarStyle.Ymcl
                    : Setting.WindowTitleBarStyle.System);
            Data.Setting.WindowTitleBarStyle =
                WindowTitleBarStyleListBox.SelectedIndex == 1
                    ? Setting.WindowTitleBarStyle.Ymcl
                    : Setting.WindowTitleBarStyle.System;
        };
    }
}