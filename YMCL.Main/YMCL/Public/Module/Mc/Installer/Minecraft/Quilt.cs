using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Installer.Minecraft;

public class Quilt
{
    public static async Task<bool> Install(QuiltInstallEntry entry, string customId, string mcPath, SubTask subTask,
        TaskEntry task, CancellationToken cancellationToken)
    {
        var isSuccess = false;
        subTask.State = TaskState.Running;
        await Task.Run(async () =>
        {
            try
            {
                var installer = QuiltInstaller.Create(mcPath, entry, customId);
                installer.ProgressChanged += (_, x) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        task.UpdateValue(x.Progress * 100);
                        task.Model.TopRightInfo =
                            x.IsStepSupportSpeed ? FileDownloader.GetSpeedText(x.Speed) : string.Empty;
                        task.Model.BottomLeftInfo = x.StepName.ToString();

                        if (!x.IsStepSupportSpeed) return;
                        subTask.FinishedTask = x.FinishedStepTaskCount;
                        subTask.TotalTask = x.TotalStepTaskCount;
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
                        ShowShortException($"{MainLang.InstallFail}: Quilt - {customId}", ex);
                    });
                    task.FinishWithError();
                }
            }
        }, cancellationToken);
        if (isSuccess)
        {
            subTask.Finish();
        }
        return isSuccess;
    }
}