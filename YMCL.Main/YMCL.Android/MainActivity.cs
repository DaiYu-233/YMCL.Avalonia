using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using YMCL.Public.Const;
using YMCL.Public.Enum;

namespace YMCL.Android;

[Activity(
    Label = "YMCL.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Data.Instance.RunnerType = RunnerType.Android;
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Context context = this;
        var externalFilesDir = context.GetExternalFilesDir(null)?.AbsolutePath;
        if (!string.IsNullOrWhiteSpace(externalFilesDir))
        {
            ConfigPath.InitPath();
        }

        base.OnCreate(savedInstanceState);
    }
}
