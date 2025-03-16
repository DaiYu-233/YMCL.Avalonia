using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using MinecraftLaunch.Base.Interfaces;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Installer.Modpack;
using MinecraftLaunch.Components.Parser;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Installer.ModPack;

public class Modrinth
{
    public static async Task<bool> Install(string path, ModrinthModpackInstallEntry modpackEntry, string id,
        TaskEntry? p_task = null)
    {
        bool isSuccess;
        var task = p_task ?? new TaskEntry($"{MainLang.Install}: {id}", state: TaskState.Running);
        task.Rename($"{MainLang.Install}: {id}");
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var mcPath = Data.SettingEntry.MinecraftFolder.Path;
        task.UpdateAction(() =>
        {
            cts.Cancel();
            task.CancelWaitFinish();
            if (Directory.Exists(Path.Combine(mcPath, "versions", id)))
                Directory.Delete(Path.Combine(mcPath, "versions", id), true);
        });
        var installTask = new SubTask($"{MainLang.InstallModPack}: {id}");
        var prepareTask = new SubTask($"{MainLang.PrepareInstall}");
        var vanillaTask = new SubTask($"{MainLang.Install}: Vanilla {modpackEntry.McVersion}");
        var checkTask = new SubTask(MainLang.CheckVersionResource);
        var forgeTask = new SubTask($"{MainLang.Install}: Forge");
        var neoForgeTask = new SubTask($"{MainLang.Install}: NeoForge");
        var optiFineTask = new SubTask($"{MainLang.Install}: OptiFine");
        var fabricTask = new SubTask($"{MainLang.Install}: Fabric");
        var quiltTask = new SubTask($"{MainLang.Install}: Quilt");

        task.AddSubTask(prepareTask);
        prepareTask.State = TaskState.Running;

        try
        {
            List<IInstallEntry> installEntrys =
            [
                await ModrinthModpackInstaller.ParseModLoaderEntryAsync(modpackEntry, cancellationToken)
            ];
            if (installEntrys == null)
            {
                Notice($"{MainLang.Unrecognized}: {Path.GetFileName(path)}", NotificationType.Warning);
                return false;
            }

            installEntrys.Insert(0,
                await VanillaInstaller.EnumerableMinecraftAsync(cancellationToken)
                    .FirstAsync(x => x.Id == modpackEntry.McVersion, cancellationToken: cancellationToken));

            installEntrys.ForEach(entry =>
            {
                if (entry is VersionManifestEntry)
                {
                    task.AddSubTask(checkTask);
                    task.AddSubTask(vanillaTask);
                }
                else if (entry is ForgeInstallEntry forgeInstallEntry)
                {
                    task.AddSubTask(forgeInstallEntry.IsNeoforge ? neoForgeTask : forgeTask);
                }
                else if (entry is OptifineInstallEntry)
                {
                    task.AddSubTask(optiFineTask);
                }
                else if (entry is QuiltInstallEntry)
                {
                    task.AddSubTask(quiltTask);
                }
                else if (entry is FabricInstallEntry)
                {
                    task.AddSubTask(fabricTask);
                }
            });

            task.AddSubTask(installTask);
            prepareTask.Finish();

            foreach (var entry in installEntrys)
            {
                if (entry is VersionManifestEntry versionManifestEntry)
                {
                    await Installer.Minecraft.Vanilla.Install(versionManifestEntry, mcPath, task, checkTask,
                        vanillaTask, cancellationToken: cancellationToken);
                }
                else if (entry is ForgeInstallEntry forgeInstallEntry)
                {
                    await Installer.Minecraft.Forge.Install(forgeInstallEntry, id, mcPath,
                        forgeInstallEntry.IsNeoforge ? neoForgeTask : forgeTask, task, cancellationToken);
                }
                else if (entry is OptifineInstallEntry optifineInstallEntry)
                {
                    await Installer.Minecraft.OptiFine.Install(optifineInstallEntry, id, mcPath,
                        optiFineTask, task, cancellationToken);
                }
                else if (entry is QuiltInstallEntry quiltInstallEntry)
                {
                    await Installer.Minecraft.Quilt.Install(quiltInstallEntry, id, mcPath,
                        quiltTask, task, cancellationToken);
                }
                else if (entry is FabricInstallEntry fabricInstallEntry)
                {
                    await Installer.Minecraft.Fabric.Install(fabricInstallEntry, id, mcPath,
                        fabricTask, task, cancellationToken);
                }
            }

            installTask.State = TaskState.Running;

            var mrModpackInstaller = ModrinthModpackInstaller.Create(mcPath, path, modpackEntry,
                new MinecraftParser(mcPath).GetMinecraft(id));
            task.Model.BottomLeftInfo = string.Empty;
            mrModpackInstaller.ProgressChanged += (_, x) =>
            {
                task.UpdateValue(x.Progress * 100);
                task.Model.TopRightInfo =
                    x.IsStepSupportSpeed ? FileDownloader.GetSpeedText(x.Speed) : string.Empty;
                task.Model.BottomLeftInfo = x.StepName.ToString();
                installTask.Name = $"{MainLang.Install}: {id} ({x.StepName})";
                installTask.FinishedTask = x.FinishedStepTaskCount;
                installTask.TotalTask = x.TotalStepTaskCount;
            };
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Run(async () =>
            {
                try
                {
                    await mrModpackInstaller.InstallAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            task.CancelFinish();
                            task.Cancel();
                            if (Directory.Exists(Path.Combine(mcPath, "versions", id)))
                                Directory.Delete(Path.Combine(mcPath, "versions", id), true);
                        }
                        else
                        {
                            ShowShortException($"{MainLang.InstallFail}: {id}", e);
                            if (Data.SettingEntry.EnableIndependencyWindowNotification)
                            {
                                NoticeWindow(MainLang.InstallFail, id);
                            }
                        }
                    });
                    isSuccess = false;
                }
            });
            cancellationToken.ThrowIfCancellationRequested();
            isSuccess = true;
        }
        catch (Exception e)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                task.CancelFinish();
                task.Cancel();
                if (Directory.Exists(Path.Combine(mcPath, "versions", id)))
                    Directory.Delete(Path.Combine(mcPath, "versions", id), true);
            }
            else
            {
                ShowShortException($"{MainLang.InstallFail}: {id}", e);
            }

            isSuccess = false;
        }


        if (cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (Directory.Exists(Path.Combine(mcPath, "versions", id)))
                    Directory.Delete(Path.Combine(mcPath, "versions", id), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
        if (isSuccess)
        {
            Notice($"{MainLang.InstallFinish}: {id}", NotificationType.Success);
            if (Data.SettingEntry.EnableIndependencyWindowNotification)
            {
                NoticeWindow(MainLang.InstallFinish, id);
            }
            task.FinishWithSuccess();
        }
        else
        {
            Notice($"{MainLang.InstallFail}: {id}", NotificationType.Success);
            if (Data.SettingEntry.EnableIndependencyWindowNotification)
            {
                NoticeWindow(MainLang.InstallFail, id);
            }
            task.FinishWithError();
        }
        
        if (!isSuccess)
        {
            try
            {
                if (Directory.Exists(Path.Combine(mcPath, "versions", id)))
                    Directory.Delete(Path.Combine(mcPath, "versions", id), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return isSuccess;
        }
        foreach (var modelSubTask in task.Model.SubTasks)
        {
            modelSubTask.Finish();
        }

        return isSuccess;
    }
}