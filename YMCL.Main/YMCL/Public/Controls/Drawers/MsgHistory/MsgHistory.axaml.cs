namespace YMCL.Public.Controls.Drawers.MsgHistory;

public partial class MsgHistory : UserControl
{
    public MsgHistory()
    {
        InitializeComponent();
        DataContext = Data.UiProperty.MsgHistoryViewModel;
    }
}