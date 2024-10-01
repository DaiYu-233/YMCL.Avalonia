using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;

#pragma warning disable CS8618
namespace YMCL.Main.Public.Classes;

public class Setting
{
    public string Language { get; set; } = "Unset";
    public string MinecraftFolder { get; set; }
    public string SkipUpdateVersion { get; set; } = string.Empty;
    public bool EnableAutoCheckUpdate { get; set; } = true;
    public double MaximumDownloadThread { get; set; } = 64;
    public bool IsCompleteJavaInitialize { get; set; }
    public bool IsCompleteMinecraftFolderInitialize { get; set; }
    public bool IsCompleteAccountInitialize { get; set; }
    public OpenFileWay OpenFileWay { get; set; }
    public DownloadSource DownloadSource { get; set; } = DownloadSource.Mojang;
    public LauncherVisibility LauncherVisibility { get; set; } = LauncherVisibility.AfterLaunchKeepLauncherVisible;
    public int AccountSelectionIndex { get; set; }
    public Theme Theme { get; set; } = Theme.Light;
    public double MaxMem { get; set; } = 1024;
    public double Volume { get; set; } = 50;
    public double DeskLyricSize { get; set; } = 16;
    public Repeat Repeat { get; set; } = Repeat.RepeatAll;
    public TextAlignment DeskLyricAlignment { get; set; } = TextAlignment.Center;
    public LaunchCore LaunchCore { get; set; } = LaunchCore.MinecraftLaunch;
    public string Version { get; set; }
    public bool EnableIndependencyCore { get; set; } = true;
    public bool EnableCustomUpdateUrl { get; set; } = false;
    public string CustomUpdateUrl { get; set; } = "https://gh.api.99988866.xyz/{%url%}";
    public string MusicApi { get; set; } = "http://music.api.daiyu.fun/";
    public CustomBackGroundWay CustomBackGround { get; set; } = CustomBackGroundWay.Default;
    public bool EnableDisplayIndependentTaskWindow { get; set; } = false;
    public bool ShowGameOutput { get; set; } = false;
    public WindowTitleBarStyle WindowTitleBarStyle { get; set; } = WindowTitleBarStyle.Unset;
    public CustomHomePageWay CustomHomePage { get; set; } = CustomHomePageWay.None;
    public Color AccentColor { get; set; } = Color.Parse("#d64eff");
    public Color DeskLyricColor { get; set; } = Color.FromRgb(22, 233, 184);
    public JavaEntry Java { get; set; } = Const.Data.AutoJava;
    public string WindowBackGroundImgData { get; set; } = string.Empty;
}

public class VersionSetting
{
    public VersionSettingEnableIndependencyCore EnableIndependencyCore { get; set; } =
        VersionSettingEnableIndependencyCore.Global;

    public JavaEntry Java { get; set; } = new() { JavaPath = "Global" };
    public double MaxMem { get; set; } = -1; // -1 = Global
    public string AutoJoinServerIp { get; set; } = "";
}