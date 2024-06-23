using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Launch
{
    public partial class LaunchPage : UserControl
    {
        List<string> minecraftFolders = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath));
        bool _firstOpenVersionList = true;
        bool _updatingMcFolder = false;
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
                Method.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
                LoadAccounts();
                minecraftFolders = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath));
                MinecraftFolderComboBox.Items.Clear();
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                foreach (var item in minecraftFolders)
                {
                    MinecraftFolderComboBox.Items.Add(item);
                }
                if (setting.MinecraftFolder == null || !minecraftFolders.Contains(setting.MinecraftFolder))
                {
                    MinecraftFolderComboBox.SelectedIndex = 0;
                }
                else
                {
                    MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
                }
                LoadVersions();
            };
            AccountComboBox.SelectionChanged += (s, e) =>
            {
                AccountComboBox.IsVisible = false;
                AccountComboBox.IsVisible = true;
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (AccountComboBox.SelectedItem as AccountInfo != null)
                {
                    Head.Source = (AccountComboBox.SelectedItem as AccountInfo).Bitmap;
                }
                if (AccountComboBox.SelectedIndex == setting.AccountSelectionIndex || AccountComboBox.SelectedIndex == -1)
                {
                    return;
                }
                setting.AccountSelectionIndex = AccountComboBox.SelectedIndex;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            };
            MinecraftFolderComboBox.SelectionChanged += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (MinecraftFolderComboBox.SelectedItem == null || MinecraftFolderComboBox.SelectedItem.ToString() == setting.MinecraftFolder)
                {
                    return;
                }
                setting.MinecraftFolder = MinecraftFolderComboBox.SelectedItem.ToString();
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                _updatingMcFolder = true;
                LoadVersions();
            };
            VersionListBtn.Click += (s, e) =>
            {
                if (_firstOpenVersionList)
                {
                    VersionListRoot.Margin = new Avalonia.Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
                    VersionListRoot.IsVisible = true;
                    _firstOpenVersionList = false;
                    VersionListRoot.Margin = new Avalonia.Thickness(10);
                }
                else
                {
                    VersionListRoot.Margin = new Avalonia.Thickness(10);
                }
            };
            CloseVersionListBtn.Click += (s, e) =>
            {
                VersionListRoot.Margin = new Avalonia.Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
            };
            VersionListView.SelectionChanged += async (s, e) =>
            {
                if (_updatingMcFolder)
                {
                    _updatingMcFolder = false;
                    return;
                }
                if (VersionListView.SelectedItem != null)
                {
                    var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                    setting.Version = (VersionListView.SelectedItem as GameEntry).Id;
                    GameCoreText.Text = (VersionListView.SelectedItem as GameEntry).Id;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                }
                await Task.Delay(200);
                VersionListRoot.Margin = new Avalonia.Thickness(Root.Bounds.Width, 10, -1 * Root.Bounds.Width, 10);
            };
            LaunchBtn.Click += (s, e) => { _ = LaunchAsync(); };
        }
        void LoadVersions()
        {
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            IGameResolver gameResolver = new GameResolver(setting.MinecraftFolder);
            var list = gameResolver.GetGameEntitys();
            VersionListView.Items.Clear();
            var index = 0;
            var a = 0;
            foreach (var version in list)
            {
                VersionListView.Items.Add(version);
                if (setting.Version != null && version.Id == setting.Version)
                {
                    index = a;
                }
                a++;
            }
            if (setting.Version != null)
            {
                VersionListView.SelectedIndex = index;
            }
            else
            {
                if (!VersionListView.Items.Contains(setting.Version) && VersionListView.Items.Count > 0)
                {
                    VersionListView.SelectedIndex = 0;
                }
            }
        }
        void LoadAccounts()
        {
            var accounts = JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.AccountDataPath));
            AccountComboBox.Items.Clear();
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            accounts.ForEach(x =>
            {
                MinecraftLaunch.Skin.SkinResolver SkinResolver = new(Convert.FromBase64String(x.Skin));
                var bytes = MinecraftLaunch.Skin.ImageHelper.ConvertToByteArray(SkinResolver.CropSkinHeadBitmap());
                var skin = Method.BytesToBase64(bytes);
                AccountComboBox.Items.Add(new AccountInfo
                {
                    Name = x.Name,
                    AccountType = x.AccountType,
                    AddTime = x.AddTime,
                    Data = x.Data,
                    Bitmap = Method.Base64ToBitmap(skin)
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
                DateTime now = DateTime.Now;
                File.WriteAllText(Const.AccountDataPath, JsonConvert.SerializeObject(new List<AccountInfo>() { new AccountInfo
                {
                    AccountType = AccountType.Offline,
                    AddTime = now.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    Name = "Steve"
                }}, Formatting.Indented));
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
        public async Task LaunchAsync(string p_id = "", string p_javaPath = "", string p_mcPath = "", double p_maxMem = -1, bool p_enableIndependencyCore = true)
        {
            LaunchBtn.IsEnabled = false;
            GameEntry gameEntry = null;
            Account account = null;
            var l_id = string.Empty;
            var l_javaPath = string.Empty;
            var l_mcPath = string.Empty;
            double l_maxMem = -1;
            var l_enableIndependencyCore = true;

            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (string.IsNullOrEmpty(p_id))
            {
                if ((VersionListView.SelectedItem as GameEntry) != null)
                {
                    l_id = (VersionListView.SelectedItem as GameEntry).Id;
                }
                else
                {
                    Method.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    LaunchBtn.IsEnabled = true;
                    return;
                }
            }
            else
            {
                l_id = p_id;
            }
            if (string.IsNullOrEmpty(p_mcPath))
            {
                l_mcPath = setting.MinecraftFolder;
            }
            else
            {
                l_mcPath = p_mcPath;
            }
            IGameResolver gameResolver = new GameResolver(l_mcPath);
            gameEntry = gameResolver.GetGameEntity(l_id);
            if (gameEntry == null)
            {
                Method.Toast(MainLang.CreateGameEntryFail, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                return;
            }
            if (string.IsNullOrEmpty(p_javaPath))
            {
                if (setting.Java.JavaPath == "Auto")
                {
                    var javaEntry = JavaUtil.GetCurrentJava(JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath))!, gameEntry);
                    l_javaPath = javaEntry.JavaPath;
                }
                else
                {
                    l_javaPath = setting.Java.JavaPath;
                }
                if (l_javaPath == MainLang.LetYMCLChooseJava)
                {
                    Method.Toast(MainLang.CannotFandRightJava, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error); return;
                }
            }
            else
            {
                l_javaPath = p_javaPath;
            }
            if (p_maxMem == -1)
            {
                l_maxMem = setting.MaxMem;
            }
            else
            {
                l_maxMem = p_maxMem;
            }
            l_enableIndependencyCore = setting.EnableIndependencyCore;

            var task = new WindowTask(MainLang.LaunchProgress, false);
            task.UpdateTextProgress("-----> YMCL", false);
            task.UpdateTextProgress(MainLang.VerifyingAccount);

            var accountData = JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.AccountDataPath))[setting.AccountSelectionIndex];
            if (accountData == null)
            {
                Method.Toast(MainLang.AccountError, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                LaunchBtn.IsEnabled = true;
                task.Hide();
                return;
            }
            switch (accountData.AccountType)
            {
                case AccountType.Offline:
                    if (!string.IsNullOrEmpty(accountData.Name))
                    {
                        OfflineAuthenticator authenticator1 = new(accountData.Name);
                        account = authenticator1.Authenticate();
                    }
                    else
                    {
                        Method.Toast(MainLang.AccountError, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                        LaunchBtn.IsEnabled = true;
                        task.Hide();
                        return;
                    }
                    break;
                case AccountType.Microsoft:
                    var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(accountData.Data!);
                    MicrosoftAuthenticator authenticator2 = new(profile, Const.AzureClientId, true);
                    try
                    {
                        account = await authenticator2.AuthenticateAsync();
                    }
                    catch (Exception ex)
                    {
                        Method.ShowShortException(MainLang.LoginFail, ex);
                        LaunchBtn.IsEnabled = true;
                        task.Hide();
                        return;
                    }
                    break;
                case AccountType.ThirdParty:
                    account = JsonConvert.DeserializeObject<YggdrasilAccount>(accountData.Data!);
                    break;
            }
            if (account == null)
            {
                Method.Toast(MainLang.AccountError, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                LaunchBtn.IsEnabled = true;
                task.Hide();
                return;
            }

            if (string.IsNullOrEmpty(l_id) ||
            string.IsNullOrEmpty(l_mcPath) ||
            string.IsNullOrEmpty(l_javaPath) ||
            l_maxMem == -1)
            {
                Method.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                LaunchBtn.IsEnabled = true;
                task.Hide();
                return;
            }

            var config = new LaunchConfig
            {
                Account = account,
                JvmConfig = new JvmConfig(l_javaPath)
                {
                    MaxMemory = Convert.ToInt32(l_maxMem)
                },
                IsEnableIndependencyCore = l_enableIndependencyCore,
                LauncherName = "YMCL"
            };
            if (config == null)
            {
                Method.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                LaunchBtn.IsEnabled = true;
                task.Hide();
                return;
            }

            Method.Toast($"java:{l_javaPath},mem:{l_maxMem},core:{l_enableIndependencyCore},mcPath:{l_mcPath}", Const.Notification.main);

            Launcher launcher = new(gameResolver, config);

            await Task.Run(async () =>
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var watcher = await launcher.LaunchAsync(l_id);

                        watcher.Exited += (_, args) =>
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Toast($"{MainLang.GameExited}：{args.ExitCode}", Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Information);

                                if (args.ExitCode == 0)
                                {
                                    task.Hide();
                                    Const.Window.main.Focus();
                                }
                                else
                                {
                                    //var crashAnalyzer = new GameCrashAnalyzer(version, aloneCore);
                                    //var reports = crashAnalyzer.AnalysisLogs();
                                    //var msg = string.Empty;
                                    //foreach (var report in reports)
                                    //{
                                    //    msg += $"\n{report.CrashCauses}";
                                    //}
                                    //MessageBoxX.Show($"{MainLang.MinecraftCrash}\n{msg}", "Yu Minecraft Launcher");

                                    task.UpdateTextProgress(string.Empty, false);
                                    task.UpdateTextProgress($"YMCL -----> {MainLang.MineratCrashed}");
                                    task.isFinish = true;
                                }
                            });
                        };
                        watcher.OutputLogReceived += async (_, args) =>
                        {
                            Debug.WriteLine(args.Log);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(args.Original, false);
                            });
                        };

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTextProgress(MainLang.WaitForGameWindowAppear);
                            task.UpdateTextProgress("\n", false);
                            task.UpdateTextProgress("-----> JvmOutputLog", false);
                            Method.Toast(MainLang.LaunchFinish, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Success);
                        });

                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Method.ShowShortException(MainLang.LaunchFail, ex);
                        LaunchBtn.IsEnabled = true;
                        task.Hide();
                    });
                }
            });
            LaunchBtn.IsEnabled = true;
        }
    }
}
