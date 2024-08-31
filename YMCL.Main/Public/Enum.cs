namespace YMCL.Main.Public;

public enum Platform
{
    Linux,
    Windows,
    MacOs,
    Unknown
}

public enum Theme
{
    //System,
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

public enum WindowTitleBarStyle
{
    Unset,
    System,
    Ymcl
}

public enum CustomHomePageWay
{
    None,
    Local,
    Network,
    Presetting_JavaNews
}

public enum DaiYuLoaderType
{
    Any,
    Forge,
    NeoForge,
    Fabric,
    Quilt,
    LiteLoader
}

public enum ModSource
{
    Modrinth,
    CurseForge
}