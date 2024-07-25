using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Skin;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using SearchOption = System.IO.SearchOption;

namespace YMCL.Main.Views.Main.Pages.Launch;

public partial class LaunchPage : UserControl
{
    private bool _firstLoad = true;
    private bool _firstOpenVersionList = true;
    private bool _firstOpenVersionSetting = true;
    private bool _shouldCloseVersuionList;

    private List<string> minecraftFolders =
        JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath));

    private ObservableCollection<ModManageEntry> modManageEntries;

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
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            LoadAccounts();
            minecraftFolders =
                JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath));
            MinecraftFolderComboBox.Items.Clear();
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            foreach (var item in minecraftFolders) MinecraftFolderComboBox.Items.Add(item);
            if (setting.MinecraftFolder == null || !minecraftFolders.Contains(setting.MinecraftFolder))
                MinecraftFolderComboBox.SelectedIndex = 0;
            else
                MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
            if (_firstLoad)
            {
                Const.Window.main.settingPage.pluginSettingPage.LoadPlugin();
                _firstLoad = false;
                _shouldCloseVersuionList = true;
            }
            else
            {
                _shouldCloseVersuionList = false;
            }

            LoadVersions();
        };
        AccountComboBox.SelectionChanged += (s, e) =>
        {
            AccountComboBox.IsVisible = false;
            AccountComboBox.IsVisible = true;
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (AccountComboBox.SelectedItem as AccountInfo != null)
                Head.Source = (AccountComboBox.SelectedItem as AccountInfo).Bitmap;
            if (AccountComboBox.SelectedIndex == setting.AccountSelectionIndex ||
                AccountComboBox.SelectedIndex == -1) return;
            setting.AccountSelectionIndex = AccountComboBox.SelectedIndex;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        MinecraftFolderComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (MinecraftFolderComboBox.SelectedItem == null ||
                MinecraftFolderComboBox.SelectedItem.ToString() == setting.MinecraftFolder) return;
            setting.MinecraftFolder = MinecraftFolderComboBox.SelectedItem.ToString();
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            _shouldCloseVersuionList = false;
            LoadVersions();
            VersionListView.SelectedIndex = 0;
        };
        VersionListBtn.Click += async (s, e) =>
        {
            LaunchConsoleRoot.Opacity = 0;
            LoadVersions();
            _shouldCloseVersuionList = true;
            if (_firstOpenVersionList)
            {
                CloseVersionList();
                await Task.Delay(260);
                VersionListRoot.IsVisible = true;
                _firstOpenVersionList = false;
                VersionListRoot.Margin = new Thickness(10);
                VersionListRoot.IsVisible = true;
                LaunchConsoleRoot.Opacity = 0;
            }
            else
            {
                VersionListRoot.Margin = new Thickness(10);
                VersionListRoot.IsVisible = true;
                LaunchConsoleRoot.Opacity = 0;
            }
        };
        VersionSettingBtn.Click += async (s, e) =>
        {
            if (VersionListView.SelectedItem != null)
            {
                var entry = VersionListView.SelectedItem as GameEntry;
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
                        VersionSettingRoot.Margin = new Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
                        await Task.Delay(260);
                        VersionSettingRoot.IsVisible = false;
                        VersionSettingRoot.IsVisible = true;
                        _firstOpenVersionSetting = false;
                        VersionSettingRoot.Margin = new Thickness(10);
                        VersionSettingRoot.IsVisible = true;

                        LaunchConsoleRoot.Opacity = 0;
                        LoadVersionSettingUI(entry);
                    }
                    else
                    {
                        VersionSettingRoot.Margin = new Thickness(10);
                        VersionSettingRoot.IsVisible = true;

                        LaunchConsoleRoot.Opacity = 0;
                        LoadVersionSettingUI(entry);
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
        VersionListView.PointerEntered += (s, e) => { _shouldCloseVersuionList = true; };
        VersionListView.SelectionChanged += (s, e) =>
        {
            Task.Delay(20);
            if (VersionListView.SelectedItem != null)
            {
                var setting =
                    JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                GameCoreText.Text = (VersionListView.SelectedItem as GameEntry).Id;
                if (VersionListView.SelectedIndex == 0)
                    setting.Version = "BedRock";
                else
                    setting.Version = (VersionListView.SelectedItem as GameEntry).Id;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            Task.Delay(200);
            if (_shouldCloseVersuionList)
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
                    _ = Method.Mc.LaunchClientAsync();
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
            _shouldCloseVersuionList = false;
            LoadVersions();
        };
        VersionTabViewItemOverview.CloseRequested += CloseVersionSetting;
        VersionTabViewItemMod.CloseRequested += CloseVersionSetting;
        VersionTabViewItemSetting.CloseRequested += CloseVersionSetting;
        EnableIndependencyCoreComboBox.SelectionChanged += (_, _) =>
        {
            var entry = VersionListView.SelectedItem as GameEntry;
            var setting = Method.Mc.GetVersionSetting(entry!);
            setting.EnableIndependencyCore =
                (VersionSettingEnableIndependencyCore)EnableIndependencyCoreComboBox.SelectedIndex;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.VersionSettingFileName),
                JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        AutoJoinServerTextBox.TextChanged += (_, _) =>
        {
            var entry = VersionListView.SelectedItem as GameEntry;
            var setting = Method.Mc.GetVersionSetting(entry!);
            setting.AutoJoinServerIp = AutoJoinServerTextBox.Text;
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.VersionSettingFileName),
                JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        JavaComboBox.SelectionChanged += (_, _) =>
        {
            JavaComboBox.IsVisible = false;
            JavaComboBox.IsVisible = true;
            var version = VersionListView.SelectedItem as GameEntry;
            var filePath = Path.Combine(Path.GetDirectoryName(version.JarPath)!, Const.VersionSettingFileName);
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
            var filteredItems = modManageEntries
                .Where(item => item.Name.Contains(ModSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
            ModManageList.Items.Clear();
            filteredItems.ForEach(version => { ModManageList.Items.Add(version); });
        };
        RefreshModBtn.Click += (_, _) => { LoadVersionMods((VersionListView.SelectedItem as GameEntry)!); };
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

            LoadVersionMods((VersionListView.SelectedItem as GameEntry)!);
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

            LoadVersionMods((VersionListView.SelectedItem as GameEntry)!);
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

                LoadVersionMods((VersionListView.SelectedItem as GameEntry)!);
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
        var entry = VersionListView.SelectedItem as GameEntry;
        var root = Path.GetDirectoryName(entry.JarPath);
        Method.IO.TryCreateFolder(Path.Combine(root!, tag!));
        var launcher = TopLevel.GetTopLevel(this).Launcher;
        launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Path.Combine(root!, tag!)));
    }

    private void LoadVersions()
    {
        var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
        IGameResolver gameResolver = new GameResolver(setting.MinecraftFolder);
        var list = gameResolver.GetGameEntitys();
        VersionListView.Items.Clear();
        var index = 0;
        var a = 0;
        VersionListView.Items.Add(new GameEntry
            { Id = MainLang.BedRockVersion, Type = "BedRock", MainClass = MainLang.IfInstallBedRock });
        foreach (var version in list)
        {
            VersionListView.Items.Add(version);
            if (setting.Version != null && version.Id == setting.Version) index = a;
            a++;
        }

        if (setting.Version == "BedRock")
            index = 0;
        else
            index++;
        if (setting.Version != null)
        {
            VersionListView.SelectedIndex = index;
        }
        else
        {
            if (!VersionListView.Items.Contains(setting.Version) && VersionListView.Items.Count > 0)
                VersionListView.SelectedIndex = 0;
        }
    }

    private void LoadAccounts()
    {
        var accounts = JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.AccountDataPath));
        AccountComboBox.Items.Clear();
        var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
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
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        }
        else
        {
            var now = DateTime.Now;
            File.WriteAllText(Const.AccountDataPath, JsonConvert.SerializeObject(new List<AccountInfo>
            {
                new()
                {
                    AccountType = AccountType.Offline,
                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Name = "Steve"
                }
            }, Formatting.Indented));
            setting.AccountSelectionIndex = 0;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            LoadAccounts();
        }

        if (setting.AccountSelectionIndex == -1 && accounts.Count > 0)
        {
            setting.AccountSelectionIndex = 0;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            LoadAccounts();
        }
    }

    private void LoadVersionMods(GameEntry version)
    {
        modManageEntries = new ObservableCollection<ModManageEntry>();
        var disabledMod = new List<ModManageEntry>();
        if (version != null)
        {
            modManageEntries.Clear();
            disabledMod.Clear();
            Task.Run(async () =>
            {
                var mods = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(version.JarPath)!, "mods"), "*.*",
                    SearchOption.AllDirectories);
                foreach (var mod in mods)
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (Path.GetExtension(mod) == ".jar")
                            modManageEntries.Add(new ModManageEntry
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
                    await Dispatcher.UIThread.InvokeAsync(() => { modManageEntries.Insert(i, item); });
                    i++;
                }

                foreach (var item in modManageEntries) ModManageList.Items.Add(item);
            });
        }

        var filteredItems = modManageEntries
            .Where(item => item.Name.Contains(ModSearchBox.Text!, StringComparison.OrdinalIgnoreCase)).ToList();
        ModManageList.Items.Clear();
        filteredItems.ForEach(version => { ModManageList.Items.Add(version); });
    }

    private void LoadVersionSettingUI(GameEntry entry)
    {
        if (entry.Type == "BedRock") return;
        OverviewVersionId.Text = entry.Id;
        OverviewVersionInfo.Text = $"{entry.MainLoaderType}  {entry.Version}  Java{entry.JavaVersion}";

        if (entry == null)
        {
            Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
            return;
        }

        var filePath = Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.VersionSettingFileName);
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, JsonConvert.SerializeObject(new VersionSetting(), Formatting.Indented));
        var versionSetting = JsonConvert.DeserializeObject<VersionSetting>(File.ReadAllText(filePath));

        EnableIndependencyCoreComboBox.SelectedIndex = (int)versionSetting.EnableIndependencyCore;

        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath));
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

        var totalMemory = Method.Value.GetTotalMemory(Const.Platform);
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
        VersionSettingRoot.Margin = new Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
        LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        VersionSettingRoot.IsVisible = false;
    }

    public async void CloseVersionList()
    {
        VersionListRoot.Margin = new Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
        LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
        VersionListRoot.IsVisible = false;
    }
}