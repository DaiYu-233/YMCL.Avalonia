using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Module.App;
using YMCL.Views.Initialize.Pages;

namespace YMCL.Public.Classes;

public class Setting : ReactiveObject
{
    [Reactive] [JsonProperty] public string Language { get; set; } = "Unset";
    [Reactive] [JsonProperty] public MinecraftFolder MinecraftFolder { get; set; } 
    [Reactive] [JsonProperty] public string SkipUpdateVersion { get; set; } = string.Empty;
    [Reactive] [JsonProperty] public bool EnableAutoCheckUpdate { get; set; } = true;
    [Reactive] [JsonProperty] public double MaximumDownloadThread { get; set; } = 64;
    [Reactive] [JsonProperty] public bool IsCompleteJavaInitialize { get; set; }
    [Reactive] [JsonProperty] public bool IsCompleteMinecraftFolderInitialize { get; set; }
    [Reactive] [JsonProperty] public bool IsCompleteAccountInitialize { get; set; }
    [Reactive] [JsonProperty] public Enum.Setting.OpenFileWay OpenFileWay { get; set; }

    [Reactive]
    [JsonProperty]
    public Enum.Setting.DownloadSource DownloadSource { get; set; } = Enum.Setting.DownloadSource.Mojang;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.LauncherVisibility LauncherVisibility { get; set; } =
        Enum.Setting.LauncherVisibility.AfterLaunchKeepLauncherVisible;

    [Reactive] [JsonProperty] public int AccountSelectionIndex { get; set; }
    [Reactive] [JsonProperty] public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Light;
    [Reactive] [JsonProperty] public double MaxMem { get; set; } = 1024;
    [Reactive] [JsonProperty] public int MaxFileFragmentation { get; set; } = 8;
    [Reactive] [JsonProperty] public double Volume { get; set; } = 50;
    [Reactive] [JsonProperty] public double DeskLyricSize { get; set; } = 16;
    [Reactive] [JsonProperty] public Enum.Setting.Repeat Repeat { get; set; } = Enum.Setting.Repeat.RepeatAll;
    [Reactive] [JsonProperty] public TextAlignment DeskLyricAlignment { get; set; } = TextAlignment.Center;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.LaunchCore LaunchCore { get; set; } = Enum.Setting.LaunchCore.MinecraftLaunch;

    [Reactive] [JsonProperty] public string Version { get; set; }
    [Reactive] [JsonProperty] public bool EnableIndependencyCore { get; set; } = true;
    [Reactive] [JsonProperty] public bool EnableCustomUpdateUrl { get; set; } = false;
    [Reactive] [JsonProperty] public string CustomUpdateUrl { get; set; } = "https://github.moeyy.xyz/{%url%}";
    [Reactive] [JsonProperty] public string MusicApi { get; set; } = "http://music.api.daiyu.fun/";

    [Reactive]
    [JsonProperty]
    public Enum.Setting.CustomBackGroundWay CustomBackGround { get; set; } = Enum.Setting.CustomBackGroundWay.Default;

    [Reactive] [JsonProperty] public bool EnableDisplayIndependentTaskWindow { get; set; } = false;
    [Reactive] [JsonProperty] public bool ShowGameOutput { get; set; } = false;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.WindowTitleBarStyle WindowTitleBarStyle { get; set; } = Enum.Setting.WindowTitleBarStyle.Unset;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.CustomHomePageWay CustomHomePage { get; set; } = Enum.Setting.CustomHomePageWay.None;

    [Reactive] [JsonProperty] public Color AccentColor { get; set; } = Color.Parse("#d64eff");
    [Reactive] [JsonProperty] public Color DeskLyricColor { get; set; } = Color.FromRgb(22, 233, 184);
    [Reactive] [JsonProperty] public JavaEntry Java { get; set; } = Data.AutoJava;
    [Reactive] [JsonProperty] public string WindowBackGroundImgData { get; set; } = string.Empty;

    public Setting()
    {
        PropertyChanged += (s, e) => { Method.SaveSetting(); };
    }
}