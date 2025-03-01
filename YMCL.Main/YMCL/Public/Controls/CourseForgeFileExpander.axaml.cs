using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CurseForge.APIClient;
using CurseForge.APIClient.Models.Files;
using DynamicData;
using YMCL.Public.Enum;
using ModLoaderType = CurseForge.APIClient.Models.Mods.ModLoaderType;

namespace YMCL.Public.Controls;

public partial class CurseForgeFileExpander : UserControl
{
    private bool _firstLoad = true;
    public ObservableCollection<File> Files { get; } = [];

    public CurseForgeFileExpander(string version, int id, string name, ModLoaderType? loader, ResourceType type)
    {
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
    }

    private async Task Init(string version, int id, ModLoaderType? type)
    {
        ApiClient apiClient = new(Const.String.CurseForgeApiKey);
        var data = await apiClient.GetModFilesAsync(id, version, type ?? null);
        data.Data.ForEach(x => { Files.Add(x); });
        Ring.IsVisible = false;
    }
}