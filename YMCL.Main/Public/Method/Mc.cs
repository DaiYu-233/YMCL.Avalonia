using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Threading;
using CurseForge.APIClient;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Analyzer;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using StarLight_Core.Authentication;
using StarLight_Core.Launch;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.TaskManage;
using YMCL.Main.Public.Langs;
using Account = MinecraftLaunch.Classes.Models.Auth.Account;
using LaunchConfig = MinecraftLaunch.Classes.Models.Launch.LaunchConfig;
using MicrosoftAccount = MinecraftLaunch.Classes.Models.Auth.MicrosoftAccount;
using YggdrasilAccount = MinecraftLaunch.Classes.Models.Auth.YggdrasilAccount;

namespace YMCL.Main.Public;

public partial class Method
{
    public static partial class Mc
    {
        public static VersionSetting GetVersionSetting(GameEntry entry)
        {
            try
            {
                if (entry == null)
                {
                    Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
                    return null;
                }

                var filePath = Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.String.VersionSettingFileName);
                if (!File.Exists(filePath))
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(new VersionSetting(), Formatting.Indented));
                var versionSetting = JsonConvert.DeserializeObject<VersionSetting>(File.ReadAllText(filePath));
                return versionSetting;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> InstallClientUsingMinecraftLaunchAsync(string versionId, string customId = null,
            ForgeInstallEntry forgeInstallEntry = null, FabricBuildEntry fabricBuildEntry = null,
            QuiltBuildEntry quiltBuildEntry = null, OptiFineInstallEntity optiFineInstallEntity = null,
            TaskManager.TaskEntry p_task = null, bool closeTask = true)
        {
            var shouldReturn = false;
            var regex = new Regex(@"[\\/:*?""<>|]");
            var matches = regex.Matches(customId);
            if (matches.Count > 0)
            {
                var str = string.Empty;
                foreach (Match match in matches) str += match.Value;
                Ui.Toast($"{MainLang.IncludeSpecialWord}: {str}", Const.Notification.main,
                    NotificationType.Error);
                return false;
            }

            var setting = Const.Data.Setting;
            var resolver = new GameResolver(setting.MinecraftFolder);
            var vanlliaInstaller = new VanlliaInstaller(resolver, versionId, MirrorDownloadManager.Bmcl);
            if (Directory.Exists(Path.Combine(setting.MinecraftFolder, "versions", customId)))
            {
                Ui.Toast($"{MainLang.FolderAlreadyExists}: {customId}", Const.Notification.main,
                    NotificationType.Error);
                return false;
            }

            MirrorDownloadManager.IsUseMirrorDownloadSource = setting.DownloadSource == DownloadSource.BmclApi;

            Const.Window.main.downloadPage.autoInstallPage.InstallPreviewRoot.IsVisible = false;
            Const.Window.main.downloadPage.autoInstallPage.InstallableVersionListRoot.IsVisible = true;

            var task = p_task == null
                ? new TaskManager.TaskEntry($"{MainLang.Install}: Vanllia - {versionId}")
                : p_task;
            task.UpdateTextProgress("-----> Vanllia", false);

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
                            Ui.Toast($"{MainLang.InstallFail}: Vanllia - {versionId}", Const.Notification.main,
                                NotificationType.Error);
                        });
                        shouldReturn = true;
                    }
                    else
                    {
                        if (forgeInstallEntry == null && quiltBuildEntry == null && fabricBuildEntry == null)
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFinish}: Vanllia - {versionId}",
                                    Const.Notification.main, NotificationType.Success);
                            });
                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Ui.ShowShortException($"{MainLang.InstallFail}: Vanllia - {versionId}", ex);
                    });
                    shouldReturn = true;
                }
            });
            if (shouldReturn) return false;

            //Forge
            if (forgeInstallEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(
                            File.ReadAllText(Const.String.JavaDataPath));
                        if (javas.Count <= 0)
                        {
                            Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                            shouldReturn = true;
                        }
                        else
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var forgeInstaller = new ForgeInstaller(game, forgeInstallEntry, javas[0].JavaPath,
                                customId, MirrorDownloadManager.Bmcl);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTitle($"{MainLang.Install}: Forge - {versionId}");
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
                                    Ui.Toast($"{MainLang.InstallFinish}: Forge - {versionId}",
                                        Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Ui.Toast($"{MainLang.InstallFail}: Forge - {customId}", Const.Notification.main,
                                        NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: Forge - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            //OptiFine
            if (optiFineInstallEntity != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(
                            File.ReadAllText(Const.String.JavaDataPath));
                        if (javas.Count <= 0)
                        {
                            Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                            shouldReturn = true;
                        }
                        else
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var optifineInstaller = new OptifineInstaller(game, optiFineInstallEntity,
                                javas[0].JavaPath, customId, MirrorDownloadManager.Bmcl);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTitle($"{MainLang.Install}: OptiFine - {versionId}");
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
                                    Ui.Toast($"{MainLang.InstallFinish}: OptiFine - {versionId}",
                                        Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Ui.Toast($"{MainLang.InstallFail}: OptiFine - {customId}",
                                        Const.Notification.main, NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: OptiFine - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            //Fabric
            if (fabricBuildEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var game = resolver.GetGameEntity(versionId);
                        var fabricInstaller =
                            new FabricInstaller(game, fabricBuildEntry, customId, MirrorDownloadManager.Bmcl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTitle($"{MainLang.Install}: Fabric - {versionId}");
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
                                Ui.Toast($"{MainLang.InstallFinish}: Fabric - {versionId}", Const.Notification.main,
                                    NotificationType.Success);
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFail}: Fabric - {customId}", Const.Notification.main,
                                    NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: Fabric - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            //Quilt
            if (quiltBuildEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var game = resolver.GetGameEntity(versionId);
                        var quiltInstaller =
                            new QuiltInstaller(game, quiltBuildEntry, customId, MirrorDownloadManager.Bmcl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTitle($"{MainLang.Install}: Quilt - {versionId}");
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
                                Ui.Toast($"{MainLang.InstallFinish}: Quilt - {versionId}", Const.Notification.main,
                                    NotificationType.Success);
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFail}: Quilt - {customId}", Const.Notification.main,
                                    NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: Quilt - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            Const.Window.main.Activate();
            if (closeTask) task.Destory();
            return true;
        }

        public static async Task<bool> LaunchClientUsingMinecraftLaunchAsync(string p_id = "", string p_javaPath = "",
            string p_mcPath = "",
            double p_maxMem = -1, string p_enableIndependencyCore = "unset", string p_fullUrl = "")
        {
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = false;
            GameEntry gameEntry = null;
            Account account = null;
            var l_id = string.Empty;
            var l_ip = string.Empty;
            var l_port = 25565;
            var l_javaPath = string.Empty;
            var l_mcPath = string.Empty;
            double l_maxMem = -1;
            var l_enableIndependencyCore = true;

            var setting = Const.Data.Setting;
            if (string.IsNullOrWhiteSpace(p_id))
            {
                if (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry != null)
                {
                    l_id = (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry).Id;
                }
                else
                {
                    Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    return false;
                }
            }
            else
            {
                l_id = p_id;
            }

            if (string.IsNullOrWhiteSpace(p_mcPath))
                l_mcPath = setting.MinecraftFolder;
            else
                l_mcPath = p_mcPath;
            IGameResolver gameResolver = new GameResolver(l_mcPath);
            gameEntry = gameResolver.GetGameEntity(l_id);
            if (gameEntry == null)
            {
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Ui.Toast(MainLang.CreateGameEntryFail, Const.Notification.main, NotificationType.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(gameEntry.JarPath) || !File.Exists(gameEntry.JarPath))
            {
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Ui.Toast(MainLang.GameMainFileDeletion, Const.Notification.main, NotificationType.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(gameEntry.JarPath) || !File.Exists(gameEntry.JarPath))
            {
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Ui.Toast(MainLang.GameMainFileDeletion, Const.Notification.main, NotificationType.Error);
                return false;
            }

            var versionSetting = GetVersionSetting(gameEntry);
            if (versionSetting == null)
            {
                Ui.Toast(MainLang.CannotLoadVersionSetting, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return false;
            }

            var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath));
            if (javas.Count == 0)
            {
                Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(p_javaPath))
            {
                if (versionSetting.Java.JavaPath == "Global")
                {
                    if (setting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = setting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        return false;
                    }
                }
                else
                {
                    if (versionSetting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = versionSetting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        return false;
                    }
                }
            }
            else
            {
                l_javaPath = p_javaPath;
            }

            if (p_maxMem == -1)
            {
                if (versionSetting.MaxMem == -1)
                    l_maxMem = setting.MaxMem;
                else
                    l_maxMem = versionSetting.MaxMem;
            }
            else
            {
                l_maxMem = p_maxMem;
            }

            if (p_enableIndependencyCore == "unset")
            {
                if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Global)
                {
                    l_enableIndependencyCore = setting.EnableIndependencyCore;
                }
                else
                {
                    if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Off)
                        l_enableIndependencyCore = false;
                }
            }
            else
            {
                if (p_enableIndependencyCore == "false" || p_enableIndependencyCore == "False")
                    l_enableIndependencyCore = true;
                else
                    l_enableIndependencyCore = false;
            }

            if (string.IsNullOrWhiteSpace(p_fullUrl))
            {
                if (!string.IsNullOrWhiteSpace(versionSetting.AutoJoinServerIp))
                {
                    if (versionSetting.AutoJoinServerIp.Contains(':'))
                    {
                        var arr = versionSetting.AutoJoinServerIp.Split(':');
                        l_ip = arr[0];
                        l_port = Convert.ToInt16(arr[1]);
                    }
                    else
                    {
                        l_ip = versionSetting.AutoJoinServerIp;
                        l_port = 25565;
                    }
                }
            }
            else
            {
                if (p_fullUrl.Contains(':'))
                {
                    var arr = p_fullUrl.Split(':');
                    l_ip = arr[0];
                    l_port = Convert.ToInt16(arr[1]);
                }
                else
                {
                    l_ip = versionSetting.AutoJoinServerIp;
                    l_port = 25565;
                }
            }

            var homePageTask = new TextBox()
                { IsReadOnly = true, FontFamily = (FontFamily)Application.Current.Resources["Font"]!, FontSize = 14 };
            Const.Window.main.launchPage.LaunchingPanel.Children.Add(homePageTask);
            homePageTask.Text += $"{MainLang.Launch} - {gameEntry.Id}";
            var task = new TaskManager.TaskEntry($"{MainLang.Launch} - {gameEntry.Id}", false);
            task.UpdateTextProgress("-----> YMCL", false);
            homePageTask.Text += $"\n[{DateTime.Now:HH:mm:ss}]" + $"{MainLang.VerifyingAccount}";
            task.UpdateTextProgress(MainLang.VerifyingAccount);

            var accountData =
                JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath))[
                    setting.AccountSelectionIndex];
            if (accountData == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                return false;
            }

            switch (accountData.AccountType)
            {
                case AccountType.Offline:
                    if (!string.IsNullOrWhiteSpace(accountData.Name))
                    {
                        OfflineAuthenticator authenticator1 = new(accountData.Name);
                        account = authenticator1.Authenticate();
                    }
                    else
                    {
                        Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                        return false;
                    }

                    break;
                case AccountType.Microsoft:
                    var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(accountData.Data!);
                    MicrosoftAuthenticator authenticator2 = new(profile, Const.String.AzureClientId, true);
                    try
                    {
                        account = await authenticator2.AuthenticateAsync();
                    }
                    catch (Exception ex)
                    {
                        Ui.ShowShortException(MainLang.LoginFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                        return false;
                    }

                    break;
                case AccountType.ThirdParty:
                    account = JsonConvert.DeserializeObject<YggdrasilAccount>(accountData.Data!);
                    break;
            }

            if (account == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                task.Destory();
                return false;
            }

            if (string.IsNullOrWhiteSpace(l_id) ||
                string.IsNullOrWhiteSpace(l_mcPath) ||
                string.IsNullOrWhiteSpace(l_javaPath) ||
                l_maxMem == -1)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                task.Destory();
                return false;
            }

            var config = new LaunchConfig
            {
                Account = account,
                JvmConfig = new JvmConfig(l_javaPath)
                {
                    MaxMemory = Convert.ToInt32(l_maxMem)
                },
                IsEnableIndependencyCore = l_enableIndependencyCore,
                LauncherName = "YMCL",
                ServerConfig = new ServerConfig(l_port, l_ip)
            };
            if (config == null)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                task.Destory();
                return false;
            }

            Launcher launcher = new(gameResolver, config);
            string _gameMsg = String.Empty;
            await Task.Run(async () =>
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var watcher = await launcher.LaunchAsync(l_id);
                        _ = Task.Run(() => { IO.CallEnabledPlugin(); });

                        watcher.Exited += async (_, args) =>
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                if (setting.LauncherVisibility !=
                                    LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                                {
                                    Const.Window.main.Show();
                                    Const.Window.main.WindowState = WindowState.Normal;
                                    Const.Window.main.Activate();
                                }

                                Ui.Toast($"{MainLang.GameExited} - {l_id} : {args.ExitCode}", Const.Notification.main);

                                if (args.ExitCode == 0)
                                {
                                    Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                                    await Task.Delay(2000);
                                    task.Destory();
                                    Const.Window.main.Focus();
                                }
                                else
                                {
                                    var crashAnalyzer = new GameCrashAnalyzer(gameEntry, l_enableIndependencyCore);
                                    var reports = crashAnalyzer.AnalysisLogs();
                                    var msg = string.Empty;
                                    task._windowTaskEntry.ProgressTextBox.Text =
                                        task._pageTaskEntry.TaskTextBox.Text;
                                    try
                                    {
                                        if (reports == null || reports.Count() == 0)
                                            msg = MainLang.NoCrashInfo;
                                        else
                                            foreach (var report in reports)
                                                msg += $"\n{report.CrashCauses}";
                                    }
                                    catch
                                    {
                                        msg = MainLang.NoCrashInfo;
                                    }

                                    task.UpdateTextProgress(string.Empty, false);
                                    task.UpdateTextProgress($"YMCL -----> \n{MainLang.MineratCrashed}");
                                    task.Finish();
                                    task.Show();
                                    var dialogResult = await Ui.ShowDialogAsync(MainLang.MineratCrashed, msg,
                                        b_primary: MainLang.Ok);
                                    task.Destory();
                                    Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                                }
                            });
                        };

                        watcher.OutputLogReceived += async (_, args) =>
                        {
                            Console.WriteLine(args.Log);
                            if (!setting.ShowGameOutput)
                            {
                                task._pageTaskEntry.UpdateTextProgress(args.Original, false);
                            }
                            else
                            {
                                task.UpdateTextProgress(args.Original, false);
                            }
                        };

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            homePageTask.Text += $"\n[{DateTime.Now:HH:mm:ss}]" + $"{MainLang.WaitForGameWindowAppear}";
                            task.UpdateTextProgress(MainLang.WaitForGameWindowAppear);
                            if (setting.ShowGameOutput)
                            {
                                task.UpdateTextProgress("\n", false);
                                task.UpdateTextProgress("-----> JvmOutputLog", false);
                            }

                            Ui.Toast(MainLang.LaunchFinish, Const.Notification.main, NotificationType.Success);
                        });
                        _ = Task.Run(async () =>
                        {
                            watcher.Process.WaitForInputIdle();

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                                if (!setting.ShowGameOutput)
                                {
                                    task.Hide();
                                }

                                switch (setting.LauncherVisibility)
                                {
                                    case LauncherVisibility.AfterLaunchExitLauncher:
                                        Environment.Exit(0);
                                        break;
                                    case LauncherVisibility.AfterLaunchMakeLauncherMinimize:
                                    case LauncherVisibility.AfterLaunchMinimizeAndShowWhenGameExit:
                                        Const.Window.main.WindowState = WindowState.Minimized;
                                        break;
                                    case LauncherVisibility.AfterLaunchHideAndShowWhenGameExit:
                                        Const.Window.main.Hide();
                                        break;
                                    case LauncherVisibility.AfterLaunchKeepLauncherVisible:
                                    default:
                                        break;
                                }
                            });

                            if (!setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.Hide(); });
                        });
                    });
                }
                catch (Exception ex)
                {
                    var a = ex.ToString();
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Ui.ShowShortException(MainLang.LaunchFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                    });
                }
            });
            await Dispatcher.UIThread.InvokeAsync(() => { Const.Window.main.launchPage.LaunchBtn.IsEnabled = true; });
            await Task.Delay(20);
            return true;
        }

        public static async Task<bool> LaunchClientUsingStarLightAsync(string p_id = "", string p_javaPath = "",
            string p_mcPath = "",
            double p_maxMem = -1, string p_enableIndependencyCore = "unset", string p_fullUrl = "")
        {
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = false;
            GameEntry gameEntry = null;
            BaseAccount account = null;
            var l_id = string.Empty;
            var l_ip = string.Empty;
            var l_port = 25565;
            var l_javaPath = string.Empty;
            var l_mcPath = string.Empty;
            double l_maxMem = -1;
            var l_enableIndependencyCore = true;

            var setting = Const.Data.Setting;
            if (string.IsNullOrWhiteSpace(p_id))
            {
                if (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry != null)
                {
                    l_id = (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry).Id;
                }
                else
                {
                    Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    return false;
                }
            }
            else
            {
                l_id = p_id;
            }

            if (string.IsNullOrWhiteSpace(p_mcPath))
                l_mcPath = setting.MinecraftFolder;
            else
                l_mcPath = p_mcPath;
            IGameResolver gameResolver = new GameResolver(l_mcPath);
            gameEntry = gameResolver.GetGameEntity(l_id);
            if (gameEntry == null)
            {
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Ui.Toast(MainLang.CreateGameEntryFail, Const.Notification.main, NotificationType.Error);
                return false;
            }

            var versionSetting = GetVersionSetting(gameEntry);
            if (versionSetting == null)
            {
                Ui.Toast(MainLang.CannotLoadVersionSetting, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return false;
            }

            var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath));
            if (javas.Count == 0)
            {
                Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(p_javaPath))
            {
                if (versionSetting.Java.JavaPath == "Global")
                {
                    if (setting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = setting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        return false;
                    }
                }
                else
                {
                    if (versionSetting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = versionSetting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        return false;
                    }
                }
            }
            else
            {
                l_javaPath = p_javaPath;
            }

            if (p_maxMem == -1)
            {
                if (versionSetting.MaxMem == -1)
                    l_maxMem = setting.MaxMem;
                else
                    l_maxMem = versionSetting.MaxMem;
            }
            else
            {
                l_maxMem = p_maxMem;
            }

            if (p_enableIndependencyCore == "unset")
            {
                if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Global)
                {
                    l_enableIndependencyCore = setting.EnableIndependencyCore;
                }
                else
                {
                    if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Off)
                        l_enableIndependencyCore = false;
                }
            }
            else
            {
                if (p_enableIndependencyCore == "false" || p_enableIndependencyCore == "False")
                    l_enableIndependencyCore = true;
                else
                    l_enableIndependencyCore = false;
            }

            if (string.IsNullOrWhiteSpace(p_fullUrl))
            {
                if (!string.IsNullOrWhiteSpace(versionSetting.AutoJoinServerIp))
                {
                    if (versionSetting.AutoJoinServerIp.Contains(':'))
                    {
                        var arr = versionSetting.AutoJoinServerIp.Split(':');
                        l_ip = arr[0];
                        l_port = Convert.ToInt16(arr[1]);
                    }
                    else
                    {
                        l_ip = versionSetting.AutoJoinServerIp;
                        l_port = 25565;
                    }
                }
            }
            else
            {
                if (p_fullUrl.Contains(':'))
                {
                    var arr = p_fullUrl.Split(':');
                    l_ip = arr[0];
                    l_port = Convert.ToInt16(arr[1]);
                }
                else
                {
                    l_ip = versionSetting.AutoJoinServerIp;
                    l_port = 25565;
                }
            }

            var homePageTask = new TextBox()
                { IsReadOnly = true, FontFamily = (FontFamily)Application.Current.Resources["Font"]!, FontSize = 14 };
            Const.Window.main.launchPage.LaunchingPanel.Children.Add(homePageTask);
            homePageTask.Text += $"{MainLang.Launch} - {gameEntry.Id}";
            var task = new TaskManager.TaskEntry($"{MainLang.Launch} - {gameEntry.Id}", false);
            task.UpdateTextProgress("-----> YMCL", false);
            homePageTask.Text += $"\n[{DateTime.Now:HH:mm:ss}]" + $"{MainLang.VerifyingAccount}";
            task.UpdateTextProgress(MainLang.VerifyingAccount);

            var accountData =
                JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath))[
                    setting.AccountSelectionIndex];
            if (accountData == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                return false;
            }


            switch (accountData.AccountType)
            {
                case AccountType.Offline:
                    if (!string.IsNullOrWhiteSpace(accountData.Name))
                    {
                        account = new OfflineAuthentication(accountData.Name).OfflineAuth();
                    }
                    else
                    {
                        Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                        return false;
                    }

                    break;
                case AccountType.Microsoft:
                    var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(accountData.Data!);
                    var entry = new MicrosoftAuthentication(Const.String.AzureClientId);
                    try
                    {
                        account = await entry.MicrosoftAuthAsync(new GetTokenResponse()
                        {
                            AccessToken = profile.AccessToken,
                            RefreshToken = profile.RefreshToken,
                            ClientId = Const.String.AzureClientId
                        }, progress => { task.UpdateTextProgress(progress); }, profile.RefreshToken);
                    }
                    catch (Exception ex)
                    {
                        Ui.ShowShortException(MainLang.LoginFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                        return false;
                    }

                    break;
                case AccountType.ThirdParty:
                    var profile1 = JsonConvert.DeserializeObject<YggdrasilAccount>(accountData.Data!);
                    account = new StarLight_Core.Models.Authentication.YggdrasilAccount()
                    {
                        ServerUrl = profile1.YggdrasilServerUrl,
                        ClientToken = profile1.ClientToken,
                        Name = profile1.Name,
                        Uuid = profile1.Uuid.ToString(),
                        AccessToken = profile1.AccessToken
                    };
                    break;
            }

            if (account == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                return false;
            }

            if (string.IsNullOrWhiteSpace(l_id) ||
                string.IsNullOrWhiteSpace(l_mcPath) ||
                string.IsNullOrWhiteSpace(l_javaPath) ||
                l_maxMem == -1)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                return false;
            }

            var config = new StarLight_Core.Models.Launch.LaunchConfig()
            {
                GameCoreConfig = new GameCoreConfig()
                {
                    Root = l_mcPath,
                    Version = l_id,
                    Ip = l_ip,
                    Port = l_port.ToString(),
                    IsVersionIsolation = l_enableIndependencyCore
                },
                Account = new StarLight_Core.Models.Authentication.Account() { BaseAccount = account },
                JavaConfig = new JavaConfig()
                {
                    JavaPath = l_javaPath,
                    MaxMemory = Convert.ToInt32(l_maxMem)
                }
            };

            if (config == null)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                return false;
            }

            var launcher = new MinecraftLauncher(config);

            await Task.Run(async () =>
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var watcher = await launcher.LaunchAsync(async x =>
                        {
                            Console.WriteLine(x.Description);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(x.Description, true);
                            });
                        });

                        _ = Task.Run(() => { IO.CallEnabledPlugin(); });

                        watcher.OutputReceived += async a =>
                        {
                            Console.WriteLine(a);
                            await Dispatcher.UIThread.InvokeAsync(() => { task.UpdateTextProgress(a, true); });
                        };
                        watcher.ErrorReceived += async a =>
                        {
                            Console.WriteLine(a);
                            if (setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.UpdateTextProgress(a, true); });
                            else
                                await Dispatcher.UIThread.InvokeAsync(() => { task.UpdateTextProgress(a, true); });
                        };

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTextProgress(MainLang.WaitForGameWindowAppear);
                            homePageTask.Text += $"\n[{DateTime.Now:HH:mm:ss}]" + $"{MainLang.WaitForGameWindowAppear}";
                            if (setting.ShowGameOutput)
                            {
                                task.UpdateTextProgress("\n", false);
                                task.UpdateTextProgress("-----> JvmOutputLog", false);
                            }

                            Ui.Toast(MainLang.LaunchFinish, Const.Notification.main, NotificationType.Success);
                        });
                        _ = Task.Run(async () =>
                        {
                            watcher.Process.WaitForInputIdle();

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                                if (!setting.ShowGameOutput)
                                {
                                    task.Hide();
                                }

                                switch (setting.LauncherVisibility)
                                {
                                    case LauncherVisibility.AfterLaunchExitLauncher:
                                        Environment.Exit(0);
                                        break;
                                    case LauncherVisibility.AfterLaunchMakeLauncherMinimize:
                                    case LauncherVisibility.AfterLaunchMinimizeAndShowWhenGameExit:
                                        Const.Window.main.WindowState = WindowState.Minimized;
                                        break;
                                    case LauncherVisibility.AfterLaunchHideAndShowWhenGameExit:
                                        Const.Window.main.Hide();
                                        break;
                                    case LauncherVisibility.AfterLaunchKeepLauncherVisible:
                                    default:
                                        break;
                                }
                            });

                            if (!setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.Hide(); });
                        });
                        _ = Task.Run(async () =>
                        {
                            watcher.Process.Exited += async (a, b) =>
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    if (setting.LauncherVisibility !=
                                        LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                                    {
                                        Const.Window.main.Show();
                                        Const.Window.main.WindowState = WindowState.Normal;
                                        Const.Window.main.Activate();
                                    }

                                    Ui.Toast($"{MainLang.GameExited} - {l_id}", Const.Notification.main);
                                    task.Destory();
                                    Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                                });
                            };
                            if (!setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.Hide(); });
                        });
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Ui.ShowShortException(MainLang.LaunchFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        Const.Window.main.launchPage.LaunchingPanel.Children.Remove(homePageTask);
                    });
                }
            });
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
            return true;
        }

        public static async Task<bool> ImportModPackFromLocal(string path, bool confirmBox = true,
            string p_customId = null)
        {
            var setting = Const.Data.Setting;
            var customId = string.Empty;
            while (true)
            {
                if (!confirmBox) break;
                var textBox = new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Text = Path.GetFileNameWithoutExtension(path)
                };
                var dialog = new ContentDialog
                {
                    PrimaryButtonText = MainLang.Ok,
                    Content = textBox,
                    DefaultButton = ContentDialogButton.Primary,
                    CloseButtonText = MainLang.Cancel,
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = $"{MainLang.Install} - {Path.GetFileName(path)}"
                };
                var dialogResult = await dialog.ShowAsync(Const.Window.main);
                if (dialogResult == ContentDialogResult.Primary)
                {
                    if (Directory.Exists(Path.Combine(setting.MinecraftFolder, "versions", textBox.Text)))
                    {
                        Ui.Toast($"{MainLang.FolderAlreadyExists}: {textBox.Text}", Const.Notification.main,
                            NotificationType.Error);
                    }
                    else
                    {
                        customId = textBox.Text;
                        break;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(customId))
            {
                customId = p_customId;
            }

            var task = new TaskManager.TaskEntry($"{MainLang.Unzip} - {Path.GetFileName(path)}");

            IO.TryCreateFolder(Path.Combine(setting.MinecraftFolder, "YMCLTemp"));
            var unzipDirectory =
                Path.Combine(setting.MinecraftFolder, "YMCLTemp", Path.GetFileNameWithoutExtension(path)); //确定临时整合包的路径
            task.UpdateTextProgress(MainLang.UnzipingModPack);
            task.UpdateValueProgress(50);
            await Task.Run(() => { ZipFile.ExtractToDirectory(path /*Zip文件路径*/, unzipDirectory /*要解压到的目录*/, true); });
            task.UpdateValueProgress(100);
            task.UpdateTextProgress(MainLang.FinsihUnzipModPack);
            task.UpdateTextProgress(MainLang.GetModPackInfo);
            var json = File.ReadAllText(Path.Combine(unzipDirectory, "manifest.json")); //read json
            var info = JsonConvert.DeserializeObject<ModPackEntry.Root>(json);
            task.UpdateTextProgress(
                $"{MainLang.ModPackInfo}:\n    Name : \t\t\t{info.name}\n    Author : \t\t\t{info.author}\n    Version : \t\t\t{info.version}\n    McVersion : \t\t\t{info.minecraft.version}\n    Loader : \t\t\t{info.minecraft.modLoaders[0].id}");
            task.UpdateTitle($"{MainLang.Install} - {Path.GetFileName(path)}");
            var loaders = info.minecraft.modLoaders[0].id.Split('-');
            var result = false;
            if (loaders[0] == "forge")
            {
                var forges = (await ForgeInstaller.EnumerableFromVersionAsync(info.minecraft.version)).ToList();
                ForgeInstallEntry enrty = null;
                foreach (var forge in forges)
                    if (forge.ForgeVersion == loaders[1])
                    {
                        enrty = forge;
                        break;
                    }

                if (enrty == null) return false;
                result = await InstallClientUsingMinecraftLaunchAsync(info.minecraft.version, customId, p_task: task,
                    forgeInstallEntry: enrty, closeTask: false);
            }
            else if (loaders[0] == "fabric")
            {
                var fabrics = (await FabricInstaller.EnumerableFromVersionAsync(info.minecraft.version)).ToList();
                FabricBuildEntry enrty = null;
                foreach (var fabric in fabrics)
                    if (fabric.BuildVersion == loaders[1])
                    {
                        enrty = fabric;
                        break;
                    }

                if (enrty == null) return false;
                result = await InstallClientUsingMinecraftLaunchAsync(info.minecraft.version, customId, p_task: task,
                    fabricBuildEntry: enrty, closeTask: false);
            }
            else
            {
                task.Destory();
                return false;
            }

            if (!result) return false;
            task.Activate();

            var semaphore = new SemaphoreSlim(20); // 允许同时运行的下载任务数  
            var completedDownloads = 0; // 已完成下载的文件数量  
            var successDownloads = 0; // 成功下载的文件数量
            var totalDownloads = info.files.Count; // 总下载文件数量  
            ApiClient cfApiClient = new(Const.String.CurseForgeApiKey); // 创建一个CurseForge API 客户端
            var tasks = new List<Task>(); // 创建一个任务列表来存储下载任务
            var errors = new List<string>(); // 创建一个列表来存储下载错误

            if (info.files.Count > 0)
            {
                task.UpdateTitle(MainLang.DownloadModPackMod);
                task.UpdateTextProgress(MainLang.DownloadModPackMod);
                info.files.ForEach(file => { tasks.Add(GetAndDownloadMod(file.projectID, file.fileID)); });
                await Task.WhenAll(tasks);

                task.UpdateTextProgress("", false);
                task.UpdateTextProgress($"{MainLang.TotalNumberOfMod}: {totalDownloads}");
                task.UpdateTextProgress($"{MainLang.DownloadSuccess}: {successDownloads}");
                var text = string.Empty;
                errors.ForEach(error => text += error + "\n");
                task.UpdateTextProgress($"{MainLang.DownloadFail} ({totalDownloads - successDownloads}): \n{text}");

                var index = 1;
                var replaceUrl = true;
                while (true)
                {
                    if (index > 7) break;
                    task.UpdateTextProgress("", false);
                    task.UpdateTextProgress(MainLang.DownloadFailedFileAgain + $": {index}");
                    var redownload = errors;
                    totalDownloads = redownload.Count;
                    tasks.Clear();
                    successDownloads = 0;
                    redownload.ForEach(file =>
                    {
                        var dl = file;
                        if (replaceUrl) dl = dl.Replace("mediafilez.forgecdn.net", "edge.forgecdn.net");
                        tasks.Add(GetAndDownloadMod(url: dl));
                    });
                    if (replaceUrl)
                        replaceUrl = false;
                    else
                        replaceUrl = true;
                    errors.Clear();
                    await Task.WhenAll(tasks);
                    task.UpdateTextProgress($"{MainLang.TotalNumberOfMod}: {redownload.Count}");
                    task.UpdateTextProgress($"{MainLang.DownloadSuccess}: {successDownloads}");
                    text = string.Empty;
                    errors.ForEach(error => text += error + "\n");
                    task.UpdateTextProgress(
                        $"{MainLang.DownloadFail} ({redownload.Count - successDownloads}): \n{text}");
                    if (errors.Count == 0) break;
                    index++;
                }
            }

            if (!string.IsNullOrWhiteSpace(info.overrides))
            {
                task.UpdateTitle(MainLang.OverrideModPack);
                IO.CopyDirectory(Path.Combine(unzipDirectory, info.overrides),
                    Path.Combine(setting.MinecraftFolder, "versions", customId));
            }

            async Task GetAndDownloadMod(int projectId = -1, int fileId = -1, string url = null)
            {
                await semaphore.WaitAsync();
                var modFileDownloadUrl = string.Empty;
                var fileName = string.Empty;
                try
                {
                    if (url == null)
                        modFileDownloadUrl =
                            (await cfApiClient.GetModFileDownloadUrlAsync(projectId, fileId)).Data.Replace(
                                "edge.forgecdn.net", "mediafilez.forgecdn.net");
                    else
                        modFileDownloadUrl = url;

                    var saveDirectory = Path.Combine(setting.MinecraftFolder, "versions", customId, "mods");
                    IO.TryCreateFolder(saveDirectory);
                    if (string.IsNullOrWhiteSpace(modFileDownloadUrl))
                        throw new Exception("Failed to get download URL");

                    var uri = new Uri(modFileDownloadUrl);
                    fileName = Path.GetFileName(uri.AbsolutePath);
                    var savePath = Path.Combine(saveDirectory, fileName);
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(modFileDownloadUrl,
                            HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                               fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                   4096, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }

                        successDownloads++;
                    }

                    Interlocked.Increment(ref completedDownloads);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        task.UpdateValueProgress(completedDownloads / (double)totalDownloads * 100);
                        task.UpdateTextProgress($"{MainLang.DownloadFinish}: {fileName}");
                    });
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref completedDownloads);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        task.UpdateValueProgress(completedDownloads / (double)totalDownloads * 100);
                        task.UpdateTextProgress($"{MainLang.DownloadFail}: {fileName}");
                    });
                    errors.Add(modFileDownloadUrl);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            task.Destory();
            return true;
        }

        public static async Task ImportModPackFromCurseForge(ModFileListViewItemEntry item, string customId)
        {
            var shouldReturn = false;
            var fN = item.DisplayName;
            if (Path.GetExtension(fN) != ".zip") fN += ".zip";
            var path = Path.Combine(Const.String.TempFolderPath, fN);
            var task = new TaskManager.TaskEntry($"{MainLang.Download} - {fN}", true, false);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(
                               item.DownloadUrl.Replace("edge.forgecdn.net", "mediafilez.forgecdn.net"),
                               HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                                   FileShare.None, 4096, true))
                        {
                            var buffer = new byte[4096];
                            var totalBytesRead = 0L;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                var progressPercentage = (int)(totalBytesRead * 100 / totalBytes);
                                task.UpdateValueProgress(progressPercentage);
                            }

                            Method.Ui.Toast($"{MainLang.DownloadFinish}: {item.DisplayName}");
                            task.Destory();
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(item.DownloadUrl,
                                   HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode(); // 确保HTTP成功状态值  

                            var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                                       FileShare.None, 4096, true))
                            {
                                var buffer = new byte[4096];
                                var totalBytesRead = 0L;
                                int bytesRead;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;
                                    var progressPercentage = (int)(totalBytesRead * 100 / totalBytes);
                                    task.UpdateValueProgress(progressPercentage);
                                }

                                Method.Ui.Toast($"{MainLang.DownloadFinish}: {item.DisplayName}");
                                task.Destory();
                            }
                        }
                    }
                }
                catch
                {
                    shouldReturn = true;
                    Method.Ui.Toast($"{MainLang.DownloadFail}: {item.DisplayName}");
                    task.Destory();
                }
            }

            if (shouldReturn) return;
            ImportModPackFromLocal(path, false, customId);
        }
    }
}