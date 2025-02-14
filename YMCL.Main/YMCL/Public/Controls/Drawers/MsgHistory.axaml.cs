using System.Linq;
using Avalonia.Interactivity;
using Ursa.Controls;
using YMCL.Public.Classes;

namespace YMCL.Public.Controls.Drawers;

public partial class MsgHistory : UserControl
{
    public MsgHistory()
    {
        InitializeComponent();
        DataContext = Data.Instance;
    }

    private void MessageCard_OnMessageClosed(object? sender, RoutedEventArgs e)
    {
        var card = sender as NotificationCard;
        var notification = UiProperty.NotificationCards
            .FirstOrDefault(x => x.Content == card.Content);
        if (notification is not null)
        {
            UiProperty.NotificationCards.Remove(notification);
        }
    }
}