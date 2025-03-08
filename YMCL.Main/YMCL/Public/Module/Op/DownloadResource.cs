using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO.Network;
using File = CurseForge.APIClient.Models.Files.File;

namespace YMCL.Public.Module.Op;

public class DownloadResource
{
    public static void SaveCurseForge(ResourceType type, File? entry)
    {
        if (entry is null) return;
        switch (type)
        {
            case ResourceType.ModPack:
                _ = ModPack(entry.DownloadUrl, ".zip");
                break;
            case ResourceType.Mod:
            case ResourceType.ShaderPack:
            case ResourceType.ResourcePack:
            case ResourceType.DataPack:
            case ResourceType.Map:
                _ = DownloadFile(entry.DownloadUrl, entry.FileName, type);
                break;
        }
    }

    public static void SaveModrinth(ResourceType type, ModrinthFile? entry)
    {
        if (entry is null) return;
        switch (type)
        {
            case ResourceType.ModPack:
                _ = ModPack(entry.Url, ".mrpack");
                break;
            case ResourceType.Mod:
            case ResourceType.ShaderPack:
            case ResourceType.ResourcePack:
            case ResourceType.DataPack:
            case ResourceType.Map:
                _ = DownloadFile(entry.Url, entry.FileName, type);
                break;
        }
    }

    public static async Task DownloadFile(string url, string fileName, ResourceType type)
    {
        string folderPath = null;
        if (string.IsNullOrWhiteSpace(url))
        {
            Notice($"{MainLang.DownloadUrlParserFail}", NotificationType.Error);
            return;
        }

        if (Data.Setting.SelectedMinecraftId != "bedrock")
        {
            folderPath = type switch
            {
                ResourceType.Mod => Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.ModsFolder),
                ResourceType.Map => Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.SavesFolder),
                ResourceType.DataPack => Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.SavesFolder),
                ResourceType.ShaderPack => Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.ShaderPacksFolder),
                ResourceType.ResourcePack => Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.ResourcePacksFolder),
                _ => null
            };
        }

        var suggest = !string.IsNullOrWhiteSpace(folderPath)
            ? await TopLevel.GetTopLevel(YMCL.App.UiRoot).StorageProvider.TryGetFolderFromPathAsync
                (new Uri(folderPath))
            : null;
        var path = (await TopLevel.GetTopLevel(YMCL.App.UiRoot).StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = MainLang.Download,
                SuggestedFileName = fileName,
                SuggestedStartLocation = suggest,
            }))?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        var name = Path.GetFileName(path);
        var task = new TaskEntry($"{MainLang.Download} - {name}",
            [new SubTask($"{MainLang.Download} - {name}") { State = TaskState.Running }]);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        task.UpdateAction(() =>
        {
            cts.Cancel();
            task.CancelWaitFinish();
        });
        Notice($"{MainLang.BeginDownload}: {name}");
        if (!await DownloadFileWithProgress.Download(url, path, task,
                cancellationToken))
        {
            Notice($"{MainLang.DownloadFail}: {name}");
            task.FinishWithError();
            await cts.CancelAsync();
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            Notice($"{MainLang.Canceled}: {name}");
            task.Cancel();
            return;
        }

        task.Destroy();
        Notice($"{MainLang.DownloadFinish}: {name}", NotificationType.Success);
    }

    public static async Task ModPack(string url, string extension)
    {
        var cr = await ShowDialogAsync(MainLang.Install, $"{MainLang.InstallModPack}: {url}", b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok);
        if (cr != ContentDialogResult.Primary) return;
        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        var task = new TaskEntry($"{MainLang.Install}: {MainLang.ModPack}", state: TaskState.Running);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        task.UpdateAction(() =>
        {
            cts.Cancel();
            task.CancelWaitFinish();
        });
        var subTask = new SubTask($"{MainLang.Download}: {url}") { State = TaskState.Running };
        task.AddSubTask(subTask);
        var path = Path.Combine(ConfigPath.TempFolderPath, $"{DateTime.Now:yyyy-MM-dd-hh-mm-ss}{extension}");
        subTask.State = TaskState.Running;
        if (!await DownloadFileWithProgress.Download(url, path, task,
                cancellationToken))
        {
            task.FinishWithError();
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            task.Cancel();
            return;
        }

        subTask.Finish();
        if (extension == ".zip")
        {
            _ = Mc.Importer.zip.ModPack.Import(path, task);
        }
        else if (extension == ".mrpack")
        {
            _ = Mc.Importer.mrpack.ModPack.Import(path, task);
        }
    }
}