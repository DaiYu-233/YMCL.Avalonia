using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Input;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Skin;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Crash;
using AccountType = YMCL.Main.Public.AccountType;
using SearchOption = System.IO.SearchOption;

namespace YMCL.Main.Views.Main.Pages.Launch;

public partial class LaunchPage : UserControl
{
    private bool _firstLoad = true;
    private bool _firstOpenVersionList = true;
    private bool _firstOpenVersionSetting = true;
    public bool ShouldCloseVersionList;
    private bool _isSelectioningVersionFolder = false;
    public bool IsVersionSettingOpenByVersionList = false;

    private List<string> _minecraftFolders =
        JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath));

    private ObservableCollection<ModManageEntry> _modManageEntries;

    public LaunchPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void ControlProperty()
    {
    }

    private void BindingEvent()
    {
        Loaded += async (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            LoadAccounts();
            _minecraftFolders =
                JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath));
            MinecraftFolderComboBox.Items.Clear();
            var setting = Const.Data.Setting;
            foreach (var item in _minecraftFolders) MinecraftFolderComboBox.Items.Add(item);
            if (setting.MinecraftFolder == null || !_minecraftFolders.Contains(setting.MinecraftFolder))
                MinecraftFolderComboBox.SelectedIndex = 0;
            else
                MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
            if (_firstLoad)
            {
                Const.Window.main.settingPage.pluginSettingPage.LoadPlugin();
                _firstLoad = false;
            }

            LoadVersions();

            if (VersionListRoot.Margin == new Thickness(10))
            {
                LaunchConsoleRoot.IsVisible = false;
                LaunchConsoleRoot.Opacity = 0;
                await Task.Delay(260);
                LaunchConsoleRoot.IsVisible = true;
            }
            else
            {
                LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
                LaunchConsoleRoot.IsVisible = true;
            }
        };
        AccountComboBox.SelectionChanged += (s, e) =>
        {
            AccountComboBox.IsVisible = false;
            AccountComboBox.IsVisible = true;
            var setting = Const.Data.Setting;
            if (AccountComboBox.SelectedItem as AccountInfo != null)
                Head.Source = (AccountComboBox.SelectedItem as AccountInfo).Bitmap;
            if (AccountComboBox.SelectedIndex == setting.AccountSelectionIndex ||
                AccountComboBox.SelectedIndex == -1) return;
            setting.AccountSelectionIndex = AccountComboBox.SelectedIndex;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        MinecraftFolderComboBox.SelectionChanged += (s, e) =>
        {
            var setting = Const.Data.Setting;
            if (MinecraftFolderComboBox.SelectedItem == null ||
                MinecraftFolderComboBox.SelectedItem.ToString() == setting.MinecraftFolder) return;
            setting.MinecraftFolder = MinecraftFolderComboBox.SelectedItem.ToString();
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            _isSelectioningVersionFolder = true;
            ShouldCloseVersionList = false;
            LoadVersions();
            VersionListView.SelectedIndex = 0;
        };
        VersionListBtn.Click += async (s, e) =>
        {
            LaunchConsoleRoot.Opacity = 0;
            ShouldCloseVersionList = false;
            if (_firstOpenVersionList)
            {
                await Task.Delay(260);
                _firstOpenVersionList = false;
                await OpenVersionList();
            }
            else
            {
                LoadVersions();
                await Task.Delay(260);
                await OpenVersionList();
            }
        };
        VersionSettingBtn.Click += async (s, e) =>
        {
            if (VersionListView.SelectedItem != null)
            {
                var entry = (VersionListView.SelectedItem as GameListEntry).Entry;
                if (entry.Type == "BedRock")
                {
                    Method.Ui.Toast(MainLang.CannotOpenBedRockSetting);
                    return;
                }

                if (entry != null)
                {
                    LaunchConsoleRoot.Opacity = 0;
                    if (_firstOpenVersionSetting)
                    {
                        _firstOpenVersionSetting = false;
                        try
                        {
                            LoadVersionSettingUI(entry);
                            OpenVersionSetting();
                        }
                        catch
                        {
                            CloseVersionSetting(null, null);
                            Method.Ui.Toast(MainLang.CannotLoadVersionSetting, Const.Notification.main,
                                NotificationType.Error);
                        }
                    }
                    else
                    {
                        try
                        {
                            LoadVersionSettingUI(entry);
                            OpenVersionSetting();
                        }
                        catch
                        {
                            CloseVersionSetting(null, null);
                            Method.Ui.Toast(MainLang.CannotLoadVersionSetting, Const.Notification.main,
                                NotificationType.Error);
                        }
                    }
                }
                else
                {
                    Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main,
                        NotificationType.Error);
                }
            }
            else
            {
                Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
            }
        };
        CloseVersionListBtn.Click += (s, e) => { CloseVersionList(); };
        VersionListView.PointerEntered += async (s, e) =>
        {
            if (_isSelectioningVersionFolder)
            {
                await Task.Delay(200);
                _isSelectioningVersionFolder = false;
            }

            ShouldCloseVersionList = true;
        };
        VersionListView.SelectionChanged += async (s, e) =>
        {
            if (VersionListView.SelectedItem != null)
            {
                var setting = Const.Data.Setting;
                GameCoreText.Text = (VersionListView.SelectedItem as GameListEntry).Entry.Id;
                if (VersionListView.SelectedIndex == 0)
                    setting.Version = "BedRock";
                else
                    setting.Version = (VersionListView.SelectedItem as GameListEntry).Entry.Id;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            await Task.Delay(100);
            if (ShouldCloseVersionList)
                CloseVersionList();
        };
        LaunchBtn.Click += (s, e) =>
        {
            if (VersionListView.SelectedItem != null)
            {
                if (VersionListView.SelectedIndex == 0)
                {
                    var launcher = TopLevel.GetTopLevel(this).Launcher;
                    launcher.LaunchUriAsync(new Uri("minecraft://play"));
                    Method.Ui.Toast(MainLang.LaunchFinish, type: NotificationType.Success);
                }
                else
                {
                    if (Const.Data.Setting.LaunchCore == LaunchCore.MinecraftLaunch)
                    {
                        _ = Method.Mc.LaunchClientUsingMinecraftLaunchAsync();
                    }
                    else
                    {
                        _ = Method.Mc.LaunchClientUsingStarLightAsync();
                    }
                }
            }
        };
        OpenSelectedMinecraftFolderBtn.Click += (s, e) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(MinecraftFolderComboBox.SelectedItem.ToString()!));
        };
        RefreshVersionListBtn.Click += (s, e) =>
        {
            ShouldCloseVersionList = false;
            LoadVersions();
        };
        VersionTabViewItemOverview.CloseRequested += CloseVersionSetting;
        VersionTabViewItemMod.CloseRequested += CloseVersionSetting;
        VersionTabViewItemSetting.CloseRequested += CloseVersionSetting;
        EnableIndependencyCoreComboBox.SelectionChanged += (_, _) =>
        {
            var entry = (VersionListView.SelectedItem as GameListEntry).Entry;
            var setting = Method.Mc.GetVersionSetting(entry!);
            setting.EnableIndependencyCore =
                (VersionSettingEnableIndependencyCore)EnableIndependencyCoreComboBox.SelectedIndex;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.String.VersionSettingFileName),
                JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        AutoJoinServerTextBox.TextChanged += (_, _) =>
        {
            var entry = (VersionListView.SelectedItem as GameListEntry).Entry;
            var setting = Method.Mc.GetVersionSetting(entry!);
            setting.AutoJoinServerIp = AutoJoinServerTextBox.Text;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.String.VersionSettingFileName),
                JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        JavaComboBox.SelectionChanged += (_, _) =>
        {
            JavaComboBox.IsVisible = false;
            JavaComboBox.IsVisible = true;
            var version = (VersionListView.SelectedItem as GameListEntry).Entry;
            var filePath = Path.Combine(Path.GetDirectoryName(version.JarPath)!, Const.String.VersionSettingFileName);
            var versionSetting = Method.Mc.GetVersionSetting(version);
            if (JavaComboBox.SelectedItem == null ||
                JavaComboBox.SelectedItem.ToString() == versionSetting.Java.JavaPath) return;
            if (JavaComboBox.SelectedIndex == 0 && versionSetting.Java.JavaPath == "Global") return;
            if (JavaComboBox.SelectedIndex == 1 && versionSetting.Java.JavaPath == "Auto") return;
            if (JavaComboBox.SelectedIndex == 0)
                versionSetting.Java = new JavaEntry
                {
                    JavaPath = "Global"
                };
            else if (JavaComboBox.SelectedIndex == 1)
                versionSetting.Java = new JavaEntry
                {
                    JavaPath = "Auto"
                };
            else
                versionSetting.Java = JavaComboBox.SelectedItem as JavaEntry;
            File.WriteAllText(filePath, JsonConvert.SerializeObject(versionSetting, Formatting.Indented));
        };
        ModSearchBox.TextChanged += (_, _) =>
        {
            var filteredItems = _modManageEntries
                .Where(item => item.Name.Contains(ModSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
            ModManageList.Items.Clear();
            filteredItems.ForEach(version => { ModManageList.Items.Add(version); });
        };
        RefreshModBtn.Click += (_, _) => { LoadVersionMods((VersionListView.SelectedItem as GameListEntry).Entry!); };
        ModManageList.SelectionChanged += (_, _) =>
        {
            SelectedModCount.Text = $"{MainLang.SelectedItem}{ModManageList.SelectedItems.Count}";
        };
        DeselectAllModBtn.Click += (_, _) => { ModManageList.SelectedIndex = -1; };
        SelectAllModBtn.Click += (_, _) => { ModManageList.SelectAll(); };
        DisableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as ModManageEntry;
                if (mod.Name.Length > 0)
                    if (Path.GetExtension(mod.File) == ".jar")
                        File.Move(mod.File, mod.File + ".disabled");
            }

            LoadVersionMods((VersionListView.SelectedItem as GameListEntry).Entry!);
        };
        EnableSelectModBtn.Click += (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            foreach (var item in mods)
            {
                var mod = item as ModManageEntry;
                if (mod.Name.Length > 0)
                    if (Path.GetExtension(mod.File) == ".disabled")
                        File.Move(mod.File, $"{Path.GetDirectoryName(mod.File)}\\{mod.Name}.jar");
            }

            LoadVersionMods((VersionListView.SelectedItem as GameListEntry).Entry!);
        };
        DeleteSelectModBtn.Click += async (_, _) =>
        {
            var mods = ModManageList.SelectedItems;
            var text = string.Empty;
            foreach (var item in mods)
            {
                var mod = item as ModManageEntry;
                text += $"• {Path.GetFileName(mod.File)}\n";
            }

            var dialog = await Method.Ui.ShowDialogAsync(MainLang.MoveToRecycleBin, text, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog == ContentDialogResult.Primary)
            {
                foreach (var item in mods)
                {
                    var mod = item as ModManageEntry;
                    FileSystem.DeleteFile(mod.File, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                }

                LoadVersionMods((VersionListView.SelectedItem as GameListEntry).Entry!);
            }
        };
        SliderBox.ValueChanged += (_, _) =>
        {
            if (SliderBox.Value == -1)
                SliderInfo.Text = MainLang.UseGlobalSetting;
            else
                SliderInfo.Text = $"{Math.Round(SliderBox.Value)} M";
        };
    }

    private void OpenVersionFolder(object? sender, RoutedEventArgs e)
    {
        var tag = ((Button)sender).Tag.ToString();
        var entry = (VersionListView.SelectedItem as GameListEntry).Entry;
        var root = Path.GetDirectoryName(entry.JarPath);
        Method.IO.TryCreateFolder(Path.Combine(root!, tag!));
        var launcher = TopLevel.GetTopLevel(this).Launcher;
        launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Path.Combine(root!, tag!)));
    }

    public void LoadVersions()
    {
        var setting = Const.Data.Setting;
        IGameResolver gameResolver = new GameResolver(setting.MinecraftFolder);
        var list = gameResolver.GetGameEntitys();
        VersionListView.Items.Clear();
        GameListEntry savedVersion = null;
        var vlist = new List<GameListEntry>();
        vlist.Add(new GameListEntry()
        {
            Id = MainLang.BedRockVersion, Type = "BedRock", Version = MainLang.IfInstallBedRock, Icon =
                Method.IO.LoadBitmapFromAppFile(
                    "YMCL.Main.Public.Assets.McIcons.dirt_path_side.png"),
            Entry = new GameEntry()
                { Id = MainLang.BedRockVersion, Type = "BedRock", Version = MainLang.IfInstallBedRock },
            IsSettingVisible = false
        });
        foreach (var version in list)
        {
            Bitmap icon = null;
            icon = version.MainLoaderType switch
            {
                LoaderType.Vanilla => version.Type switch
                {
                    "release" => Method.IO.LoadBitmapFromAppFile(
                        "YMCL.Main.Public.Assets.McIcons.grass_block_side.png"),
                    "snapshot" => Method.IO.LoadBitmapFromAppFile(
                        "YMCL.Main.Public.Assets.McIcons.crafting_table_front.png"),
                    _ => icon
                },
                LoaderType.Forge =>
                    Method.IO.LoadBitmapFromAppFile("YMCL.Main.Public.Assets.McIcons.furnace_front.png"),
                LoaderType.OptiFine => Method.IO.LoadBitmapFromAppFile(
                    "YMCL.Main.Public.Assets.McIcons.OptiFineIcon.png"),
                LoaderType.Fabric => Method.IO.LoadBitmapFromAppFile("YMCL.Main.Public.Assets.McIcons.FabricIcon.png"),
                LoaderType.Quilt => Method.IO.LoadBitmapFromAppFile("YMCL.Main.Public.Assets.McIcons.QuiltIcon.png"),
                _ => Method.IO.LoadBitmapFromAppFile("YMCL.Main.Public.Assets.McIcons.grass_block_side.png")
            };


            try
            {
                vlist.Add(new GameListEntry()
                {
                    Id = version.Id, Type = version.Type, Version = version.Version, Entry = version, Icon = icon,
                    IsFavourite = File.Exists(Path.Combine(Path.GetDirectoryName(version.JarPath)!, "YMCL.Favourite"))
                        ? 0
                        : 1,
                    FavouriteIcon = File.Exists(Path.Combine(Path.GetDirectoryName(version.JarPath)!, "YMCL.Favourite"))
                        ? "F1 M 4.21875 19.53125 C 4.049479 19.53125 3.902995 19.467773 3.779297 19.34082 C 3.655599 19.213867 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 L 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.571939 0.633139 9.672852 0.571289 C 9.773763 0.509441 9.879557 0.478516 9.990234 0.478516 C 10.107422 0.478516 10.218099 0.507812 10.322266 0.566406 C 10.426432 0.625 10.504557 0.709637 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.082683 19.918619 8.232422 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.420573 19.505209 4.322917 19.53125 4.21875 19.53125 Z "
                        : "F1 M 4.648438 12.675781 L 0.205078 8.349609 C 0.08138 8.225912 0.019531 8.079428 0.019531 7.910156 C 0.019531 7.760417 0.071615 7.623698 0.175781 7.5 C 0.279948 7.376303 0.406901 7.301434 0.556641 7.275391 L 6.689453 6.386719 L 9.443359 0.820312 C 9.495442 0.716146 9.573567 0.633139 9.677734 0.571289 C 9.7819 0.509441 9.889322 0.478516 10 0.478516 C 10.110677 0.478516 10.218099 0.509441 10.322266 0.571289 C 10.426432 0.633139 10.504557 0.716146 10.556641 0.820312 L 13.310547 6.386719 L 19.443359 7.275391 C 19.599609 7.301434 19.728189 7.373048 19.829102 7.490234 C 19.930012 7.607423 19.980469 7.744142 19.980469 7.900391 C 19.980469 8.076172 19.918619 8.225912 19.794922 8.349609 L 15.351562 12.675781 L 16.396484 18.789062 C 16.402994 18.815104 16.40625 18.850912 16.40625 18.896484 C 16.40625 19.065756 16.3444 19.21224 16.220703 19.335938 C 16.097004 19.459635 15.950521 19.521484 15.78125 19.521484 C 15.670572 19.521484 15.572916 19.498697 15.488281 19.453125 L 10 16.5625 L 4.511719 19.453125 C 4.427083 19.498697 4.329427 19.521484 4.21875 19.521484 C 4.049479 19.521484 3.902995 19.459635 3.779297 19.335938 C 3.655599 19.21224 3.59375 19.065756 3.59375 18.896484 C 3.59375 18.850912 3.597005 18.815104 3.603516 18.789062 Z M 5.048828 17.753906 L 10 15.146484 L 14.951172 17.753906 C 14.794922 16.829428 14.640299 15.909831 14.487305 14.995117 C 14.33431 14.080404 14.173177 13.160808 14.003906 12.236328 L 18.017578 8.330078 L 12.480469 7.529297 C 12.057291 6.689453 11.642252 5.852865 11.235352 5.019531 C 10.82845 4.186199 10.416666 3.349609 10 2.509766 C 9.583333 3.349609 9.171549 4.186199 8.764648 5.019531 C 8.357747 5.852865 7.942708 6.689453 7.519531 7.529297 L 1.982422 8.330078 L 5.996094 12.236328 C 5.826823 13.160808 5.66569 14.080404 5.512695 14.995117 C 5.3597 15.909831 5.205078 16.829428 5.048828 17.753906 Z "
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        var olist = vlist.OrderBy(entry => entry.IsFavourite)
            .ToList();
        Const.Window.main.launchPage.VersionListView.Items.Clear();
        foreach (var item in olist)
        {
            Const.Window.main.launchPage.VersionListView.Items.Add(item);
            if (setting.Version != null && item.Entry.Id == setting.Version) savedVersion = item;
        }

        if (savedVersion != null)
        {
            VersionListView.SelectedItem = savedVersion;
        }
        else
        {
            VersionListView.SelectedIndex = 0;
        }
    }

    private void LoadAccounts()
    {
        var accounts = JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath));
        AccountComboBox.Items.Clear();
        var setting = Const.Data.Setting;
        accounts.ForEach(x =>
        {
            SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
            var bytes = ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
            var skin = Method.Value.BytesToBase64(bytes);
            AccountComboBox.Items.Add(new AccountInfo
            {
                Name = x.Name,
                AccountType = x.AccountType,
                AddTime = x.AddTime,
                Data = x.Data,
                Bitmap = Method.Value.Base64ToBitmap(skin)
            });
        });

        if (AccountComboBox.Items.Count > 0)
        {
            if (setting.AccountSelectionIndex + 1 <= AccountComboBox.Items.Count)
            {
                AccountComboBox.SelectedIndex = setting.AccountSelectionIndex;
            }
            else
            {
                AccountComboBox.SelectedItem = AccountComboBox.Items[0];
                setting.AccountSelectionIndex = 0;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        }
        else
        {
            var now = DateTime.Now;
            File.WriteAllText(Const.String.AccountDataPath, JsonConvert.SerializeObject(new List<AccountInfo>
            {
                new()
                {
                    AccountType = AccountType.Offline,
                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Name = "Steve"
                }
            }, Formatting.Indented));
            setting.AccountSelectionIndex = 0;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            LoadAccounts();
        }

        if (setting.AccountSelectionIndex == -1 && accounts.Count > 0)
        {
            setting.AccountSelectionIndex = 0;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            LoadAccounts();
        }
    }

    private void LoadVersionMods(GameEntry version)
    {
        _modManageEntries = [];
        var disabledMod = new List<ModManageEntry>();
        if (version != null)
        {
            _modManageEntries.Clear();
            disabledMod.Clear();
            Task.Run(async () =>
            {
                var mods = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(version.JarPath)!, "mods"), "*.*",
                    SearchOption.AllDirectories);
                foreach (var mod in mods)
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (Path.GetExtension(mod) == ".jar")
                            _modManageEntries.Add(new ModManageEntry
                            {
                                Name = Path.GetFileName(mod).Substring(0, Path.GetFileName(mod).Length - 4),
                                File = mod
                            });
                        if (Path.GetExtension(mod) == ".disabled")
                            disabledMod.Add(new ModManageEntry
                            {
                                Name = Path.GetFileName(mod).Substring(0, Path.GetFileName(mod).Length - 13),
                                Decorations = TextDecorations.Strikethrough,
                                File = mod
                            });
                    });
                var i = 0;
                foreach (var item in disabledMod)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => { _modManageEntries.Insert(i, item); });
                    i++;
                }

                foreach (var item in _modManageEntries) ModManageList.Items.Add(item);
            });
        }

        var filteredItems = _modManageEntries
            .Where(item => item.Name.Contains(ModSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
        ModManageList.Items.Clear();
        filteredItems.ForEach(version => { ModManageList.Items.Add(version); });
    }

    public void LoadVersionSettingUI(GameEntry entry)
    {
        if (entry.Type == "BedRock") return;
        OverviewVersionId.Text = entry.Id;
        OverviewVersionInfo.Text = $"{entry.MainLoaderType}  {entry.Version}  Java{entry.JavaVersion}";

        if (entry == null)
        {
            Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
            return;
        }

        var filePath = Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.String.VersionSettingFileName);
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, JsonConvert.SerializeObject(new VersionSetting(), Formatting.Indented));
        var versionSetting = JsonConvert.DeserializeObject<VersionSetting>(File.ReadAllText(filePath));

        EnableIndependencyCoreComboBox.SelectedIndex = (int)versionSetting.EnableIndependencyCore;

        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath));
        JavaComboBox.Items.Clear();
        JavaComboBox.Items.Add(new JavaEntry
        {
            JavaPath = MainLang.UseGlobalSetting,
            JavaVersion = "All"
        });
        JavaComboBox.Items.Add(new JavaEntry
        {
            JavaPath = MainLang.LetYMCLChooseJava,
            JavaVersion = "All"
        });
        foreach (var item in javas) JavaComboBox.Items.Add(item);
        if (versionSetting.Java == null || versionSetting.Java.JavaPath == "Global" ||
            versionSetting.Java.JavaPath == string.Empty)
            JavaComboBox.SelectedIndex = 0;
        else if (versionSetting.Java.JavaPath == "Auto")
            JavaComboBox.SelectedIndex = 1;
        else if (!javas.Contains(versionSetting.Java) && versionSetting.Java.JavaPath != "Global" &&
                 versionSetting.Java.JavaPath != "Auto")
            JavaComboBox.SelectedIndex = 0;
        else
            JavaComboBox.SelectedItem = versionSetting.Java;

        var totalMemory = Method.Value.GetTotalMemory(Const.Data.Platform);
        if (totalMemory != 0)
            SliderBox.Maximum = totalMemory / 1024;
        else
            SliderBox.Maximum = 65536;
        SliderBox.Value = versionSetting.MaxMem;
        if (SliderBox.Value == -1)
            SliderInfo.Text = MainLang.UseGlobalSetting;
        else
            SliderInfo.Text = $"{Math.Round(SliderBox.Value)} M";
        AutoJoinServerTextBox.Text = versionSetting.AutoJoinServerIp;

        ModSearchBox.Text = string.Empty;
        LoadVersionMods(entry);
        SelectedModCount.Text = $"{MainLang.SelectedItem}0";
    }

    public async void CloseVersionSetting(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        VersionSettingRoot.Margin = new Thickness(30);
        VersionSettingRoot.Opacity = 0;
        await Task.Delay(210);
        if (!IsVersionSettingOpenByVersionList)
        {
            LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        }
        else
        {
            IsVersionSettingOpenByVersionList = false;
        }

        VersionSettingRoot.IsVisible = false;
    }

    public async void CloseVersionList()
    {
        VersionListRoot.Margin = new Thickness(30);
        VersionListRoot.Opacity = 0;
        await Task.Delay(210);
        LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        VersionListRoot.IsVisible = false;
    }

    public async void OpenVersionSetting()
    {
        VersionSettingRoot.IsVisible = true;
        LaunchConsoleRoot.Opacity = 0;
        VersionSettingRoot.Margin = new Thickness(10);
        VersionSettingRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        await Task.Delay(210);
    }

    public async Task OpenVersionList()
    {
        VersionListRoot.IsVisible = true;
        LaunchConsoleRoot.Opacity = 0;
        VersionListRoot.Margin = new Thickness(10);
        VersionListRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        await Task.Delay(210);
    }

    public async void ReLoadPage()
    {
        LoadAccounts();
        _minecraftFolders =
            JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath));
        MinecraftFolderComboBox.Items.Clear();
        var setting = Const.Data.Setting;
        foreach (var item in _minecraftFolders) MinecraftFolderComboBox.Items.Add(item);
        if (setting.MinecraftFolder == null || !_minecraftFolders.Contains(setting.MinecraftFolder))
            MinecraftFolderComboBox.SelectedIndex = 0;
        else
            MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
        if (_firstLoad)
        {
            Const.Window.main.settingPage.pluginSettingPage.LoadPlugin();
            _firstLoad = false;
        }

        LoadVersions();

        if (VersionListRoot.Margin == new Thickness(10))
        {
            LaunchConsoleRoot.IsVisible = false;
            LaunchConsoleRoot.Opacity = 0;
            await Task.Delay(260);
            LaunchConsoleRoot.IsVisible = true;
        }
        else
        {
            LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
            LaunchConsoleRoot.IsVisible = true;
        }
    }
}