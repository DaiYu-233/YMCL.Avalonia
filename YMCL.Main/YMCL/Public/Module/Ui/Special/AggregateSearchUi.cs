﻿using System.IO;
using System.Linq;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Components.Parser;
using Modrinth.Models;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using SearchResult = YMCL.Views.Main.Pages.DownloadPages.ModrinthPages.SearchResult;

namespace YMCL.Public.Module.Ui.Special;

public class AggregateSearchUi
{
    public static void UpdateAllAggregateSearchEntries()
    {
        Data.AllAggregateSearchEntries.Clear();
        foreach (var entry in UiProperty.AllInstallableGames)
        {
            Data.AllAggregateSearchEntries.Add(new AggregateSearchEntry
            {
                Tag = "jump", Text = $"{MainLang.ReleaseVersion} - {entry.Id}",
                Type = MainLang.InstallVersion, Target = "auto-install",
                Summary =
                    $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-{MainLang.AutoInstall}")}",
                Order = 70, VersionManifestEntry = entry
            });
        }

        foreach (var folder in Data.MinecraftFolders)
        {
            var parser = new MinecraftParser(folder.Path);
            var entries = parser.GetMinecrafts();
            foreach (var game in entries)
            {
                Data.AllAggregateSearchEntries.Add(new AggregateSearchEntry
                {
                    Tag = "change-selection", Text = $"{MainLang.GameVersion} - {game.Id}",
                    Type = MainLang.LocalResource, Target = "mc-version",
                    Summary = string.IsNullOrWhiteSpace(game.ClientJarPath)
                        ? "Unknown"
                        : Mc.Utils.GetMinecraftSpecialFolder(game, GameSpecialFolder.GameFolder),
                    MinecraftEntry = game, Order = 30
                });
            }
        }

        foreach (var account in Data.Accounts)
        {
            Data.AllAggregateSearchEntries.Add(new AggregateSearchEntry
            {
                Tag = "change-selection", Text = $"{MainLang.ExistAccount} - {account.Name}",
                Type = MainLang.LocalResource, Account = account,
                Summary = $"{account.AccountType} - {account.AddTime}", Order = 20, Target = "account"
            });
        }

        foreach (var java in Data.JavaRuntimes)
        {
            Data.AllAggregateSearchEntries.Add(new AggregateSearchEntry
            {
                Tag = "change-selection", Text = $"Java - {java.JavaStringVersion}", Type = MainLang.LocalResource,
                Summary = java.JavaPath, Order = 50
            });
        }

        Data.AllAggregateSearchEntries = Data.AllAggregateSearchEntries.OrderBy(x => x.Order).ToList();
        Filter(Data.UiProperty.AggregateSearchFilter);
    }

    public static void Filter(string filter)
    {
        UiProperty.Instance.FilteredAggregateSearchEntries.Clear();
        if (!string.IsNullOrWhiteSpace(filter))
        {
            UiProperty.Instance.FilteredAggregateSearchEntries.Add(new AggregateSearchEntry
                {
                    Tag = "jump", Text = $"{MainLang.SearchInTip.Replace("{target}", "CurseForge")} : {filter.Trim()}",
                    Type = MainLang.JumpSearch, Keyword = filter.Trim(),
                    Summary = $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-CurseForge")}",
                    Target = "curse-forge", Order = 10
                }
            );
            UiProperty.Instance.FilteredAggregateSearchEntries.Add(new AggregateSearchEntry
                {
                    Tag = "jump", Text = $"{MainLang.SearchInTip.Replace("{target}", "Modrinth")} : {filter.Trim()}",
                    Type = MainLang.JumpSearch, Keyword = filter.Trim(),
                    Summary = $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-Modrinth")}",
                    Target = "modrinth", Order = 10
                }
            );
            if (Data.DesktopType == DesktopRunnerType.Windows)
                UiProperty.Instance.FilteredAggregateSearchEntries.Add(new AggregateSearchEntry
                {
                    Tag = "jump",
                    Text = $"{MainLang.SearchInTip.Replace("{target}", MainLang.Music)} : {filter.Trim()}",
                    Type = MainLang.JumpSearch, Keyword = filter.Trim(),
                    Summary = $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Music}")}", Target = "music",
                    Order = 10
                });
        }

