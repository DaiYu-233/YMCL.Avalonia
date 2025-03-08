using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Components.Parser;
using Newtonsoft.Json;
using ReactiveUI;
using Ursa.Controls;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using Language = YMCL.Public.Classes.Data.Language;
using WindowNotificationManager = Ursa.Controls.WindowNotificationManager;

namespace YMCL.Public.Const;

public sealed class Data : ReactiveObject
{
    private static Data? _instance;
    public static ObservableCollection<Language> Langs => LangHelper.Langs;
    public static DesktopRunnerType DesktopType { get; set; } = DesktopRunnerType.Unknown;
    public static WindowNotificationManager Notification { get; set; }
    public static WindowToastManager Toast { get; set; }
    public static ObservableCollection<MinecraftFolder> MinecraftFolders { get; set; }
    public static ObservableCollection<JavaEntry> JavaRuntimes { get; set; }
    public static ObservableCollection<string> EnablePlugins { get; set; } = [];
    public static ObservableCollection<PluginInfoEntry> IdentifiedPlugins { get; set; } = [];
    public static ObservableCollection<AccountInfo> Accounts { get; set; }
    public static ObservableCollection<FavouriteResource> FavouriteResources { get; set; } = [];
    public static ObservableCollection<MinecraftDataEntry> CurrentFolderGames { get; set; } = [];
    public static ObservableCollection<TaskEntry> TaskEntries { get; set; } = [];
    public static List<AggregateSearchEntry> AllAggregateSearchEntries { get; set; } = [];
    public static ObservableCollection<RecordSongEntry> RecordSongEntries { get; set; } = [];
    public static ObservableCollection<RecordSongEntry> SearchSongEntries { get; set; } = [];
    public static List<NewsDataListEntry> NewsDataList { get; set; } = [];
    public static SettingEntry SettingEntry { get; set; }
    public static UiProperty UiProperty { get; } = new();
    public static string TranslateToken { get; set; }

    public static Data Instance
    {
        get { return _instance ??= new Data(); }
    }

    public Data()
    {
        UiProperty.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(UiProperty.SelectedMinecraft) &&
                !string.IsNullOrEmpty(UiProperty.SelectedMinecraft?.Id))
            {
                SettingEntry.SelectedMinecraftId = UiProperty.SelectedMinecraft.Type == "bedrock"
                    ? "bedrock"
                    : UiProperty.SelectedMinecraft.Id;
            }
        };
        FavouriteResources.CollectionChanged += (_, _) =>
        {
            File.WriteAllText(ConfigPath.FavouriteResourceDataPath,
                JsonConvert.SerializeObject(FavouriteResources, Formatting.Indented));
        };
        SettingEntry.PropertyChanged += (_, e) =>
        {
            try
            {
                if (e.PropertyName != nameof(SettingEntry.SelectedMinecraftId) ||
                    string.IsNullOrEmpty(SettingEntry.SelectedMinecraftId)) return;
                if (SettingEntry.SelectedMinecraftId == "bedrock")
                {
                    UiProperty.SelectedMinecraft = new MinecraftDataEntry(null, true, true)
                        { IsSettingVisible = false, Type = "bedrock" };
                }
                else
                {
                    var parser = new MinecraftParser(SettingEntry.MinecraftFolder.Path);
                    UiProperty.SelectedMinecraft =
                        new MinecraftDataEntry(parser.GetMinecraft(SettingEntry.SelectedMinecraftId));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        };
    }
}