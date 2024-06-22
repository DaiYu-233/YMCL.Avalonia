using Avalonia.Controls;
using Avalonia.Threading;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Installer;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Download.Pages.AutoInstall
{
    public partial class AutoInstallPage : UserControl
    {
        public ObservableCollection<VersionManifestEntry> versionManifestEntriesObservableCollection = new();
        bool _firstLoad = true;
        bool _loadedVanllia = false;
        bool latestRelease = false;
        bool latestSnapshot = false;
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
                Method.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
                if (_firstLoad)
                {
                    _firstLoad = false;
                    LoadVanlliaVersion();
                }
            };
            AllVersionSearchBox.TextChanged += (_, _) =>
            {
                var filteredItems = versionManifestEntriesObservableCollection.Where(item => item.Id.Contains(AllVersionSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
                AllVersionListView.Items.Clear();
                filteredItems.ForEach(version =>
                {
                    AllVersionListView.Items.Add(version);
                });
            };
            void GoToListInstallPreview(object? sender, SelectionChangedEventArgs e)
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
            }
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
            void GoToLatestInstallPreview(object? sender, Avalonia.Input.PointerPressedEventArgs e)
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
                        }
                    }
                }
            }
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
                LoadnstallableVersionListErrorInfoBar.IsOpen = false;
                LoadnstallableVersionListErrorInfoBar.Margin = new Avalonia.Thickness(0);
                LoadVanlliaVersion();
            };
            void HandleSelectedLoaderName(object? sender, SelectionChangedEventArgs e)
            {
                var a = "Null";
                switch (((ListBox)sender).Tag)
                {
                    case "optifine":
                        if (((ListBox)sender).SelectedIndex != -1)
                        {
                            a = $"{(((ListBox)sender).SelectedItem as OptiFineInstallEntity).Type} {(((ListBox)sender).SelectedItem as OptiFineInstallEntity).Patch}";
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
            OptiFineListView.SelectionChanged += HandleSelectedLoaderName;
            FabricListView.SelectionChanged += HandleSelectedLoaderName;
            ForgeListView.SelectionChanged += HandleSelectedLoaderName;
            QuiltListView.SelectionChanged += HandleSelectedLoaderName;
            void HandleAdditionalInstallTips()
            {
                InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
                var tip = string.Empty;
                if (ForgeListView.SelectedIndex != -1)
                {
                    tip += $"-Forge_{SelectedForge.Text}  ";
                }
                if (OptiFineListView.SelectedIndex != -1)
                {
                    tip += $"-OptiFine_{SelectedOptiFine.Text}  ";
                }
                if (FabricListView.SelectedIndex != -1)
                {
                    tip += $"-Fabric_{SelectedFabric.Text}  ";
                }
                if (QuiltListView.SelectedIndex != -1)
                {
                    tip += $"-Quilt_{SelectedQuilt.Text}  ";
                }
                InstallPreviewAdditionalInstallText.Text = tip;
            }
        }
        private void ControlProperty()
        {

        }
        async void LoadVanlliaVersion()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var vanlliaList = await VanlliaInstaller.EnumerableGameCoreAsync(MirrorDownloadManager.Bmcl);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        latestRelease = false;
                        _loadedVanllia = true;
                        latestSnapshot = false;
                        foreach (var item in vanlliaList)
                        {
                            AllVersionListView.Items.Add(item);
                            versionManifestEntriesObservableCollection.Add(item);
                            if (item.Type == "release")
                            {
                                ReleaseVersionListView.Items.Add(item);
                                if (!latestRelease)
                                {
                                    latestRelease = true;
                                    LatestReleaseVersionId.Text = item.Id;
                                    LatestReleaseVersionTime.Text = item.ReleaseTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
                                }
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
                            }
                            else
                            {
                                OldVersionListView.Items.Add(item);
                            }
                        }
                        ReleaseVersionLoadingRing.IsVisible = false;
                        PreviewVersionLoadingRing.IsVisible = false;
                        OldVersionLoadingRing.IsVisible = false;
                        AllVersionLoadingRing.IsVisible = false;
                    });
                });
            }
            catch (Exception ex)
            {
                Method.ShowShortException(MainLang.GetInstallableVersionFail, ex);
                LoadnstallableVersionListErrorInfoBar.IsOpen = true;
                LoadnstallableVersionListErrorInfoBar.Margin = new Avalonia.Thickness(0, 10, 0, 0);
            }
        }
        async void LoadLoaderList()
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

            _ = Task.Run(async () =>
            {
                try
                {
                    var list = (await OptifineInstaller.EnumerableFromVersionAsync(id)).ToList();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        OptiFineListView.Items.Clear(); list.ForEach(item =>
                        {
                            OptiFineListView.Items.Add(item);
                        });
                        if (list.Count == 0)
                        {
                            NoOptifine.IsVisible = true;
                        }
                        OptiFineLoading.IsVisible = false;
                    });
                }
                catch (Exception ex)
                {
                    Method.ShowShortException($"{MainLang.GetFail}£ºOptiFine", ex);
                }
            });
            _ = Task.Run(async () =>
            {
                try
                {
                    var list = (await ForgeInstaller.EnumerableFromVersionAsync(id)).ToList();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ForgeListView.Items.Clear(); list.ForEach(item =>
                        {
                            ForgeListView.Items.Add(item);
                        });
                        if (list.Count == 0)
                        {
                            NoForge.IsVisible = true;
                        }
                        ForgeLoading.IsVisible = false;
                    });
                }
                catch (Exception ex)
                {
                    Method.ShowShortException($"{MainLang.GetFail}£ºForge", ex);
                }
            });
            _ = Task.Run(async () =>
            {
                try
                {
                    var list = (await QuiltInstaller.EnumerableFromVersionAsync(id)).ToList();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        QuiltListView.Items.Clear(); list.ForEach(item =>
                        {
                            QuiltListView.Items.Add(item);
                        });
                        if (list.Count == 0)
                        {
                            NoQuilt.IsVisible = true;
                        }
                        QuiltLoading.IsVisible = false;
                    });
                }
                catch (Exception ex)
                {
                    Method.ShowShortException($"{MainLang.GetFail}£ºQuilt", ex);
                }
            });
            _ = Task.Run(async () =>
            {
                try
                {
                    var list = (await FabricInstaller.EnumerableFromVersionAsync(id)).ToList();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        FabricListView.Items.Clear(); list.ForEach(item =>
                        {
                            FabricListView.Items.Add(item);
                        });
                        if (list.Count == 0)
                        {
                            NoFabric.IsVisible = true;
                        }
                        FabricLoading.IsVisible = false;
                    });
                }
                catch (Exception ex)
                {
                    Method.ShowShortException($"{MainLang.GetFail}£ºFabric", ex);
                }
            });
        }

    }
}
