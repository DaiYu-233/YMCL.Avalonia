using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;

#pragma warning disable CS8618
namespace YMCL.Main.Public.Classes;

public class Setting
{
    public Color AccentColor { get; set; } = Color.FromRgb(22, 233, 184);
    public string Language { get; set; } = "Unset";
    public string MinecraftFolder { get; set; }
    public double MaximumDownloadThread { get; set; }
    public OpenFileWay OpenFileWay { get; set; }
    public DownloadSource DownloadSource { get; set; } = DownloadSource.Mojang;
    public JavaEntry Java { get; set; } = Const.AutoJava;
    public int AccountSelectionIndex { get; set; }
    public Theme Theme { get; set; } = Theme.Light;
    public double MaxMem { get; set; } = 1024;
    public string Version { get; set; }
    public string WindowBackGroundImgData { get; set; } = string.Empty;
    public bool EnableIndependencyCore { get; set; } = true;
    public bool EnableCustomBackGroundImg { get; set; } = false;
    public bool AlreadyWrittenIntoTheUrlScheme { get; set; } = false;
    public bool ShowGameOutput { get; set; } = false;
    public WindowTitleBarStyle WindowTitleBarStyle { get; set; } = WindowTitleBarStyle.Unset;
    public CustomHomePageWay CustomHomePage { get; set; } = CustomHomePageWay.None;
}

public class VersionSetting
{
    public VersionSettingEnableIndependencyCore EnableIndependencyCore { get; set; } =
        VersionSettingEnableIndependencyCore.Global;

    public JavaEntry Java { get; set; } = new() { JavaPath = "Global" };
    public double MaxMem { get; set; } = -1; // -1 = Global
    public string AutoJoinServerIp { get; set; } = "";
}