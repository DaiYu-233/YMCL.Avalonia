using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Models.Install;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using Setting = YMCL.Public.Enum.Setting;

namespace YMCL.Public.Module.Mc.Installer.InstallJavaClientByMinecraftLauncher;

public class Dispatcher
{
    public static async Task<bool> Install(string versionId, string? customId = null,
        ForgeInstallEntry? forgeInstallEntry = null, FabricBuildEntry? fabricBuildEntry = null,
        QuiltBuildEntry? quiltBuildEntry = null, OptiFineInstallEntity? optiFineInstallEntity = null,
        TaskEntry? p_task = null, bool closeTaskWhenFinish = true)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        var regex = new Regex(@"[\\/:*?""<>|]");
        var matches = regex.Matches(customId ?? versionId);
        if (matches.Count > 0)
        {
            var str = string.Empty;
            foreach (Match match in matches) str += match.Value;
            Toast($"{MainLang.IncludeSpecialWord}: {str}", NotificationType.Error);
            return false;
        }

        var setting = Const.Data.Setting;
        MirrorDownloadManager.IsUseMirrorDownloadSource = setting.DownloadSource == Setting.DownloadSource.BmclApi;

        if (Directory.Exists(Path.Combine(setting.MinecraftFolder.Path, "versions", customId ?? versionId)))
        {
            Toast($"{MainLang.FolderAlreadyExists}: {customId ?? versionId}", NotificationType.Error);
            return false;
        }

        var task = p_task ?? new TaskEntry($"{MainLang.Install}: {customId}(Minecraft {versionId})",
            state: TaskState.Running);
        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        SubTask[] subTasks =
        [
            new(MainLang.CheckVersionResource, 1),
            new($"{MainLang.DownloadResource}(Vanllia)", 1)
        ];
        task.AddSubTaskRange(subTasks);
        task.UpdateAction(() =>
        {
            task.CancelWaitFinish();
            cts.Cancel();
        });

        var forgeTask = new SubTask($"{MainLang.Install}: Forge", 1);
        var optiFineTask = new SubTask($"{MainLang.Install}: OptiFine", 1);
        var fabricTask = new SubTask($"{MainLang.Install}: Fabric", 1);
        var quiltTask = new SubTask($"{MainLang.Install}: Quilt", 1);

        if (forgeInstallEntry != null)
            task.AddSubTask(forgeTask);

        if (optiFineInstallEntity != null)
            task.AddSubTask(optiFineTask);

        if (fabricBuildEntry != null)
            task.AddSubTask(fabricTask);

        if (quiltBuildEntry != null)
            task.AddSubTask(quiltTask);

        var vanllia = await Vanllia.Install(versionId, task, subTasks[0], subTasks[1], token);
        if (!vanllia)
        {
            task.FinishWithError();
            return false;
        }

        subTasks[1].State = TaskState.Finished;
        
        if (forgeInstallEntry != null)
        {
            var forge = await Forge.Install(versionId, customId ?? versionId, forgeInstallEntry, forgeTask, task, token);
            if (!forge)
            {
                task.FinishWithError();
                return false;
            }

            forgeTask.State = TaskState.Finished;
        }

        if (closeTaskWhenFinish)
        {
            task.FinishWithSuccess();
        }

        return true;
    }
}