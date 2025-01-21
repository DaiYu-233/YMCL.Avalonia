namespace YMCL.Public.Module.Mc.Launcher;

public class LaunchBedRockByUrlScheme
{
    public static void Launch(Control sender)
    {
        var launcher = TopLevel.GetTopLevel(sender).Launcher;
        launcher.LaunchUriAsync(new Uri("minecraft://play"));
    }
}