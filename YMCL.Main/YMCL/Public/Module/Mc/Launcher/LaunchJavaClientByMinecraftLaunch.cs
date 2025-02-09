using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Authentication;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Launch;
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
    public static async Task<bool> Launch(string p_id, string p_mcPath, double p_maxMem,
        MinecraftLaunch.Base.Models.Game.JavaEntry p_javaPath,
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

        var parser = new MinecraftParser(p_mcPath);
        var entry = parser.GetMinecraft(p_id);
        if (entry == null)
        {
            Toast(MainLang.CreateGameEntryFail, NotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(entry.ClientJarPath) || !File.Exists(entry.ClientJarPath))
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
                    OfflineAuthenticator authenticator1 = new();
                    account = authenticator1.Authenticate(Data.Setting.Account.Name);
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
                MicrosoftAuthenticator authenticator2 = new(String.AzureClientId);
                try
                {
                    account = await authenticator2.RefreshAsync(profile, token);
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
            JavaPath = p_javaPath,
            MaxMemorySize = Convert.ToInt32(p_maxMem),
            MinMemorySize = 512,
            IsEnableIndependency = p_enableIndependencyCore,
            JvmArguments = [],
            LauncherName = "YMCL",
        };

        task.AdvanceSubTask();

        MinecraftRunner runner = new(config, parser);

        var isKillByYmcl = false;
        var window = new LogWindow();
        task.UpdateDestoryAction(() => { window.Destory(); });

        try
        {
            await Task.Run(async () =>
            {
                try
                {
                    var process = await runner.RunAsync(p_id, token);
                    var copyArguments = string.Join(" ", process.ArgumentList);
                    process.Exited += async (_, arg) =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            if (Data.Setting.LauncherVisibility !=
                                Setting.LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                            {
                                if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window1)
                                {
                                    window1.Show();
                                    window1.WindowState = WindowState.Normal;
                                    window1.Activate();
                                }
                            }

                            Toast($"{MainLang.GameExited} - {p_id}");

                            // if (process.Process.ExitCode == 0)
                            // {
                            task.FinishWithSuccess();
                            await Task.Delay(2000);
                            if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window2)
                            {
                                window2.Activate();
                                window2.Focus();
                            }
                            // }
                            // else
                            // {
                            //     if (!isKillByYmcl)
                            //     {
                            //         var crashAnalyzer = new GameCrashAnalyzer(entry, p_enableIndependencyCore);
                            //         var reports = crashAnalyzer.AnalysisLogs();
                            //         var msg = string.Empty;
                            //         try
                            //         {
                            //             var crashReports = reports.ToList();
                            //             if (reports == null || crashReports.Count == 0)
                            //                 msg = MainLang.NoCrashInfo;
                            //             else
                            //                 msg = crashReports.Aggregate(msg,
                            //                     (current, report) => current + $"\n{report.CrashCauses}");
                            //         }
                            //         catch
                            //         {
                            //             msg = MainLang.NoCrashInfo;
                            //         }
                            //
                            //         task.FinishWithError();
                            //         await ShowDialogAsync(MainLang.MineratCrashed, msg,
                            //             b_primary: MainLang.Ok);
                            //         task.FinishWithError();
                            //     }
                            // }
                        });
                    };

                    process.OutputLogReceived += (_, arg) =>
                    {
                        var regex = new Regex(@"^\[[^\]]*\]\s*\[([^\]]*?)(\]|$)(\s*.*)");
                        var match = regex.Match(arg.Data.Source);
                        var regStr = match.Groups[1].Value + match.Groups[3].Value;
                        Dispatcher.UIThread.Invoke(
                            () =>
                            {
                                window.Append(arg.Data.Log, arg.Data.Time, (LogType)arg.Data.LogLevel,
                                    string.IsNullOrWhiteSpace(regStr) ? arg.Data.Source : regStr);
                            },
                            DispatcherPriority.ApplicationIdle);
                    };

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.AdvanceSubTask();
                        Toast($"{MainLang.LaunchFinish} - {p_id}");
                        task.AddOperateButton(new TaskEntryOperateButtonEntry(MainLang.DisplayLaunchArguments,
                            async () =>
                            {
                                var dialog = await ShowDialogAsync(MainLang.LaunchArguments,
                                    string.Join(" \n", process.ArgumentList), b_cancel: MainLang.Ok,
                                    b_primary: MainLang.Copy);
                                if (dialog != ContentDialogResult.Primary) return;
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
                                process.Process.Kill(true);
                                task.CancelWithSuccess();
                                await cts.CancelAsync();
                            }
                            catch
                            {
                            }
                        }));
                        task.AddOperateButton(new TaskEntryOperateButtonEntry("显示Minecraft日志", () =>
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
                                process.Process.Kill(true);
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
                                    if (TopLevel.GetTopLevel(YMCL.App.UiRoot) is Window window2)
                                    {
                                        window2.WindowState = WindowState.Minimized;
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

        await Task.Delay(20);
        return true;
    }
}