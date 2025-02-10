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

public class Composite
{
    public static async Task<bool> Install(VersionManifestEntry versionManifestEntry, ForgeInstallEntry forgeEntry,
        OptifineInstallEntry optifineEntry,
        string customId, string mcPath, SubTask forgeTask, SubTask optifineTask,
        TaskEntry task, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        forgeTask.State = TaskState.Running;
        await Task.Run(async () =>
        {
            try
            {
                var installer = CompositeInstaller.Create([versionManifestEntry, forgeEntry, optifineEntry],
                    mcPath, Data.JavaRuntimes.FirstOrDefault(x => x.MajorVersion != 0).JavaPath, customId);

                installer.ProgressChanged += (_, x) =>
                {
                    var prStep = x.PrimaryStepName is InstallStep.Undefined ? "" : $" {x.PrimaryStepName}";
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.UpdateValue(x.Progress * 100);
                        task.Model.TopRightInfo =
                            x.IsStepSupportSpeed ? FileDownloader.GetSpeedText(x.Speed) : string.Empty;
                        task.Model.BottomLeftInfo = x.StepName + prStep;

                        if (!x.IsStepSupportSpeed) return;
                        forgeTask.FinishedTask = x.FinishedStepTaskCount;
                        forgeTask.TotalTask = x.TotalStepTaskCount;
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
        return isSuccess;
    }
}