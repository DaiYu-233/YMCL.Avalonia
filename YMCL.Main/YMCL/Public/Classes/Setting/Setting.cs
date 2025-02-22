using Avalonia.Media;
using Avalonia.Threading;
using MinecraftLaunch;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.Init.SubModule;
using YMCL.Public.Module.Ui.Special;
using YMCL.Views.Main;
using Language = YMCL.Public.Classes.Data.Language;
using MinecraftFolder = YMCL.Public.Classes.Data.MinecraftFolder;
using Setter = YMCL.Public.Module.Ui.Setter;

namespace YMCL.Public.Classes.Setting;

public class Setting : ReactiveObject
{
    [Reactive] [JsonProperty] public string SkipUpdateVersion { get; set; } = string.Empty;
    [Reactive] [JsonProperty] public bool EnableAutoCheckUpdate { get; set; } = true;
    [Reactive] [JsonProperty] public int MaxDownloadThread { get; set; } = 64;
    [Reactive] [JsonProperty] public double TranslucentBackgroundOpacity { get; set; } = 0.68;
    [Reactive] [JsonProperty] public bool IsCompleteJavaInitialize { get; set; }
    [Reactive] [JsonProperty] public bool IsCompleteMinecraftFolderInitialize { get; set; }
    [Reactive] [JsonProperty] public bool IsCompleteAccountInitialize { get; set; }
    [Reactive] [JsonProperty] public string CustomHomePageUrl { get; set; }
    [Reactive] [JsonProperty] public Enum.Setting.OpenFileWay OpenFileWay { get; set; }
    [Reactive] [JsonProperty] public Enum.Setting.NoticeWay NoticeWay { get; set; } = Enum.Setting.NoticeWay.Bubble;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.DownloadSource DownloadSource { get; set; } = Enum.Setting.DownloadSource.Mojang;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.LauncherVisibility LauncherVisibility { get; set; } =
        Enum.Setting.LauncherVisibility.AfterLaunchKeepLauncherVisible;

    [Reactive] [JsonProperty] public Enum.Setting.Theme Theme { get; set; } = Enum.Setting.Theme.Dark;
    [Reactive] [JsonProperty] public double MaxMem { get; set; } = 1024;
    [Reactive] [JsonProperty] public int MaxFileFragmentation { get; set; } = 8;
    [Reactive] [JsonProperty] public int CornerRadius { get; set; } = 5;
    [Reactive] [JsonProperty] public double Volume { get; set; } = 50;
    [Reactive] [JsonProperty] public double DeskLyricSize { get; set; } = 16;
    [Reactive] [JsonProperty] public Enum.Setting.Repeat Repeat { get; set; } = Enum.Setting.Repeat.RepeatAll;
    [Reactive] [JsonProperty] public TextAlignment DeskLyricAlignment { get; set; } = TextAlignment.Center;

    [Reactive]
    [JsonProperty]
    public Enum.Setting.LaunchCore LaunchCore { get; set; } = Enum.Setting.LaunchCore.MinecraftLaunch;

    [Reactive] [JsonProperty] public string SelectedMinecraftId { get; set; }
    [Reactive] [JsonProperty] public bool EnableIndependencyCore { get; set; } = true;
    [Reactive] [JsonProperty] public bool EnableCustomUpdateUrl { get; set; }
    [Reactive] [JsonProperty] public string CustomUpdateUrl { get; set; } = "https://github.moeyy.xyz/{%url%}";
    [Reactive] [JsonProperty] public string MusicApi { get; set; } = "http://music.api.daiyu.fun/";

    [Reactive]
    [JsonProperty]
    public string SpecialControlEnableTranslucent { get; set; } =
        "ContentDialog,NotificationCard,NotificationBubble,Popup";

    [Reactive]
    [JsonProperty]
    public Enum.Setting.CustomBackGroundWay CustomBackGround { get; set; } = Enum.Setting.CustomBackGroundWay.Default;

    [Reactive] [JsonProperty] public bool EnableDisplayIndependentTaskWindow { get; set; }
    [Reactive] [JsonProperty] public bool ShowGameOutput { get; set; }

    [Reactive]
    [JsonProperty]
    public Enum.Setting.CustomHomePageWay CustomHomePage { get; set; } = Enum.Setting.CustomHomePageWay.None;

    [Reactive] [JsonProperty] public string MusicApiWithIPAddress { get; set; } = "120.230.112.69";

    [Reactive] [JsonProperty] public Language Language { get; set; } = new();
    [Reactive] [JsonProperty] public MinecraftFolder MinecraftFolder { get; set; }

    [Reactive] [JsonProperty] public Color AccentColor { get; set; } = Color.Parse("#00b2ff");
    [Reactive] [JsonProperty] public Color DeskLyricColor { get; set; } = Color.Parse("#00b2ff");

    [Reactive]
    [JsonProperty]
    public JavaEntry Java { get; set; } = new() { JavaPath = MainLang.LetYMCLChooseJava, JavaStringVersion = "Auto" };

    [Reactive]
    [JsonProperty]
    public AccountInfo Account { get; set; } = new()
    {
        Name = "Steve", AccountType = Enum.Setting.AccountType.Offline,
        AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
    };

    [Reactive] [JsonProperty] public string WindowBackGroundImgData { get; set; } = string.Empty;

    public Setting()
    {
        var accentColorSetter = new Debouncer(() => { Public.Module.Ui.Setter.SetAccentColor(AccentColor); }, 5);
        var _setBackGroundDebouncer = new Debouncer(
            () => { Dispatcher.UIThread.Invoke(Public.Module.Ui.Setter.SetBackGround); },
            500);
        PropertyChanged += (o, e) =>
        {
            if (e.PropertyName == nameof(Language))
            {
                LangHelper.Current.ChangedCulture(Const.Data.Setting.Language.Code);
            }

            if (e.PropertyName == nameof(AccentColor))
            {
                accentColorSetter.Trigger();
            }

            if (e.PropertyName == nameof(MinecraftFolder))
            {
                LaunchUi.LoadGames();
            }

            if (e.PropertyName == nameof(CustomHomePage))
            {
                _ = InitUi.SetCustomHomePage();
            }

            if (e.PropertyName == nameof(CustomBackGround))
            {
                try
                {
                    Setter.SetBackGround();
                }
                catch
                {
                }
            }

            if (e.PropertyName == nameof(TranslucentBackgroundOpacity))
            {
                Application.Current.Resources["MainOpacity"] = TranslucentBackgroundOpacity;
                if (TopLevel.GetTopLevel(App.UiRoot) is MainWindow window)
                {
                    try
                    {
                        (Public.Module.Ui.Getter.FindControlByName(window, "PART_PaneRoot") as Panel).Opacity =
                            (double)Application.Current.Resources["MainOpacity"]!;
                    }
                    catch
                    {
                    }
                }

                _setBackGroundDebouncer.Trigger();
            }

            if (e.PropertyName == nameof(CornerRadius))
            {
                Application.Current.Resources["MainCornerRadius"] = new CornerRadius(CornerRadius);
            }

            if (e.PropertyName == nameof(SpecialControlEnableTranslucent))
            {
                Public.Module.Ui.Setter.DynamicStyle.SetDynamicStyle();
            }

            if (e.PropertyName == nameof(Theme))
            {
                Public.Module.Ui.Setter.ToggleTheme(Theme);
            }

            if (e.PropertyName == nameof(MaxDownloadThread))
            {
                DownloadMirrorManager.MaxThread = MaxDownloadThread;
            }

            AppMethod.SaveSetting();
        };
    }
}