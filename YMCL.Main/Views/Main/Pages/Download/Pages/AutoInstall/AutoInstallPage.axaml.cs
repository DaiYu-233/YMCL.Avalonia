using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Resolver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Download.Pages.AutoInstall
{
    public partial class AutoInstallPage : UserControl
    {
        public ObservableCollection<VersionManifestEntry> versionManifestEntries = new();
        bool _firstLoad = true;
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
                Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
                if (_firstLoad)
                {
                    _firstLoad = false;
                    LoadVanlliaVersion();
                }
            };
            BeginInstallBtn.Click += (sender, e) =>
            {
                if (OptiFineListView.SelectedIndex >= 0 && ForgeListView.SelectedIndex >= 0)
                {
                    var entry1 = ForgeListView.SelectedItem as ForgeInstallEntry;
                    var entry2 = OptiFineListView.SelectedItem as OptiFineInstallEntity;
                    Install(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!, optiFineInstallEntity: entry2!, forgeInstallEntry: entry1!);
                }
                else if (OptiFineListView.SelectedIndex >= 0)
                {
                    var entry = OptiFineListView.SelectedItem as OptiFineInstallEntity;
                    Install(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!, optiFineInstallEntity: entry!);
                }
                else if (ForgeListView.SelectedIndex >= 0)
                {
                    var entry = ForgeListView.SelectedItem as ForgeInstallEntry;
                    Install(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!, forgeInstallEntry: entry!);
                }
                else if (FabricListView.SelectedIndex >= 0)
                {
                    var entry = FabricListView.SelectedItem as FabricBuildEntry;
                    Install(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!, fabricBuildEntry: entry!);
                }
                else if (QuiltListView.SelectedIndex >= 0)
                {
                    var entry = QuiltListView.SelectedItem as QuiltBuildEntry;
                    Install(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!, quiltBuildEntry: entry!);
                }
                else
                {
                    Install(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!);
                }
            };
            InstallPreviewIdTextBox.TextChanged += (_, _) =>
            {
                HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!);
            };
            AllVersionSearchBox.TextChanged += (_, _) =>
            {
                var filteredItems = versionManifestEntries.Where(item => item.Id.Contains(AllVersionSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
                AllVersionListView.Items.Clear();
                filteredItems.ForEach(version =>
                {
                    AllVersionListView.Items.Add(version);
                });
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
                LoadInstallableVersionListErrorInfoBar.IsOpen = false;
                LoadInstallableVersionListErrorInfoBar.Margin = new Avalonia.Thickness(0);
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
                        latestSnapshot = false;
                        foreach (var item in vanlliaList)
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
                Method.Ui.ShowShortException(MainLang.GetInstallableVersionFail, ex);
                LoadInstallableVersionListErrorInfoBar.IsOpen = true;
                LoadInstallableVersionListErrorInfoBar.Margin = new Avalonia.Thickness(0, 10, 0, 0);
            }
        }
        void LoadLoaderList()
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
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Method.Ui.ShowShortException($"{MainLang.GetFail}：OptiFine", ex);
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
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Method.Ui.ShowShortException($"{MainLang.GetFail}：Forge", ex);
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
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Method.Ui.ShowShortException($"{MainLang.GetFail}：Quilt", ex);
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
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Method.Ui.ShowShortException($"{MainLang.GetFail}：Fabric", ex);
                        FabricLoading.IsVisible = false;
                        NoFabric.IsVisible = true;
                    });
                }
            });
            InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
        }
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
            CustomIdWarning.IsVisible = false;
        }
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
            HandleCustomId(InstallPreviewIdText.Text!, InstallPreviewIdTextBox.Text!);
        }
        public string HandleCustomId(string versionId, string customId)
        {
            if ((ForgeListView.SelectedIndex >= 0 || FabricListView.SelectedIndex >= 0 || QuiltListView.SelectedIndex >= 0 || OptiFineListView.SelectedIndex >= 0) && customId == versionId)
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
            else
            {
                CustomIdWarning.IsVisible = false;
                InstallPreviewAdditionalInstallText.Text = MainLang.NoAdditionalInstall;
                return customId;
            }
        }
        public async void Install(string versionId, string versionName = null, ForgeInstallEntry forgeInstallEntry = null, FabricBuildEntry fabricBuildEntry = null, QuiltBuildEntry quiltBuildEntry = null, OptiFineInstallEntity optiFineInstallEntity = null)
        {
            var shouldReturn = false;
            var customId = versionName != null ? versionName : versionId;
            customId = HandleCustomId(versionId, customId);
            Regex regex = new Regex(@"[\\/:*?""<>|]");
            MatchCollection matches = regex.Matches(customId);
            if (matches.Count > 0)
            {
                var str = string.Empty;
                foreach (Match match in matches)
                {
                    str += match.Value;
                }

                return;
            }

            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            var resolver = new GameResolver(setting.MinecraftFolder);
            var vanlliaInstaller = new VanlliaInstaller(resolver, versionId, MirrorDownloadManager.Bmcl);
            if (Directory.Exists(Path.Combine(setting.MinecraftFolder, "versions", customId)))
            {
                Method.Ui.Toast($"{MainLang.FolderAlreadyExists}：{customId}", Const.Notification.main, NotificationType.Error);
                return;
            }

            InstallPreviewRoot.IsVisible = false;
            InstallableVersionListRoot.IsVisible = true;

            var task = new WindowTask($"{MainLang.Install}：Vanllia - {versionId}", true);
            task.UpdateTextProgress("-----> Vanllia");

            //Vanllia
            await Task.Run(async () =>
            {
                try
                {
                    vanlliaInstaller.ProgressChanged += (_, x) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTextProgress(x.ProgressStatus);
                            task.UpdateValueProgress(x.Progress * 100);
                        });
                    };

                    var result = await vanlliaInstaller.InstallAsync();

                    if (!result)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.Toast($"{MainLang.InstallFail}：Vanllia - {versionId}", Const.Notification.main, NotificationType.Error);
                        });
                        shouldReturn = true;
                    }
                    else
                    {
                        if (forgeInstallEntry == null && quiltBuildEntry == null && fabricBuildEntry == null)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.Toast($"{MainLang.InstallFinish}：Vanllia - {versionId}", Const.Notification.main, NotificationType.Success);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Method.Ui.ShowShortException($"{MainLang.InstallFail}：Vanllia - {versionId}", ex);
                    });
                    shouldReturn = true;
                }
            });
            if (shouldReturn) { return; }

            //Forge
            if (forgeInstallEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath));
                        if (javas.Count <= 0)
                        {
                            Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                            shouldReturn = true;
                        }
                        else
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var forgeInstaller = new ForgeInstaller(game, forgeInstallEntry, javas[0].JavaPath, customId, MirrorDownloadManager.Bmcl);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTitle($"{MainLang.Install}：Forge - {versionId}");
                                task.UpdateTextProgress("-----> Forge", false);
                            });
                            forgeInstaller.ProgressChanged += (_, x) =>
                            {
                                Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.UpdateTextProgress(x.ProgressStatus);
                                    task.UpdateValueProgress(x.Progress * 100);
                                });
                            };

                            var result = await forgeInstaller.InstallAsync();

                            if (result)
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFinish}：Forge - {versionId}", Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFail}：Forge - {customId}", Const.Notification.main, NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.ShowShortException($"{MainLang.InstallFail}：Forge - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) { return; }
            }

            //OptiFine
            if (optiFineInstallEntity != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath));
                        if (javas.Count <= 0)
                        {
                            Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                            shouldReturn = true;
                        }
                        else
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var optifineInstaller = new OptifineInstaller(game, optiFineInstallEntity, javas[0].JavaPath, customId, MirrorDownloadManager.Bmcl);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTitle($"{MainLang.Install}：OptiFine - {versionId}");
                                task.UpdateTextProgress("-----> OptiFine", false);
                            });
                            optifineInstaller.ProgressChanged += (_, x) =>
                            {
                                Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.UpdateTextProgress(x.ProgressStatus);
                                    task.UpdateValueProgress(x.Progress * 100);
                                });
                            };

                            var result = await optifineInstaller.InstallAsync();

                            if (result)
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFinish}：OptiFine - {versionId}", Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFail}：OptiFine - {customId}", Const.Notification.main, NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.ShowShortException($"{MainLang.InstallFail}：OptiFine - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) { return; }
            }

            //Fabric
            if (fabricBuildEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var game = resolver.GetGameEntity(versionId);
                        var fabricInstaller = new FabricInstaller(game, fabricBuildEntry, customId, MirrorDownloadManager.Bmcl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTitle($"{MainLang.Install}：Fabric - {versionId}");
                            task.UpdateTextProgress("-----> Fabric", false);
                        });
                        fabricInstaller.ProgressChanged += (_, x) =>
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(x.ProgressStatus);
                                task.UpdateValueProgress(x.Progress * 100);
                            });
                        };

                        var result = await fabricInstaller.InstallAsync();

                        if (result)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.Toast($"{MainLang.InstallFinish}：Fabric - {versionId}", Const.Notification.main, NotificationType.Success);
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.Toast($"{MainLang.InstallFail}：Fabric - {customId}", Const.Notification.main, NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.ShowShortException($"{MainLang.InstallFail}：Fabric - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) { return; }
            }

            //Quilt
            if (quiltBuildEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var game = resolver.GetGameEntity(versionId);
                        var quiltInstaller = new QuiltInstaller(game, quiltBuildEntry, customId, MirrorDownloadManager.Bmcl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTitle($"{MainLang.Install}：Quilt - {versionId}");
                            task.UpdateTextProgress("-----> Quilt", false);
                        });
                        quiltInstaller.ProgressChanged += (_, x) =>
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(x.ProgressStatus);
                                task.UpdateValueProgress(x.Progress * 100);
                            });
                        };

                        var result = await quiltInstaller.InstallAsync();

                        if (result)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.Toast($"{MainLang.InstallFinish}：Quilt - {versionId}", Const.Notification.main, NotificationType.Success);
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.Toast($"{MainLang.InstallFail}：Quilt - {customId}", Const.Notification.main, NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.ShowShortException($"{MainLang.InstallFail}：Quilt - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) { return; }
            }

            Const.Window.main.Activate();
            task.Finish();
            task.Hide();
        }
    }
}
