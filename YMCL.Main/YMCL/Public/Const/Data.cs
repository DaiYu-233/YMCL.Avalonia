using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Classes.Models.Game;
using ReactiveUI;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.ViewModels;
using YMCL.Views.Initialize.Pages;
using YMCL.Views.Main.Pages.MorePages;
using Language = YMCL.Public.Classes.Language;
using Setting = YMCL.Public.Classes.Setting;

namespace YMCL.Public.Const;

public sealed class Data : ReactiveObject
{
    private static Data? _instance;
    public static ObservableCollection<Language> Langs => LangHelper.Langs;
    public static DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.Unknown;
    public static WindowNotificationManager Notification { get; set; }
    public static ObservableCollection<Classes.MinecraftFolder> MinecraftFolders { get; set; }
    public static ObservableCollection<JavaEntry> JavaRuntimes { get; set; }
    public static ObservableCollection<AccountInfo> Accounts { get; set; }
    public static ObservableCollection<GameDataEntry> CurrentFolderGames { get; set; } = [];
    public static ObservableCollection<TaskEntry> TaskEntries  { get; set; } = [];
    public static List<AggregateSearchEntry> AllAggregateSearchEntries  { get; set; } = [];
    public static ObservableCollection<RecordSongEntry> RecordSongEntries  { get; set; } = [];
    public static ObservableCollection<RecordSongEntry> SearchSongEntries { get; set; }= [];

    public static List<NewsDataListEntry> NewsDataList { get; set; } = [];
    public static Setting Setting { get; set; }
    public static UiProperty UiProperty { get; } = new();
    public static string TranslateToken { get; set; }

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }
}