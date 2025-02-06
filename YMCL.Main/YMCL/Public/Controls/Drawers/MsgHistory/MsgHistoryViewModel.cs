using System.Collections.ObjectModel;
using Avalonia.Controls.Notifications;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Drawers.MsgHistory;

public class MsgHistoryViewModel : NotifyPropertyModelBase
{
    public ObservableCollection<NotificationCard> NotificationCards { get; set; } = [];
}