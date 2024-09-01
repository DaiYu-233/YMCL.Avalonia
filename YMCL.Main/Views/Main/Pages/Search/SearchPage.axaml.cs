using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Resolver;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Search;

public partial class SearchPage : UserControl
{
    private Method.Debouncer _debouncer;
    public List<AggregateSearch> aggregateSearchList = new();
    public List<VersionManifestEntry> versionManifestEntries = null;

    public SearchPage()
    {
        InitializeComponent();
        InitAggregateSearch();
        UpdateAggregateSearch();
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            LoadSearcgItems();
            UpdateAggregateSearch();
        };
        AggregateSearchBox.TextChanged += (_, _) => { UpdateAggregateSearch(); };
    }

    public void UpdateAggregateSearch()
    {
        var text = AggregateSearchBox.Text;
        AggregateSearchListBox.Items.Clear();
        if (string.IsNullOrWhiteSpace(text))
        {
            aggregateSearchList.ForEach(i => { AggregateSearchListBox.Items.Add(i); });
        }
        else
        {
            var keys = text.Split(' ').Select(keyword => keyword.ToLower()).ToList();
            AggregateSearchListBox.Items.Add(new AggregateSearch()
                {
                    Tag = "jump", Text = $"{MainLang.SearchInTip.Replace("{target}", "CurseForge")} : {text.Trim()}",
                    Type = MainLang.JumpSearch,
                    Summary = $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-CurseForge")}",
                    Target = "curse-forge", Order = 10
                }
            );
            AggregateSearchListBox.Items.Add(new AggregateSearch()
            {
                Tag = "jump", Text = $"{MainLang.SearchInTip.Replace("{target}", MainLang.Music)} : {text.Trim()}",
                Type = MainLang.JumpSearch,
                Summary = $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Music}")}", Target = "music",
                Order = 10
            });
            aggregateSearchList.ForEach(i =>
            {
                if (i != null && keys.Any(keyword => i.Text.ToLower().Contains(keyword)))
                {
                    AggregateSearchListBox.Items.Add(i);
                }
            });
        }
    }

    private void InitAggregateSearch()
    {
        _debouncer = new Method.Debouncer(async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (AggregateSearchListBox.SelectedItem is not AggregateSearch entry) return;
                if (entry.Tag == "jump" && !string.IsNullOrWhiteSpace(entry.Target))
                {
                    var key = Const.Window.main.downloadPage.curseForgeFetcherPage.ModNameTextBox.Text =
                        AggregateSearchBox.Text;
                    if (entry.Target == "curse-forge")
                    {
                        if (string.IsNullOrWhiteSpace(key)) return;
                        Const.Window.main.Nav.SelectedItem = Const.Window.main.NavDownload;
                        Const.Window.main.downloadPage.Nav.SelectedItem =
                            Const.Window.main.downloadPage.Nav.MenuItems[1];
                        Const.Window.main.downloadPage.curseForgeFetcherPage.ResultTypeComboBox.SelectedIndex = 0;
                        Const.Window.main.downloadPage.curseForgeFetcherPage.ModNameTextBox.Text = key.Trim();
                        Const.Window.main.downloadPage.curseForgeFetcherPage.ExternalCallSearchModFromCurseForge();
                    }
                    else if (entry.Target == "music")
                    {
                        if (string.IsNullOrWhiteSpace(key)) return;
                        Const.Window.main.Nav.SelectedItem = Const.Window.main.NavMusic;
                        Const.Window.main.musicPage.SearchBox.Text = key.Trim();
                        Const.Window.main.musicPage.ExternalCall();
                    }
                    else if (entry.Target == "auto-install")
                    {
                        if (string.IsNullOrWhiteSpace(entry.InstallVersionId)) return;
                        Const.Window.main.Nav.SelectedItem = Const.Window.main.NavDownload;
                        Const.Window.main.downloadPage.Nav.SelectedItem =
                            Const.Window.main.downloadPage.Nav.MenuItems[0];
                        Const.Window.main.downloadPage.autoInstallPage.ExternalCall(entry.InstallVersionId);
                    }
                }
                else if (entry.Tag == "change-selection" && !string.IsNullOrWhiteSpace(entry.Target))
                {
                    if (entry is { Target: "mc-version", GameEntry: not null })
                    {
                        if (entry.Summary == "Unknown")
                        {
                            Method.Ui.Toast(MainLang.CannotRecognitionTheVersion, type: NotificationType.Error);
                        }
                        else
                        {
                            Const.Data.Setting.MinecraftFolder = entry.GameEntry.GameFolderPath;
                            Const.Data.Setting.Version = entry.GameEntry.Id;
                            File.WriteAllText(Const.String.SettingDataPath,
                                JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
                            Const.Window.main.Nav.SelectedItem = Const.Window.main.NavLaunch;
                            Method.Ui.Toast(
                                $"{MainLang.SwitchedTo} : {entry.GameEntry.Id}\n( {Path.GetDirectoryName(entry.GameEntry.JarPath)} )",
                                type: NotificationType.Success);
                        }
                    }
                    else if (entry is { Target: "account" })
                    {
                        Const.Data.Setting.AccountSelectionIndex = entry.AccountIndex;
                        File.WriteAllText(Const.String.SettingDataPath,
                            JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
                        Method.Ui.Toast(
                            $"{MainLang.SwitchedTo} : {entry.Text}\n{entry.Summary}",
                            type: NotificationType.Success);
                    }
                }

                await Task.Delay(100);
                AggregateSearchBox.Text = String.Empty;
            });
        }, 10);

        AggregateSearchListBox.SelectionChanged += async (_, _) =>
        {
            if (AggregateSearchListBox.SelectedIndex == -1) return;
            _debouncer.Trigger();
            await Task.Delay(250);
            AggregateSearchListBox.SelectedIndex = -1;
        };
    }

    private void LoadSearcgItems()
    {
        aggregateSearchList.Clear();
        JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath))!.ForEach(
            path =>
            {
                var gameResolver = new GameResolver(path);
                gameResolver.GetGameEntitys().ToList().ForEach(v =>
                {
                    aggregateSearchList.Add(new AggregateSearch()
                    {
                        Tag = "change-selection", Text = $"{MainLang.GameVersion} - {v.Id}",
                        Type = MainLang.LocalResource, Target = "mc-version",
                        Summary = Path.GetDirectoryName(v.JarPath) ?? "Unknown", GameEntry = v, Order = 30
                    });
                });
            });

        var accountIndex = 0;
        JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath))!.ForEach(
            a =>
            {
                aggregateSearchList.Add(new AggregateSearch()
                {
                    Tag = "change-selection", Text = $"{MainLang.ExistAccount} - {a.Name}",
                    Type = MainLang.LocalResource, AccountIndex = accountIndex,
                    Summary = $"{a.AccountType} - {a.AddTime}", Order = 20, Target = "account"
                });
                accountIndex++;
            });

        JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath))!.ForEach(
            e =>
            {
                aggregateSearchList.Add(new AggregateSearch()
                {
                    Tag = "change-selection", Text = $"Java - {e.JavaVersion}", Type = MainLang.LocalResource,
                    Summary = e.JavaPath ?? "Unknown", Order = 90
                });
            });

        if (versionManifestEntries != null)
        {
            versionManifestEntries.ForEach(item =>
            {
                switch (item.Type)
                {
                    case "release":
                        Const.Window.main.searchPage.aggregateSearchList.Add(new AggregateSearch()
                        {
                            Tag = "jump", Text = $"{MainLang.ReleaseVersion} - {item.Id}",
                            Type = MainLang.InstallVersion,
                            Target = "auto-install",
                            Summary =
                                $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-{MainLang.AutoInstall}")}",
                            Order = 70, InstallVersionId = item.Id
                        });
                        break;
                    case "snapshot":
                        Const.Window.main.searchPage.aggregateSearchList.Add(new AggregateSearch()
                        {
                            Tag = "jump", Text = $"{MainLang.PreviewVersion} - {item.Id}",
                            Type = MainLang.InstallVersion,
                            Target = "auto-install",
                            Summary =
                                $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-{MainLang.AutoInstall}")}",
                            Order = 70, InstallVersionId = item.Id
                        });
                        break;
                    default:
                        Const.Window.main.searchPage.aggregateSearchList.Add(new AggregateSearch()
                        {
                            Tag = "jump", Text = $"{MainLang.OldVersion} - {item.Id}", Type = MainLang.InstallVersion,
                            Target = "auto-install",
                            Summary =
                                $"{MainLang.JumpToSearchTip.Replace("{target}", $"{MainLang.Download}-{MainLang.AutoInstall}")}",
                            Order = 70, InstallVersionId = item.Id
                        });
                        break;
                }
            });
        }


        aggregateSearchList = aggregateSearchList.OrderBy(x => x.Order).ToList();
    }
}