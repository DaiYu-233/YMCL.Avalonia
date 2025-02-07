using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Analyzer;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Extensions;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Views.Log;
using Setting = YMCL.Public.Enum.Setting;
using String = YMCL.Public.Const.String;

namespace YMCL.Public.Module.Mc.Launcher;

public class LaunchJavaClientByMinecraftLaunch
{
    public static async Task<bool> Launch(string p_id, string p_mcPath, double p_maxMem, string p_javaPath,
        string? p_fullUrl = null, bool p_enableIndependencyCore = true, bool p_isDebug = false)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        object[] args = [p_id, p_mcPath, p_maxMem, p_javaPath];
        foreach (var t in args)
        {
            if (t != null) continue;
            var exception = new ArgumentNullException(nameof(t), $"{nameof(t)} cannot be null.");
            Toast($"{MainLang.LaunchFail}\n{exception.Message}", NotificationType.Error);
            return false;
        }

        var resolver = new GameResolver(p_mcPath);
        var entry = resolver.GetGameEntity(p_id);
        if (entry == null)
        {
            Toast(MainLang.CreateGameEntryFail, NotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(entry.JarPath) || !File.Exists(entry.JarPath))
        {
            Toast(MainLang.GameMainFileDeletion, NotificationType.Error);
            return false;
        }

        var host = string.Empty;
        var port = 25565;
        if (!string.IsNullOrWhiteSpace(p_fullUrl))
        {
            try
            {
                var uri = new Uri(p_fullUrl);
                host = uri.Host;
                if (uri.Port != -1)
                {
                    port = uri.Port;
                }
            }
            catch (UriFormatException)
            {
                Toast(MainLang.ServerUrlError, NotificationType.Error);
                return false;
            }
        }

        if (Data.Setting.Account == null)
        {
            Toast(MainLang.AccountError);
            return false;
        }

        ObservableCollection<SubTask> subTasks =
        [
            new(MainLang.CheckLaunchArg),
            new(MainLang.RefreshAccountToken),
            new(MainLang.BuildLaunchConfig),
            new(MainLang.LaunchMinecraftProcess)
        ];
        var task = new TaskEntry($"{MainLang.Launch}: {entry.Id}", subTasks, TaskState.Running);
        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        var canceled = false;

        task.UpdateAction(() =>
        {
            canceled = true;
            task.CancelWaitFinish();
            cts.Cancel();
        });
        task.AdvanceSubTask();

        Account? account = null!;
        switch (Data.Setting.Account.AccountType)
        {
            case Setting.AccountType.Offline:
                if (!string.IsNullOrWhiteSpace(Data.Setting.Account.Name))
                {
                    OfflineAuthenticator authenticator1 = new(Data.Setting.Account.Name);
                    account = authenticator1.Authenticate();
                }
                else
                {
                    Toast(MainLang.AccountError);
                    task.FinishWithError();
                    return false;
                }

                break;
            case Setting.AccountType.Microsoft:
                var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(Data.Setting.Account.Data!);
                MicrosoftAuthenticator authenticator2 = new(profile, String.AzureClientId, true);
                try
                {
                    account = await authenticator2.AuthenticateAsync();
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.LoginFail, ex);
                    task.FinishWithError();
                    return false;
                }

                break;
            case Setting.AccountType.ThirdParty:
                account = JsonConvert.DeserializeObject<YggdrasilAccount>(Data.Setting.Account.Data!);
                break;
        }

        if (canceled)
        {
            Toast($"{MainLang.Canceled}: {MainLang.Launch} - {entry.Id}");
            task.CancelFinish();
            return false;
        }

        if (account == null)
        {
            Toast(MainLang.AccountError);
            task.FinishWithError();
            return false;
        }

        task.AdvanceSubTask();

        var config = new LaunchConfig
        {
            Account = account,
            JvmConfig = new JvmConfig(p_javaPath)
            {
                MaxMemory = Convert.ToInt32(p_maxMem)
            },
            IsEnableIndependencyCore = p_isDebug,
            LauncherName = "YMCL",
            ServerConfig = new ServerConfig(port, host)
        };

        task.AdvanceSubTask();

        MinecraftLaunch.Components.Launcher.Launcher launcher = new(resolver, config);

        var isKillByYmcl = false;
        var window = new LogWindow();
        task.UpdateDestoryAction(() => { window.Destory(); });
        
