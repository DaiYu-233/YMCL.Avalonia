using System.Collections.Generic;
using Avalonia.Media;
using YMCL.Public.Classes.Data;

namespace YMCL.Public.Classes.Setting;

public record ExchangeSettingEntry
{
    public record Data
    {
        public NetworkSettings? NetworkSettings { get; set; }
        public LaunchSettings? LaunchSettings { get; set; }
        public UiSettings? UiSettings { get; set; }
        public OtherSettings? OtherSettings { get; set; }
        public IEnumerable<AccountInfo>? AccountSettings { get; set; }
    }

    public record NetworkSettings
    {
        public bool EnableAutoCheckUpdate { get; set; }
        public int MaxDownloadThread { get; set; }
        public string? CustomUpdateUrl { get; set; }
        public bool EnableCustomUpdateUrl { get; set; }
        public string? MusicApiWithIPAddress { get; set; }
        public Enum.Setting.DownloadSource DownloadSource { get; set; }
        public string? MusicApi { get; set; }
        public int MaxFileFragmentation { get; set; }
    }

    public record LaunchSettings
    {
        public double MaxMem { get; set; }
        public bool EnableIndependencyCore { get; set; }
    }

    public record UiSettings
    {
        public double TranslucentBackgroundOpacity { get; set; }
        public Enum.Setting.Theme Theme { get; set; }
        public Enum.Setting.LauncherVisibility LauncherVisibility { get; set; }
        public string? SpecialControlEnableTranslucent { get; set; }
        public int CornerRadius { get; set; }
        public string? CustomHomePageUrl { get; set; }
        public Color DeskLyricColor { get; set; }
        public Enum.Setting.NoticeWay NoticeWay { get; set; }
        public Color AccentColor { get; set; }
        public Language Language { get; set; }
        public Enum.Setting.CustomBackGroundWay CustomBackGround { get; set; }
        public double DeskLyricSize { get; set; }
        public TextAlignment DeskLyricAlignment { get; set; }
        public string? WindowBackGroundImgData { get; set; }
    }
    
    public record OtherSettings
    {
        public double Volume { get; set; }
        public Enum.Setting.Repeat Repeat { get; set; }
    }
}