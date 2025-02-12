using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Downloader;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Installer.Modpack;
using MinecraftLaunch.Components.Parser;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Installer.ModPack;

public class CurseForge
{
    public static async Task<bool> Install(string path, CurseforgeModpackInstallEntry modpackEntry, string id)
    {
        var isSuccess = false;
        var task = new TaskEntry($"{MainLang.Install}: {id}", state: TaskState.Running);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        task.UpdateAction(() =>
        {
            cts.Cancel();
            task.CancelWaitFinish();
        });
        var mcPath = Data.Setting.MinecraftFolder.Path;
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
            var installEntrys =
                await CurseforgeModpackInstaller.ParseModLoaderEntryByManifestAsync(modpackEntry, cancellationToken)
                    .ToListAsync(cancellationToken: cancellationToken);
            if (installEntrys == null)
            {
                Toast($"{MainLang.Unrecognized}: {Path.GetFileName(path)}");
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
                        forgeTask, task, cancellationToken);
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
            
            var cfModpackInstaller = CurseforgeModpackInstaller.Create(mcPath, path, modpackEntry,
                new MinecraftParser(mcPath).GetMinecraft(id));
            task.Model.BottomLeftInfo = string.Empty;
            cfModpackInstaller.ProgressChanged += (_, x) =>
            {
                task.UpdateValue(x.Progress * 100);
                task.Model.TopRightInfo =
                    x.IsStepSupportSpeed ? FileDownloader.GetSpeedText(x.Speed) : string.Empty;
                installTask.Name = $"{MainLang.Install}: {id} (Installing ModPack)";
                installTask.FinishedTask = x.FinishedStepTaskCount;
                installTask.TotalTask = x.TotalStepTaskCount;
            };
            await cfModpackInstaller.InstallAsync(cancellationToken);

            isSuccess = true;
        }
        catch (Exception e)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                task.CancelFinish();
                task.Cancel();
                Directory.Delete(mcPath, true);
            }
            else
            {
                ShowShortException($"{MainLang.InstallFail}: {id}", e);
            }

            isSuccess = false;
        }


        if (cancellationToken.IsCancellationRequested) return isSuccess;
        if (isSuccess)
        {
            Toast($"{MainLang.InstallFinish}: {id}");
            task.FinishWithSuccess();
        }
        else
        {
            Toast($"{MainLang.InstallFail}: {id}");
            task.FinishWithError();
        }

        if (!isSuccess) return isSuccess;
        foreach (var modelSubTask in task.Model.SubTasks)
        {
            modelSubTask.Finish();
        }

        return isSuccess;
    }
}