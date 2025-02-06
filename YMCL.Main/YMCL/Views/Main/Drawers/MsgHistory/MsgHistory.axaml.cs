using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YMCL.Views.Main.Drawers.MsgHistory;

public partial class MsgHistory : UserControl
{
    public MsgHistory()
    {
        InitializeComponent();
        DataContext = Data.UiProperty.MsgHistoryViewModel;
    }
}