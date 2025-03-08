using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CurseForge.APIClient;
using CurseForge.APIClient.Models.Files;
using DynamicData;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;
using ModLoaderType = CurseForge.APIClient.Models.Mods.ModLoaderType;

namespace YMCL.Public.Controls;

public partial class CurseForgeFileExpander : UserControl
{
    private readonly string _version;
    private bool _firstLoad = true;
    public ObservableCollection<File> Files { get; } = [];
    public ObservableCollection<CurseForgeResourceEntry> DependencyItems { get; } = [];
    public ObservableCollection<int> DependencyIds { get; } = [];

    public CurseForgeFileExpander(string version, int id, string name, ModLoaderType? loader, ResourceType type)
    {
        _version = version;
        InitializeComponent();
        Expander.Header = name;
        DataContext = this;
        Loaded += (_, _) =>
        {
            if (!_firstLoad) return;
            _firstLoad = false;
            _ = Init(version, id, loader);
        };
        ListView.SelectionChanged += async (_, _) =>
        {
            if (ListView.SelectedItem == null) return;
            Public.Module.Op.DownloadResource.SaveCurseForge(type, ListView.SelectedItem as File);
            await Task.Delay(300);
            ListView.SelectedItem = null;
        };
        ListBox.SelectionChanged += (_, _) =>
        {
            if (ListBox.SelectedItem == null) return;
            var item = ListBox.SelectedItem as CurseForgeResourceEntry;
            YMCL.App.UiRoot.ViewModel.Download._curseForge.CreateNewPage(new SearchTabViewItemEntry()
            {
                CanClose = true, Host = nameof(Views.Main.Pages.DownloadPages.CurseForge),
                Content = new ModFileResult(item!),
                Title = $"{item.DisplayType}: {item.Name}"
            });
            ListBox.SelectedItem = null;
        };
    }

    public CurseForgeFileExpander()
    {
    }

    private async Task Init(string version, int id, ModLoaderType? type)
    {
        ApiClient apiClient = new(Const.String.CurseForgeApiKey);
        var data = await apiClient.GetModFilesAsync(id, version, type ?? null);
        data.Data.ForEach(x =>
        {
            Files.Add(x);
            x.Dependencies.ForEach(y =>
            {
                if (y.RelationType == FileRelationType.RequiredDependency && !DependencyIds.Contains(y.ModId))
                {
                    DependencyIds.Add(y.ModId);
                }
            });
        });
        Ring.IsVisible = false;
        if (DependencyIds.Count != 0)
        {
            var tasks = new List<Task>();
            Dependencies.IsVisible = true;
            DependencyIds.ToList().ForEach(x => { tasks.Add(GetProject(x)); });
            await Task.WhenAll(tasks.ToArray());
            ListBox.IsVisible = true;
            DependenciesRing.IsVisible = false;
        }

        if (DependencyItems.Count == 0)
            DependenciesRing.IsVisible = false;
    }

    private async Task GetProject(int id)
    {
        ApiClient apiClient = new(Public.Const.String.CurseForgeApiKey);
        var res = await apiClient.GetModAsync(id);
        var item = res.Data;
        var conVersion = false;
        item.LatestFiles.ForEach(x =>
        {
            if (x.GameVersions.All(y => y != _version)) return;
            conVersion = true;
        });
        if (!conVersion) return;
        var entry = new CurseForgeResourceEntry()
        {
            Id = item.Id,
            GameId = item.GameId,
            Name = item.Name,
            Slug = item.Slug,
            Summary = item.Summary,
            Status = item.Status,
            DownloadCount = item.DownloadCount,
            IsFeatured = item.IsFeatured,
            PrimaryCategoryId = item.PrimaryCategoryId,
            ClassId = item.ClassId,
            Authors = item.Authors,
            Logo = item.Logo,
            LatestFiles = item.LatestFiles,
            LatestFilesIndexes = item.LatestFilesIndexes,
            DateCreated = item.DateCreated,
            DateModified = item.DateModified,
            DateReleased = item.DateReleased,
            AllowModDistribution = item.AllowModDistribution,
            GamePopularityRank = item.GamePopularityRank,
            IsAvailable = item.IsAvailable,
            ThumbsUpCount = item.ThumbsUpCount,
            Rating = item.Rating
        };
        entry.Type = item.ClassId switch
        {
            6 => ResourceType.Mod,
            12 => ResourceType.ResourcePack,
            17 => ResourceType.Map,
            6552 => ResourceType.ShaderPack,
            6945 => ResourceType.DataPack,
            4471 => ResourceType.ModPack,
            _ => ResourceType.Unknown
        };
        DependencyItems.Add(entry);
    }
}