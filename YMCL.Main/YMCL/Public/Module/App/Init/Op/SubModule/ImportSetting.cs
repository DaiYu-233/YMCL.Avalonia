using System.IO;
using System.Threading;
using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO.Network;

namespace YMCL.Public.Module.App.Init.Op.SubModule;

public class ImportSetting
{
    public static async Task Invoke(string text)
    {
        var url = text.Split(' ')[1];
        var task = new TaskEntry($"{MainLang.Download} - {url}",
            [new SubTask($"{MainLang.Download} - {url}") { State = TaskState.Running }]);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        task.UpdateAction(() =>
        {
            cts.Cancel();
            task.CancelWaitFinish();
        });
        Notice($"{MainLang.BeginDownload}: {url}");
        var path = Path.Combine(ConfigPath.TempFolderPath, $"{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.DaiYu");
        if (!await DownloadFileWithProgress.Download(url, path, task,
                cancellationToken))
        {
            task.FinishWithError();
            Notice($"{MainLang.DownloadFail}: {url}");
            return;
        }
        var info = Public.Module.App.Setting.Import(await File.ReadAllTextAsync(path, cancellationToken));
        if (!info.success || info.data == null)
        {
            Notice(MainLang.ImportFailed, NotificationType.Error);
            return;
        }


        var data = JsonConvert.DeserializeObject<ExchangeSettingEntry.Data>(JsonConvert.SerializeObject(info.data));
        if (data.UiSettings != null)
        {
            data.UiSettings.WindowBackGroundImgData = " ( ...... ) ";
        }

        if (data.AccountSettings != null)
        {
            foreach (var a in data.AccountSettings)
            {
                a.Skin = " ( ...... ) ";
                a.Data = " ( ...... ) ";
            }
        }
            
        var cr1 = await ShowDialogAsync(MainLang.Import,
            JsonConvert.SerializeObject(data, Formatting.Indented), b_primary: MainLang.Import,
            b_cancel: MainLang.Cancel);
        if (cr1 != ContentDialogResult.Primary) return;
        Public.Module.App.Setting.Replace(Data.SettingEntry, info.data);
    }
}