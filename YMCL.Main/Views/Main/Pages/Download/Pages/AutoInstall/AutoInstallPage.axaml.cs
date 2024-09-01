using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Threading;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Installer;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Download.Pages.AutoInstall;

public partial class AutoInstallPage : UserControl
{
    private bool _firstLoad = true;
    private bool latestRelease;
    private bool latestSnapshot;
    public ObservableCollection<VersionManifestEntry> versionManifestEntries = new();

    public AutoInstallPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            if (_firstLoad)
            {
                _firstLoad = false;
                LoadVanlliaVersion();
            }
        };
        BedrockRoot.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (Const.Data.Platform == Platform.Windows && Environment.OSVersion.Version.Major >= 10)
                {
                    var launcher = TopLevel.GetTopLevel(this).Launcher;
                    launcher.LaunchUriAsync(
                        new Uri($"https://minecraft.wiki/w/Java_Edition_{InstallPreviewIdText.Text}"));
                }
                else
                {
                    Method.Ui.Toast(MainLang.OnlySupportsWindows10AndAboveSystems, type: NotificationType.Error);
                }
            }
        };
        BeginInstallBtn.Click += (sender, e) =>
        {
            if (OptiFineListView.SelectedIndex >= 0 && ForgeListView.SelectedIndex >= 0)
            {
                var entry1 = ForgeListView.SelectedItem as ForgeInstallEntry;
                var entry2 = OptiFineListView.SelectedItem as OptiFineInstallEntity;
                _ = Method.Mc.InstallClientUsingMinecraftLaunchAsync(InstallPreviewIdText.Text!,
                    HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!),
                    optiFineInstallEntity: entry2!, forgeInstallEntry: entry1!);
            }
            else if (OptiFineListView.SelectedIndex >= 0)
            {
                var entry = OptiFineListView.SelectedItem as OptiFineInstallEntity;
                _ = Method.Mc.InstallClientUsingMinecraftLaunchAsync(InstallPreviewIdText.Text!,
                    HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!),
                    optiFineInstallEntity: entry!);
            }
            else if (ForgeListView.SelectedIndex >= 0)
            {
                var entry = ForgeListView.SelectedItem as ForgeInstallEntry;
                _ = Method.Mc.InstallClientUsingMinecraftLaunchAsync(InstallPreviewIdText.Text!,
                    HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!), entry!);
            }
            else if (FabricListView.SelectedIndex >= 0)
            {
                var entry = FabricListView.SelectedItem as FabricBuildEntry;
                _ = Method.Mc.InstallClientUsingMinecraftLaunchAsync(InstallPreviewIdText.Text!,
                    HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!),
                    fabricBuildEntry: entry!);
            }
            else if (QuiltListView.SelectedIndex >= 0)
            {
                var entry = QuiltListView.SelectedItem as QuiltBuildEntry;
                _ = Method.Mc.InstallClientUsingMinecraftLaunchAsync(InstallPreviewIdText.Text!,
                    HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!), quiltBuildEntry: entry!);
            }
            else
            {
                _ = Method.Mc.InstallClientUsingMinecraftLaunchAsync(InstallPreviewIdText.Text!,
                    HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!));
            }
        };
        InstallPreviewIdTextBox.TextChanged += (_, _) =>
        {
            HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!);
        };
        AllVersionSearchBox.TextChanged += (_, _) =>
        {
            var filteredItems = versionManifestEntries.Where(item =>
                item.Id.Contains(AllVersionSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
            AllVersionListView.Items.Clear();
            filteredItems.ForEach(version => { AllVersionListView.Items.Add(version); });
        };
        AllVersionListView.SelectionChanged += GoToListInstallPreview;
        ReleaseVersionListView.SelectionChanged += GoToListInstallPreview;
        PreviewVersionListView.SelectionChanged += GoToListInstallPreview;
        OldVersionListView.SelectionChanged += GoToListInstallPreview;
        ViewUpdatedContentBtn.Click += (_, _) =>
        {
            //$"https://minecraft.wiki/w/Java_Edition_{id}"
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchUriAsync(new Uri($"https://minecraft.wiki/w/Java_Edition_{InstallPreviewIdText.Text}"));
        };
        LatestReleaseVersionRoot.PointerPressed += GoToLatestInstallPreview;
        LatestPreviewVersionRoot.PointerPressed += GoToLatestInstallPreview;
        RetuenToListRoot.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                InstallableVersionListRoot.IsVisible = true;
                InstallPreviewRoot.IsVisible = false;
            }
        };
        ReloadLoadnstallableVersionListBtn.Click += (s, e) =>
        {
            LoadInstallableVersionListErrorInfoBar.IsVisible = false;
            LoadInstallableVersionListErrorInfoBar.Margin = new Thickness(0);
            LoadVanlliaVersion();
        };
        OptiFineListView.SelectionChanged += HandleSelectedLoaderName;
        FabricListView.SelectionChanged += HandleSelectedLoaderName;
        ForgeListView.SelectionChanged += HandleSelectedLoaderName;
        QuiltListView.SelectionChanged += HandleSelectedLoaderName;
    }

    private void ControlProperty()
    {
    }

    private async void LoadVanlliaVersion()
    {
        try
        {
            await Task.Run(async () =>
            {
                var vanlliaList = await VanlliaInstaller.EnumerableGameCoreAsync(MirrorDownloadManager.Bmcl);
                var manifestEntries = vanlliaList.ToList();
                Const.Window.main.searchPage.versionManifestEntries = manifestEntries.ToList();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    latestRelease = false;
                    latestSnapshot = false;
                    foreach (var item in manifestEntries)
                    {
                        AllVersionListView.Items.Add(item);
                        versionManifestEntries.Add(item);
                        if (item.Type == "release")
                        {
                            ReleaseVersionListView.Items.Add(item);
                            if (!latestRelease)
                            {
                                latestRelease = true;
                                LatestReleaseVersionId.Text = item.Id;
                                LatestReleaseVersionTime.Text = item.ReleaseTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
                            }

                            Const.Window.main.searchPage.aggregateSearchList.Add(new AggregateSearch()
                            {
                                Tag = "jump", Text = $"{MainLang.ReleaseVersion} - {item.Id}", Type = MainLang.InstallVersion, Target = "auto-install",
                                Summary = $"{MainLang.JumpToSearchTip.Replace("{target}",$"{MainLang.Download}-{MainLang.AutoInstall}")}", Order = 70, InstallVersionId = item.Id
                            });
                        }
                        else if (item.Type == "snapshot")
                        {
                            PreviewVersionListView.Items.Add(item);
                            if (!latestSnapshot)
                            {
                                latestSnapshot = true;
                                LatestPreviewVersionId.Text = item.Id;
                                LatestPreviewVersionTime.Text = item.ReleaseTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
                            }

                            Const.Window.main.searchPage.aggregateSearchList.Add(new AggregateSearch()
                            {
                                Tag = "jump", Text = $"{MainLang.PreviewVersion} - {item.Id}", Type =MainLang.InstallVersion, Target = "auto-install",
                                Summary =  $"{MainLang.JumpToSearchTip.Replace("{target}",$"{MainLang.Download}-{MainLang.AutoInstall}")}", Order = 70, InstallVersionId = item.Id
                            });
                        }
                        else
                        {
                            OldVersionListView.Items.Add(item);
                            Const.Window.main.searchPage.aggregateSearchList.Add(new AggregateSearch()
                            {
                                Tag = "jump", Text = $"{MainLang.OldVersion} - {item.Id}", Type = MainLang.InstallVersion, Target = "auto-install",
                                Summary =  $"{MainLang.JumpToSearchTip.Replace("{target}",$"{MainLang.Download}-{MainLang.AutoInstall}")}", Order = 70, InstallVersionId = item.Id
                            });
                        }
                    }

                    Const.Window.main.searchPage.aggregateSearchList = Const.Window.main.searchPage.aggregateSearchList
                        .OrderBy(x => x.Order).ToList();
                    Const.Window.main.searchPage.UpdateAggregateSearch();

                    ReleaseVersionLoadingRing.IsVisible = false;
                    PreviewVersionLoadingRing.IsVisible = false;
                    OldVersionLoadingRing.IsVisible = false;
                    AllVersionLoadingRing.IsVisible = false;
                });
            });
        }
        catch (Exception ex)
        {
            Method.Ui.ShowShortException(MainLang.GetInstallableVersionFail, ex);
            LoadInstallableVersionListErrorInfoBar.IsVisible = true;
            LoadInstallableVersionListErrorInfoBar.Margin = new Thickness(0, 10, 0, 0);
        }
    }

    public void ExternalCall(string id)
    {
        InstallPreviewIdText.Text = id;
        MinecraftPreviewVerionId.Text = id;
        InstallPreviewIdTextBox.Text = id;
        InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
        InstallableVersionListRoot.IsVisible = false;
        InstallPreviewRoot.IsVisible = true;
        LoadLoaderList();
        CustomIdWarning.IsVisible = false;
    }
    
    private void LoadLoaderList()
    {
        var id = InstallPreviewIdText.Text;

        NoOptifine.IsVisible = false;
        NoForge.IsVisible = false;
        NoFabric.IsVisible = false;
        NoQuilt.IsVisible = false;
        OptiFineLoading.IsVisible = true;
        ForgeLoading.IsVisible = true;
        FabricLoading.IsVisible = true;
        QuiltLoading.IsVisible = true;

        OptiFineListView.Items.Clear();
        FabricListView.Items.Clear();
        ForgeListView.Items.Clear();
        QuiltListView.Items.Clear();

        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await OptifineInstaller.EnumerableFromVersionAsync(id)).ToList();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    OptiFineListView.Items.Clear();
                    list.ForEach(item => { OptiFineListView.Items.Add(item); });
                    if (list.Count == 0) NoOptifine.IsVisible = true;
                    OptiFineLoading.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Method.Ui.ShowShortException($"{MainLang.GetFail}: OptiFine", ex);
                    OptiFineLoading.IsVisible = false;
                    NoOptifine.IsVisible = true;
                });
            }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await ForgeInstaller.EnumerableFromVersionAsync(id)).ToList();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ForgeListView.Items.Clear();
                    list.ForEach(item => { ForgeListView.Items.Add(item); });
                    if (list.Count == 0) NoForge.IsVisible = true;
                    ForgeLoading.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Method.Ui.ShowShortException($"{MainLang.GetFail}: Forge", ex);
                    ForgeLoading.IsVisible = false;
                    NoForge.IsVisible = true;
                });
            }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await QuiltInstaller.EnumerableFromVersionAsync(id)).ToList();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    QuiltListView.Items.Clear();
                    list.ForEach(item => { QuiltListView.Items.Add(item); });
                    if (list.Count == 0) NoQuilt.IsVisible = true;
                    QuiltLoading.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Method.Ui.ShowShortException($"{MainLang.GetFail}: Quilt", ex);
                    QuiltLoading.IsVisible = false;
                    NoQuilt.IsVisible = true;
                });
            }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                var list = (await FabricInstaller.EnumerableFromVersionAsync(id)).ToList();
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    FabricListView.Items.Clear();
                    list.ForEach(item => { FabricListView.Items.Add(item); });
                    if (list.Count == 0) NoFabric.IsVisible = true;
                    FabricLoading.IsVisible = false;
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Method.Ui.ShowShortException($"{MainLang.GetFail}: Fabric", ex);
                    FabricLoading.IsVisible = false;
                    NoFabric.IsVisible = true;
                });
            }
        });
        InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
    }

    private void GoToListInstallPreview(object? sender, SelectionChangedEventArgs e)
    {
        if (((ListBox)sender).SelectedIndex == -1) return;
        var entry = ((ListBox)sender).SelectedItem as VersionManifestEntry;
        ((ListBox)sender).SelectedIndex = -1;
        InstallPreviewIdText.Text = entry.Id;
        MinecraftPreviewVerionId.Text = entry.Id;
        InstallPreviewIdTextBox.Text = entry.Id;
        InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
        InstallableVersionListRoot.IsVisible = false;
        InstallPreviewRoot.IsVisible = true;
        LoadLoaderList();
        CustomIdWarning.IsVisible = false;
    }

    private void HandleSelectedLoaderName(object? sender, SelectionChangedEventArgs e)
    {
        var a = "Null";
        switch (((ListBox)sender).Tag)
        {
            case "optifine":
                if (((ListBox)sender).SelectedIndex != -1)
                {
                    a =
                        $"{(((ListBox)sender).SelectedItem as OptiFineInstallEntity).Type} {(((ListBox)sender).SelectedItem as OptiFineInstallEntity).Patch}";
                    QuiltListView.SelectedItem = -1;
                    FabricListView.SelectedItem = -1;
                }

                SelectedOptiFine.Text = a;
                break;
            case "forge":
                if (((ListBox)sender).SelectedIndex != -1)
                {
                    a = $"{(((ListBox)sender).SelectedItem as ForgeInstallEntry).ForgeVersion}";
                    QuiltListView.SelectedItem = -1;
                    FabricListView.SelectedItem = -1;
                }

                SelectedForge.Text = a;
                break;
            case "quilt":
                if (((ListBox)sender).SelectedIndex != -1)
                {
                    a = $"{(((ListBox)sender).SelectedItem as QuiltBuildEntry).BuildVersion}";
                    ForgeListView.SelectedItem = -1;
                    OptiFineListView.SelectedItem = -1;
                    FabricListView.SelectedItem = -1;
                }

                SelectedQuilt.Text = a;
                break;
            case "fabric":
                if (((ListBox)sender).SelectedIndex != -1)
                {
                    a = $"{(((ListBox)sender).SelectedItem as FabricBuildEntry).BuildVersion}";
                    ForgeListView.SelectedItem = -1;
                    OptiFineListView.SelectedItem = -1;
                    QuiltListView.SelectedItem = -1;
                }

                SelectedFabric.Text = a;
                break;
        }

        HandleAdditionalInstallTips();
    }

    private void GoToLatestInstallPreview(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var name = ((Control)sender).Name;
            if (name != null)
            {
                if (name == "LatestReleaseVersionRoot" && LatestReleaseVersionId.Text != MainLang.Loading)
                {
                    InstallPreviewIdText.Text = LatestReleaseVersionId.Text;
                    MinecraftPreviewVerionId.Text = LatestReleaseVersionId.Text;
                    InstallPreviewIdTextBox.Text = LatestReleaseVersionId.Text;
                    InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
                    InstallableVersionListRoot.IsVisible = false;
                    InstallPreviewRoot.IsVisible = true;
                    LoadLoaderList();
                    CustomIdWarning.IsVisible = false;
                }
                else if (name == "LatestPreviewVersionRoot" && LatestPreviewVersionId.Text != MainLang.Loading)
                {
                    InstallPreviewIdText.Text = LatestPreviewVersionId.Text;
                    MinecraftPreviewVerionId.Text = LatestPreviewVersionId.Text;
                    InstallPreviewIdTextBox.Text = LatestPreviewVersionId.Text;
                    InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
                    InstallableVersionListRoot.IsVisible = false;
                    InstallPreviewRoot.IsVisible = true;
                    LoadLoaderList();
                    CustomIdWarning.IsVisible = false;
                }
            }
        }
    }

    private void HandleAdditionalInstallTips()
    {
        InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
        var tip = string.Empty;
        if (ForgeListView.SelectedIndex != -1) tip += $"-Forge_{SelectedForge.Text}  ";
        if (OptiFineListView.SelectedIndex != -1) tip += $"-OptiFine_{SelectedOptiFine.Text}  ";
        if (FabricListView.SelectedIndex != -1) tip += $"-Fabric_{SelectedFabric.Text}  ";
        if (QuiltListView.SelectedIndex != -1) tip += $"-Quilt_{SelectedQuilt.Text}  ";
        InstallPreviewAdditionalInstallText.Text = tip;
        HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!);
    }

    public string HandleCustomId(string versionId, string customId)
    {
        if ((ForgeListView.SelectedIndex >= 0 || FabricListView.SelectedIndex >= 0 ||
             QuiltListView.SelectedIndex >= 0 || OptiFineListView.SelectedIndex >= 0) && customId == versionId)
        {
            var value = versionId;
            CustomIdWarning.IsVisible = true;
            if (ForgeListView.SelectedIndex >= 0 && OptiFineListView.SelectedIndex >= 0)
            {
                var entry1 = ForgeListView.SelectedItem as ForgeInstallEntry;
                var entry2 = OptiFineListView.SelectedItem as OptiFineInstallEntity;
                value = $"{versionId}-Forge_{entry1.ForgeVersion}-Optifine_{entry2.Type}_{entry2.Patch}";
            }
            else if (ForgeListView.SelectedIndex >= 0)
            {
                var entry = ForgeListView.SelectedItem as ForgeInstallEntry;
                value = $"{versionId}-Forge_{entry.ForgeVersion}";
            }
            else if (OptiFineListView.SelectedIndex >= 0)
            {
                var entry = OptiFineListView.SelectedItem as OptiFineInstallEntity;
                value = $"{versionId}-Optifine_{entry.Type}_{entry.Patch}";
            }
            else if (FabricListView.SelectedIndex >= 0)
            {
                var entry = FabricListView.SelectedItem as FabricBuildEntry;
                value = $"{versionId}-Fabric_{entry.BuildVersion}";
            }
            else if (QuiltListView.SelectedIndex >= 0)
            {
                var entry = QuiltListView.SelectedItem as QuiltBuildEntry;
                value = $"{versionId}-Quilt_{entry.BuildVersion}";
            }

            CustomIdWarningText.Text = value;
            return value;
        }

        CustomIdWarning.IsVisible = false;
        InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
        return customId;
    }
}