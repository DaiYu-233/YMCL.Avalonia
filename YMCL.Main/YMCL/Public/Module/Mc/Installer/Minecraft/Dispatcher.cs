using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MinecraftLaunch;
using MinecraftLaunch.Base.Models.Network;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Installer.Minecraft;

public class Dispatcher
{
    public static async Task<bool> Install(VersionManifestEntry versionManifestEntry, string? customId = null,
        ForgeInstallEntry? forgeInstallEntry = null, ForgeInstallEntry? neoForgeInstallEntry = null,
        FabricInstallEntry? fabricInstallEntry = null,
        QuiltInstallEntry? quiltBuildEntry = null, OptifineInstallEntry? optiFineInstallEntity = null,
        TaskEntry? p_task = null, bool closeTaskWhenFinish = true)
    {
        if (string.IsNullOrWhiteSpace(customId ?? versionManifestEntry.Id))
        {
            Notice($"{MainLang.VersionIdCannotBeEmpty}", NotificationType.Error);
            return false;
        }
        
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var setting = Const.Data.SettingEntry;
        
        var regex = new Regex(@"[\\/:*?""<>|]");
        var matches = regex.Matches(customId ?? versionManifestEntry.Id);
        if (matches.Count > 0)
        {
            var str = string.Empty;
            foreach (Match match in matches) str += match.Value;
            Notice($"{MainLang.IncludeSpecialWord}: {str}", NotificationType.Error);
            return false;
        }

        if (optiFineInstallEntity != null || quiltBuildEntry != null || fabricInstallEntry != null ||
            forgeInstallEntry != null)
        {
            if (Directory.Exists(Path.Combine(setting.MinecraftFolder.Path, "versions",
                    customId ?? versionManifestEntry.Id)))
            {
                Notice($"{MainLang.FolderAlreadyExists}: {customId ?? versionManifestEntry.Id}", NotificationType.Error);
                return false;
            }
        }

        var mcPath = Data.SettingEntry.MinecraftFolder.Path;

        var task = p_task ?? new TaskEntry($"{MainLang.Install}: {customId} (Minecraft {versionManifestEntry.Id})",
            state: TaskState.Running);
        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        SubTask[] subTasks =
        [
            new(MainLang.CheckVersionResource),
            new($"{MainLang.Install}: Vanilla {versionManifestEntry.Id}")
        ];
        task.AddSubTaskRange(subTasks);
        task.UpdateAction(() =>
        {
            task.CancelWaitFinish();
            cts.Cancel();
        });

        if (versionManifestEntry == null)
        {
            task.FinishWithError();
            return false;
        }

        var forgeTask = new SubTask($"{MainLang.Install}: Forge");
        var neoForgeTask = new SubTask($"{MainLang.Install}: NeoForge");
        var optiFineTask = new SubTask($"{MainLang.Install}: OptiFine");
        var fabricTask = new SubTask($"{MainLang.Install}: Fabric");
        var quiltTask = new SubTask($"{MainLang.Install}: Quilt");

        if (forgeInstallEntry != null)
            task.AddSubTask(forgeTask);

        if (neoForgeInstallEntry != null)
            task.AddSubTask(neoForgeTask);

        if (optiFineInstallEntity != null)
            task.AddSubTask(optiFineTask);

        if (fabricInstallEntry != null)
            task.AddSubTask(fabricTask);

        if (quiltBuildEntry != null)
            task.AddSubTask(quiltTask);

        Console.WriteLine($"MaxThread: {DownloadMirrorManager.MaxThread}");

        if (optiFineInstallEntity != null && versionManifestEntry != null && forgeInstallEntry != null)
        {
            var composite = await CompositeForgeAndOptiFine.Install(versionManifestEntry, forgeInstallEntry, optiFineInstallEntity,
                customId!, mcPath, subTasks[0], subTasks[1],
                forgeTask, optiFineTask, task, cancellationToken);

            if (!composite)
            {
                task.FinishWithError();
                return false;
            }

            if (!closeTaskWhenFinish || cancellationToken.IsCancellationRequested) return true;
            task.FinishWithSuccess();
            Notice($"{MainLang.InstallFinish} - {customId ?? versionManifestEntry.Id}", NotificationType.Success);
            if (Data.SettingEntry.EnableIndependencyWindowNotification)
            {
                NoticeWindow(MainLang.InstallFinish, customId ?? versionManifestEntry.Id);
            }

            return true;
        }

        var vanilla = await Vanilla.Install(versionManifestEntry!, mcPath, task, subTasks[0], subTasks[1],
            cancellationToken);
        if (!vanilla)
        {
            task.FinishWithError();
            return false;
        }

        subTasks[0].Finish();
        subTasks[1].Finish();
        
        if (forgeInstallEntry != null)
        {
            var forge = await Forge.Install(forgeInstallEntry, customId!, mcPath, forgeTask, task,
                cancellationToken);
            if (!forge)
            {
                task.FinishWithError();
                return false;
            }

            forgeTask.Finish();
        }

        if (optiFineInstallEntity != null)
        {
            var optifine = await OptiFine.Install(optiFineInstallEntity, customId!, mcPath, optiFineTask, task,
                cancellationToken);
            if (!optifine)
            {
                task.FinishWithError();
                return false;
            }

            optiFineTask.Finish();
        }


        if (neoForgeInstallEntry != null)
        {
            var neoForge = await Forge.Install(neoForgeInstallEntry, customId!, mcPath, neoForgeTask, task,
                cancellationToken);
            if (!neoForge)
            {
                task.FinishWithError();
                return false;
            }

            neoForgeTask.Finish();
        }

        if (fabricInstallEntry != null)
        {
            var fabric = await Fabric.Install(fabricInstallEntry, customId!, mcPath, fabricTask, task,
                cancellationToken);
            if (!fabric)
            {
                task.FinishWithError();
                return false;
            }

            fabricTask.Finish();
        }

        if (quiltBuildEntry != null)
        {
            var quilt = await Quilt.Install(quiltBuildEntry, customId!, mcPath, quiltTask, task,
                cancellationToken);
            if (!quilt)
            {
                task.FinishWithError();
                return false;
            }

            quiltTask.Finish();
        }

        if (!closeTaskWhenFinish || cancellationToken.IsCancellationRequested) return true;
        task.FinishWithSuccess();
        Notice($"{MainLang.InstallFinish} - {customId ?? versionManifestEntry.Id}", NotificationType.Success);
        if (Data.SettingEntry.EnableIndependencyWindowNotification)
        {
            NoticeWindow(MainLang.InstallFinish, customId ?? versionManifestEntry.Id);
        }

        return true;
    }
}