        try
        {
            await Task.Run(async () =>
            {
                try
                {
                    var watcher = await launcher.LaunchAsync(p_id);
                    var copyArguments = string.Join(" ", watcher.Arguments);
                    watcher.Exited += async (_, eventArgs) =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            if (Data.Setting.LauncherVisibility !=
                                Setting.LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                            {
                                if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window)
                                {
                                    window.Show();
                                    window.WindowState = WindowState.Normal;
                                    window.Activate();
                                }
                            }

                            Toast($"{MainLang.GameExited} - {p_id} : {eventArgs.ExitCode}");


                            if (eventArgs.ExitCode == 0)
                            {
                                task.FinishWithSuccess();
                                await Task.Delay(2000);
                                if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window)
                                {
                                    window.Activate();
                                    window.Focus();
                                }
                            }
                            else
                            {
                                if (!isKillByYmcl)
                                {
                                    var crashAnalyzer = new GameCrashAnalyzer(entry, p_enableIndependencyCore);
                                    var reports = crashAnalyzer.AnalysisLogs();
                                    var msg = string.Empty;
                                    try
                                    {
                                        var crashReports = reports.ToList();
                                        if (reports == null || crashReports.Count == 0)
                                            msg = MainLang.NoCrashInfo;
                                        else
                                            msg = crashReports.Aggregate(msg,
                                                (current, report) => current + $"\n{report.CrashCauses}");
                                    }
                                    catch
                                    {
                                        msg = MainLang.NoCrashInfo;
                                    }

                                    task.FinishWithError();
                                    await ShowDialogAsync(MainLang.MineratCrashed, msg,
                                        b_primary: MainLang.Ok);
                                    task.FinishWithError();
                                }
                            }
                        });
                    };

                    watcher.OutputLogReceived += (_, eventArgs) =>
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            window.Append(eventArgs.Log, eventArgs.Time , (LogType)eventArgs.LogType);
                        }, DispatcherPriority.ApplicationIdle);
                    };

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.AdvanceSubTask();
                        Toast($"{MainLang.LaunchFinish} - {p_id}");
                        task.AddOperateButton(new TaskEntryOperateButtonEntry(MainLang.DisplayLaunchArguments,
                            async () =>
                            {
                                var dialog = await ShowDialogAsync(MainLang.LaunchArguments,
                                    string.Join(" \n", watcher.Arguments), b_cancel: MainLang.Ok,
                                    b_primary: MainLang.Copy);
                                if(dialog != ContentDialogResult.Primary) return;
                                var clipboard = TopLevel.GetTopLevel(App.UiRoot)?.Clipboard;
                                await clipboard.SetTextAsync(copyArguments);
                                Toast(MainLang.AlreadyCopyToClipBoard, NotificationType.Success);
                            }));
                        task.AddOperateButton(new TaskEntryOperateButtonEntry(MainLang.KillProcess, async () =>
                        {
                            try
                            {
                                canceled = true;
                                isKillByYmcl = true;
                                watcher.Process.Kill(true);
                                task.CancelWithSuccess();
                                await cts.CancelAsync();
                            }
                            catch
                            {
                            }
                        }));
                        task.AddOperateButton(new TaskEntryOperateButtonEntry("显示Minecraft日志", async () =>
                        {
                            window.Show();
                            window.Activate();
                        }));
                    });
                    _ = Task.Run(async () =>
                    {
                        task.UpdateButtonText(MainLang.KillProcess);
                        task.UpdateAction(() =>
                        {
                            try
                            {
                                canceled = true;
                                isKillByYmcl = true;
                                watcher.Process.Kill(true);
                                task.CancelWithSuccess();
                                cts.Cancel();
                            }
                            catch
                            {
                            }
                        });
                        await Task.Delay(8000);
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            switch (Data.Setting.LauncherVisibility)
                            {
                                case Setting.LauncherVisibility.AfterLaunchExitLauncher:
                                    Environment.Exit(0);
                                    break;
                                case Setting.LauncherVisibility.AfterLaunchMakeLauncherMinimize:
                                case Setting.LauncherVisibility.AfterLaunchMinimizeAndShowWhenGameExit:
                                    if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window)
                                    {
                                        window.WindowState = WindowState.Minimized;
                                    }

                                    break;
                                case Setting.LauncherVisibility.AfterLaunchHideAndShowWhenGameExit:
                                    if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window1)
                                    {
                                        window1.Hide();
                                    }

                                    break;
                                case Setting.LauncherVisibility.AfterLaunchKeepLauncherVisible:
                                default:
                                    break;
                            }
                        });
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowShortException(MainLang.LaunchFail, ex);
                        task.FinishWithError();
                    });
                }
            }, token);
        }
        catch (OperationCanceledException)
        {
            Toast($"{MainLang.Canceled}: {MainLang.Launch} - {entry.Id}");
            task.CancelFinish();
            return false;
        }

        await Dispatcher.UIThread.InvokeAsync(() => { });
        await Task.Delay(20);
        return true;
    }
}