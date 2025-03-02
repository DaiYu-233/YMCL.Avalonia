using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;
using Modrinth;
using Modrinth.Models;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Views.Main.Pages.DownloadPages.ModrinthPages;

namespace YMCL.Public.Controls;

public partial class ModrinthFileExpander : UserControl
{
    private readonly string _version;
    public ObservableCollection<ModrinthResourceEntry> DependencyItems { get; } = [];
    public ObservableCollection<string> DependencyIds { get; } = [];
    public ObservableCollection<ModrinthFile> Files { get; } = [];
    private bool _firstLoad = true;

    public ModrinthFileExpander(string name, string version, ResourceType type, ModrinthResourceEntry entry)
    {
        _version = version;
        InitializeComponent();
        Expander.Header = name;
        DataContext = this;
        ListView.SelectionChanged += async (_, _) =>
        {
            if (ListView.SelectedItem == null) return;
            Public.Module.Op.DownloadResource.SaveModrinth(type, ListView.SelectedItem as ModrinthFile);
            await Task.Delay(300);
            ListView.SelectedItem = null;
        };
        ListBox.SelectionChanged += (_, _) =>
        {
            if (ListBox.SelectedItem == null) return;
            var item = ListBox.SelectedItem as ModrinthResourceEntry;
            App.UiRoot.ViewModel.Download._modrinth.CreateNewPage(new SearchTabViewItemEntry()
            {
                CanClose = true, Host = nameof(Views.Main.Pages.DownloadPages.Modrinth),
                Content = new ModFileResult(item.ProjectId, (item.Name, item.IconUrl, item.Summary)!),
                Title = $"{item.DisplayType}: {item.Name}"
            });
            ListBox.SelectedItem = null;
        };
        Loaded += (_, _) =>
        {
            if (!_firstLoad) return;
            _firstLoad = false;
            _ = Init(entry);
        };
    }


    private async Task Init(ModrinthResourceEntry entry)
    {
        Dependencies.IsVisible = true;
        var client = new ModrinthClient();
        var deps = await client.Project.GetDependenciesAsync(entry.ProjectId);
        var tasks = new List<Task>();
        deps.Projects.ToList().ForEach(x =>
        {
            tasks.Add(GetDepVersion(x));
        });
        await Task.WhenAll(tasks);
        if (DependencyItems.Count != 0)
        {
            ListBox.IsVisible = true;
            DependenciesRing.IsVisible = false;
        }
        else
        {
            Dependencies.IsVisible = false;
        }
    }

    private async Task GetDepVersion(Project x)
    {
        var versions = await Module.IO.Network.Modrinth.GetVersionsById(x.Id);
        var list = new List<string>();
        versions.data.ForEach(y => list.AddRange(y.game_versions));
        if (!list.Contains(_version)) return;
        var item = new ModrinthResourceEntry()
        {
            Slug = x.Slug,
            ProjectId = x.Id,
            Categories = x.Categories,
            ClientSide = x.ClientSide,
            ServerSide = x.ServerSide,
            Description = x.Description,
            ProjectType = x.ProjectType,
            Title = x.Title,
            DateCreated = x.Published,
            DateModified = x.Updated,
            Followers = x.Followers,
            Downloads = x.Downloads,
            IconUrl = x.IconUrl,
            License = x.License.ToString(),
            Color = x.Color,
            FeaturedGallery = x.FeaturedGallery,
        };
        DependencyItems.Add(item);
    }

    public ModrinthFileExpander()
    {
    }
}