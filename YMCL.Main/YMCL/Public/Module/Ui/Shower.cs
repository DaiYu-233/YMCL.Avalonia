using Avalonia.Controls.Notifications;

namespace YMCL.Public.Module.Ui;

public class Shower
{
    public static void Toast(string msg, NotificationType type = NotificationType.Information, 
        bool time = true, string title = "Yu Minecraft Launcher")
    {
        var showTitle = Const.String.AppTitle;
        if (!string.IsNullOrWhiteSpace(title)) showTitle = title;
        if (time) showTitle += $" - {DateTime.Now:HH:mm:ss}";
        Data.Instance.Notification.Show(new Notification(showTitle, msg, type));
    }
    
    public static void ShowShortException(string msg, Exception ex)
    {
        Toast($"{msg}\n{ex.Message}", NotificationType.Error);
    }
}