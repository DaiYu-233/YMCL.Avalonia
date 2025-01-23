using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MinecraftLaunch.Classes.Models.Install;
using YMCL.Public.Classes;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Pages.DownloadPages;

public partial class InstallableGames : UserControl
{
    public InstallableGames()
    {
        InitializeComponent();
        DataContext = UiProperty.Instance;
        AllVersionListView.SelectionChanged += SelectionChanged;
        ReleaseVersionListView.SelectionChanged += SelectionChanged;
        PreviewVersionListView.SelectionChanged += SelectionChanged;
        OldVersionListView.SelectionChanged += SelectionChanged;
        LatestPreviewVersionRoot.PointerPressed += (_, _) =>
        {
            if (UiProperty.Instance.LatestSnapshotGame.Type == null) return;
            App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview(UiProperty.Instance.LatestSnapshotGame.Id);
        };
        LatestReleaseVersionRoot.PointerPressed += (_, _) =>
        {
            if (UiProperty.Instance.LatestReleaseGame.Type == null) return;
            App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview(UiProperty.Instance.LatestReleaseGame.Id);
        };
    }

    private void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count <= 0) return;
        (sender as ListBox).SelectedItem = null;
        Console.WriteLine(e.AddedItems[0]);
        App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview((e.AddedItems[0] as VersionManifestEntry).Id);
    }
}