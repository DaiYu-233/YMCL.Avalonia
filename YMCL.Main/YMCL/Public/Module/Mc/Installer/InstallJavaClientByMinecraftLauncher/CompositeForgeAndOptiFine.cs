using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Base.Enums;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Installer.InstallJavaClientByMinecraftLauncher;

public class CompositeForgeAndOptiFine
{
    public static async Task<bool> Install(VersionManifestEntry versionManifestEntry, ForgeInstallEntry forgeEntry,
        OptifineInstallEntry optifineEntry,
        string customId, string mcPath, SubTask checkTask, SubTask vanillaTask, SubTask forgeTask, SubTask optifineTask,
        TaskEntry task, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        var fCheck = false;
        var fVanilla = false;
        var fForge = false;
        var fOptifine = false;
        checkTask.State = TaskState.Running;
        vanillaTask.State = TaskState.Waiting;
        forgeTask.State = TaskState.Waiting;
        optifineTask.State = TaskState.Waiting;
        checkTask.FinishedTask = 0;
        vanillaTask.FinishedTask = 0;
        forgeTask.FinishedTask = 0;
        optifineTask.FinishedTask = 0;

        await Task.Run(async () =>
        {
            try
            {
                var installer = CompositeInstaller.Create([versionManifestEntry, forgeEntry, optifineEntry],
                    mcPath, Data.JavaRuntimes.FirstOrDefault(x => x.MajorVersion != 0).JavaPath, customId);

                installer.ProgressChanged += (_, x) =>
                {
                    var info = x.PrimaryStepName is InstallStep.Undefined
                        ? $"{x.StepName}"
                        : $"{x.PrimaryStepName} {x.StepName}";
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.UpdateValue(x.Progress * 100);
                        task.Model.TopRightInfo =
                            x.IsStepSupportSpeed ? FileDownloader.GetSpeedText(x.Speed) : string.Empty;
                        task.Model.BottomLeftInfo = info;

                        if (x.PrimaryStepName == InstallStep.InstallPrimaryModLoader)
                        {
                            checkTask.Finish();
                            vanillaTask.Finish();
                            fVanilla = true;
                            fCheck = true;
                            forgeTask.State = TaskState.Running;
                        }

                        if (x.PrimaryStepName == InstallStep.InstallSecondaryModLoader)
                        {
                            checkTask.Finish();
                            vanillaTask.Finish();
                            forgeTask.Finish();
                            fVanilla = true;
                            fCheck = true;
                            fForge = true;
                            optifineTask.State = TaskState.Running;
                        }

                        if (!fCheck)
                        {
                            if (x is { PrimaryStepName: InstallStep.InstallVanilla, IsStepSupportSpeed: true })
                            {
                                checkTask.Finish();
                                fCheck = true;
                                vanillaTask.State = TaskState.Running;
                            }
                            else return;
                        }

                        if (!fVanilla)
                        {
                            if (x.PrimaryStepName == InstallStep.InstallPrimaryModLoader)
                            {
                                vanillaTask.Finish();
                                fVanilla = true;
                                forgeTask.State = TaskState.Running;
                            }
                            else
                            {
                                if (!x.IsStepSupportSpeed) return;
                                vanillaTask.FinishedTask = x.FinishedStepTaskCount;
                                vanillaTask.TotalTask = x.TotalStepTaskCount;
                                return;
                            }
                        }

                        if (!fForge)
                        {
                            if (x.PrimaryStepName == InstallStep.InstallSecondaryModLoader)
                            {
                                forgeTask.Finish();
                                fForge = true;
                                optifineTask.State = TaskState.Running;
                            }
                            else
                            {
                                if (!x.IsStepSupportSpeed) return;
                                forgeTask.FinishedTask = x.FinishedStepTaskCount;
                                forgeTask.TotalTask = x.TotalStepTaskCount;
                                return;
                            }
                        }

                        if (!fOptifine)
                        {
                            if (x.PrimaryStepName == InstallStep.RanToCompletion)
                            {
                                optifineTask.Finish();
                                fOptifine = true;
                            }
                            else
                            {
                                if (!x.IsStepSupportSpeed) return;
                                optifineTask.FinishedTask = x.FinishedStepTaskCount;
                                optifineTask.TotalTask = x.TotalStepTaskCount;
                            }
                        }
                    });
                };

                await installer.InstallAsync(cancellationToken);
                isSuccess = true;
            }
            catch (OperationCanceledException)
            {
                task.CancelFinish();
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    task.CancelFinish();
                }
                else
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowShortException($"{MainLang.InstallFail}: Forge - {customId}", ex);
                    });
                    task.FinishWithError();
                }
            }
        }, cancellationToken);
        if (!isSuccess) return isSuccess;
        fVanilla = true;
        fCheck = true;
        fForge = true;
        fOptifine = true;
        checkTask.Finish();
        vanillaTask.Finish();
        forgeTask.Finish();
        optifineTask.Finish();

        return isSuccess;
    }
}