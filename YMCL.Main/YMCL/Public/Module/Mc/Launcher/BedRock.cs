using Avalonia.Controls.Notifications;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Launcher;

public class BedRock
{
    public static void Launch(Control sender)
    {
        try
        {
            var launcher = TopLevel.GetTopLevel(sender).Launcher;
            launcher.LaunchUriAsync(new Uri("minecraft://play"));
        }
        catch
        {
            Notice($"{MainLang.LaunchFail} - {MainLang.BedRockVersion}", NotificationType.Error);
        }
    }
}