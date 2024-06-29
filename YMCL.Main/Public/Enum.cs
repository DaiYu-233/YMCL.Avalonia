namespace YMCL.Main.Public
{
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
    public enum OpenFileWay
    {
        FileSelectWindow,
        ManualInput
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
        Network
    }
}
