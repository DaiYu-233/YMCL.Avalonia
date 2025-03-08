﻿using System.Linq;
using MinecraftLaunch.Base.Models.Network;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Langs;
using YMCL.Public.Module.App.Init.SubModule.GetDataFromNetwork;

namespace YMCL.Views.Main.Pages.DownloadPages.AutoInstallPages;

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
            YMCL.App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview(UiProperty.Instance.LatestSnapshotGame);
        };
        LatestReleaseVersionRoot.PointerPressed += (_, _) =>
        {
            if (UiProperty.Instance.LatestReleaseGame.Type == null) return;
            YMCL.App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview(UiProperty.Instance.LatestReleaseGame);
        };
        UiProperty.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(UiProperty.Instance.InstallableGameSearchFilter))
            {
                Filter();
            }
        };
        ReloadInstallableGameListBtn.Click += (_, _) =>
        {
            LoadInstallableVersionListErrorInfoBar.IsVisible = false;
            _ = InstallableGame.Load();
            UiProperty.Instance.LatestReleaseGame = new VersionManifestEntry
                { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.Loading, Type = null };
            UiProperty.Instance.LatestSnapshotGame = new VersionManifestEntry
                { ReleaseTime = new DateTime(1970, 1, 1, 0, 0, 0), Id = MainLang.Loading, Type = null };
        };
    }

    private void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count <= 0) return;
        (sender as ListBox).SelectedItem = null;
        YMCL.App.UiRoot.ViewModel.Download._autoInstall.JumpToInstallPreview((e.AddedItems[0] as VersionManifestEntry)!);
    }

    public void Filter()
    {
        UiProperty.FilteredAllInstallableGames.Clear();
        UiProperty.AllInstallableGames.Where(item =>
                item.Id.Contains(UiProperty.Instance.InstallableGameSearchFilter, StringComparison.OrdinalIgnoreCase))
            .ToList().ForEach(item => UiProperty.FilteredAllInstallableGames.Add(item));
    }
}