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

public partial class CourseForgeModFileExpander : UserControl
{
    public ObservableCollection<File> Files { get; } = [];

    public CourseForgeModFileExpander(string version, int id, string name, ModLoaderType? type)
    {
        InitializeComponent();
        Expander.Header = name;
        DataContext = this;
        _ = Init(version, id, type);
    }

    private async Task Init(string version, int id, ModLoaderType? type)
    {
        ApiClient apiClient = new(Const.String.CurseForgeApiKey);
        var data = await apiClient.GetModFilesAsync(id, version, type ?? null);
        Files.AddRange(data.Data);
        Ring.IsVisible = false;
    }
}