using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using YMCL.Views.Initialize.Pages;

namespace YMCL.Public.Classes;

public class Setting
{
    public string Language { get; set; } = "Unset";
    public MinecraftFolder MinecraftFolder { get; set; }
    public string SkipUpdateVersion { get; set; } = string.Empty;
    public bool EnableAutoCheckUpdate { get; set; } = true;
    public double MaximumDownloadThread { get; set; } = 64;
    public bool IsCompleteJavaInitialize { get; set; }
    public bool IsCompleteMinecraftFolderInitialize { get; set; }
    public bool IsCompleteAccountInitialize { get; set; }
    public Enum.Setting.OpenFileWay OpenFileWay { get; set; }
    public Enum.Setting.DownloadSource DownloadSource { get; set; } = Enum.Setting.DownloadSource.Mojang;
    public Enum.Setting.LauncherVisibility LauncherVisibility { get; set; } = Enum.Setting.LauncherVisibility.AfterLaunchKeepLauncherVisible;
    public int AccountSelectionIndex { get; set; }
    public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Light;
    public double MaxMem { get; set; } = 1024;
    public int MaxFileFragmentation { get; set; } = 8;
    public double Volume { get; set; } = 50;
    public double DeskLyricSize { get; set; } = 16;
    public Enum.Setting.Repeat Repeat { get; set; } = Enum.Setting.Repeat.RepeatAll;
    public TextAlignment DeskLyricAlignment { get; set; } = TextAlignment.Center;
    public Enum.Setting.LaunchCore LaunchCore { get; set; } = Enum.Setting.LaunchCore.MinecraftLaunch;
    public string Version { get; set; }
    public bool EnableIndependencyCore { get; set; } = true;
    public bool EnableCustomUpdateUrl { get; set; } = false;
    public string CustomUpdateUrl { get; set; } = "https://github.moeyy.xyz/{%url%}";
    public string MusicApi { get; set; } = "http://music.api.daiyu.fun/";
    public Enum.Setting.CustomBackGroundWay CustomBackGround { get; set; } = Enum.Setting.CustomBackGroundWay.Default;
    public bool EnableDisplayIndependentTaskWindow { get; set; } = false;
    public bool ShowGameOutput { get; set; } = false;
    public Enum.Setting.WindowTitleBarStyle WindowTitleBarStyle { get; set; } = Enum.Setting.WindowTitleBarStyle.Unset;
    public Enum.Setting.CustomHomePageWay CustomHomePage { get; set; } = Enum.Setting.CustomHomePageWay.None;
    public Color AccentColor { get; set; } = Color.Parse("#d64eff");
    public Color DeskLyricColor { get; set; } = Color.FromRgb(22, 233, 184);
    public JavaEntry Java { get; set; } = new() { JavaPath = "Auto", JavaVersion = "All" };
    public string WindowBackGroundImgData { get; set; } = string.Empty;
}