        Data.AllAggregateSearchEntries.Where(item =>
                item.Text.ToLower().Contains(filter.ToLower(), StringComparison.OrdinalIgnoreCase))
            .ToList().OrderBy(x => x.Order).ToList()
            .ForEach(item => UiProperty.Instance.FilteredAggregateSearchEntries.Add(item));
    }

    public static void HandleSelectedEntry(AggregateSearchEntry entry)
    {
        if (entry.Tag == "jump" && !string.IsNullOrWhiteSpace(entry.Target))
        {
            var key = UiProperty.Instance.AggregateSearchFilter;
            if (entry.Target == "curse-forge")
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                if (string.IsNullOrWhiteSpace(entry.Keyword)) return;
                YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavDownload;
                YMCL.App.UiRoot.ViewModel.Download.Nav.SelectedItem = YMCL.App.UiRoot.ViewModel.Download.NavCf;
                YMCL.App.UiRoot.ViewModel.Download._curseForge.CreateNewPage(new SearchTabViewItemEntry()
                {
                    CanClose = true, Host = nameof(Views.Main.Pages.DownloadPages.CurseForge),
                    Content = new Views.Main.Pages.DownloadPages.CurseForgePages.SearchResult(entry.Keyword,
                        string.Empty, 0, 0),
                    Title = $"{MainLang.Search}({MainLang.AllType}): {entry.Keyword}"
                });
            }

            if (entry.Target == "modrinth")
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                if (string.IsNullOrWhiteSpace(entry.Keyword)) return;
                YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavDownload;
                YMCL.App.UiRoot.ViewModel.Download.Nav.SelectedItem = YMCL.App.UiRoot.ViewModel.Download.NavMr;
                YMCL.App.UiRoot.ViewModel.Download._modrinth.CreateNewPage(new SearchTabViewItemEntry()
                {
                    CanClose = true, Host = nameof(Views.Main.Pages.DownloadPages.Modrinth),
                    Content = new Views.Main.Pages.DownloadPages.ModrinthPages.SearchResult(entry.Keyword, string.Empty,
                        0),
                    Title = $"{MainLang.Search}({MainLang.AllType}): {entry.Keyword}"
                });
            }
            else if (entry.Target == "music")
            {
                if (string.IsNullOrWhiteSpace(key)) return;
                if (string.IsNullOrWhiteSpace(entry.Keyword)) return;
                YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavMusic;
                YMCL.App.UiRoot.ViewModel.Music.SearchFormCall(entry.Keyword);
            }
            else if (entry.Target == "auto-install")
            {
                if (string.IsNullOrWhiteSpace(entry.VersionManifestEntry.Id)) return;
                YMCL.App.UiRoot.ViewModel.Download.Nav.SelectedItem = YMCL.App.UiRoot.ViewModel.Download.NavAuto;
                YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavDownload;
                YMCL.App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview(entry.VersionManifestEntry);
            }
        }
        else if (entry.Tag == "change-selection" && !string.IsNullOrWhiteSpace(entry.Target))
        {
            if (entry is { Target: "mc-version", MinecraftEntry: not null })
            {
                if (entry.Summary == "Unknown")
                {
                    Notice(MainLang.CannotRecognitionTheVersion, NotificationType.Error);
                }
                else
                {
                    Const.Data.SettingEntry.MinecraftFolder =
                        Data.MinecraftFolders.FirstOrDefault(x => x.Path == entry.MinecraftEntry.MinecraftFolderPath);
                    Const.Data.UiProperty.SelectedMinecraft = new MinecraftDataEntry(entry.MinecraftEntry);
                    YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavLaunch;
                    Notice(
                        $"{MainLang.SwitchedTo} : {entry.MinecraftEntry.Id}\n" +
                        $"( {Mc.Utils.GetMinecraftSpecialFolder(entry.MinecraftEntry, GameSpecialFolder.GameFolder, true)} )",
                        type: NotificationType.Success);
                }
            }
            else if (entry is { Target: "account" })
            {
                Const.Data.SettingEntry.Account = entry.Account;
                Notice($"{MainLang.SwitchedTo} : {entry.Text}\n{entry.Summary}",
                    type: NotificationType.Success);
            }
        }
    }
}