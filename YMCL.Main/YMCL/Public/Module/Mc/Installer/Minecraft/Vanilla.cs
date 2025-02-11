using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Installer.Minecraft;

public class Vanilla
{
    public static async Task<bool> Install(VersionManifestEntry entry, string mcPath, TaskEntry task,
        SubTask checkSubTask = null,
        SubTask downloadSubTask = null, CancellationToken cancellationToken = default)
    {
        var isSuccess = false;
        var installer = VanillaInstaller.Create(mcPath, entry);
        checkSubTask.State = TaskState.Running;
        downloadSubTask.State = TaskState.Waiting;
        await Task.Run(async () =>
        {
            try
            {
                var checkFinished = false;
                installer.ProgressChanged += (_, x) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.UpdateValue(x.Progress * 100);
                        task.Model.TopRightInfo =
                            x.IsStepSupportSpeed ? FileDownloader.GetSpeedText(x.Speed) : string.Empty;
                        task.Model.BottomLeftInfo = x.StepName.ToString();

                        if (x.IsStepSupportSpeed && !checkFinished)
                        {
                            checkSubTask.Finish();
                            downloadSubTask.State = TaskState.Running;
                            checkFinished = true;
                        }

                        if (!x.IsStepSupportSpeed) return;
                        downloadSubTask.FinishedTask = x.FinishedStepTaskCount;
                        downloadSubTask.TotalTask = x.TotalStepTaskCount;
                    });
                };

                await installer.InstallAsync(cancellationToken);
                isSuccess = true;
            }
            catch (OperationCanceledException)
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(task.CancelFinish);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (cancellationToken.IsCancellationRequested)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(task.CancelFinish);
                }
                else
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ShowShortException($"{MainLang.InstallFail}: Vanilla - {entry.Id}", ex);
                        task.FinishWithError();
                    });
                }
            }
        }, cancellationToken);
        if (!isSuccess) return isSuccess;
        checkSubTask.Finish();
        downloadSubTask.Finish();
        return isSuccess;
    }
}