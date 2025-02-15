namespace YMCL.Public.Enum;

public class Setting
{
    public enum Theme
    {
        System,
        Light,
        Dark
    }

    public enum Repeat
    {
        RepeatOff,
        RepeatAll,
        RepeatOne
    }

    public enum LaunchCore
    {
        MinecraftLaunch,
        StarLight
    }

    public enum LauncherVisibility
    {
        AfterLaunchKeepLauncherVisible,
        AfterLaunchMakeLauncherMinimize,
        AfterLaunchMinimizeAndShowWhenGameExit,
        AfterLaunchHideAndShowWhenGameExit,
        AfterLaunchExitLauncher
    }

    public enum OpenFileWay
    {
        FileSelectWindow,
        ManualInput
    }

    public enum DownloadSource
    {
        Mojang,
        BmclApi
    }


    public enum WindowTitleBarStyle
    {
        System,
        Ymcl,
        Unset
    }

    public enum CustomHomePageWay
    {
        None,
        Local,
        Network,
        Presetting_JavaNews
    }


    public enum CustomBackGroundWay
    {
        Default,
        Image,
        AcrylicBlur,
        Transparent,
        Mica
    }

    public enum AccountType
    {
        Offline,
        Microsoft,
        ThirdParty
    }

    public enum VersionSettingEnableIndependencyCore
    {
        Global,
        Off,
        On
    }

    public enum NoticeWay
    {
        Bubble,
        Card
    }
}