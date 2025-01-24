using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.IO.Network;

public class Update
{
    public static async Task<CheckUpdateInfo> CheckUpdateAsync()
    {
        try
        {
            const string resourceName = "YMCL.Public.Texts.DateTime.txt";
            var _assembly = Assembly.GetExecutingAssembly();
            var stream = _assembly.GetManifestResourceStream(resourceName);
            string version;
            using (var reader = new StreamReader(stream!))
            {
                var result = await reader.ReadToEndAsync();
                version = $"v{result.Trim()}";
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl));
            var apiVersion = (string)githubApiJson[0]["name"]!;
            return new CheckUpdateInfo()
            {
                Success = true,
                NewVersion = apiVersion,
                IsNeedUpdate = apiVersion != version,
                GithubUrl = (string)githubApiJson[0]["html_url"]!
            };
        }
        catch
        {
            return new CheckUpdateInfo { Success = false };
        }
    }

    public static async Task<bool> UpdateAppAsync()
    {
        App.UiRoot.Nav.SelectedItem = App.UiRoot.NavTask;
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var task = new TaskEntry(MainLang.CheckUpdate, 
        [
            new SubTask(MainLang.CheckUpdate,1),
            new SubTask(MainLang.GetUpdateUrl,1),
            new SubTask(MainLang.DownloadResource,1),
            new SubTask(MainLang.Update,1),
        ], TaskState.Running);
        task.UpdateAction(() =>
        {
            cts.Cancel();
        });
        try
        {
            var architecture = IO.Disk.Getter.GetCurrentPlatformAndArchitecture();
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl, token));
            var assets = (JArray)githubApiJson[0]["assets"];
            var url = string.Empty;
            var fileName = string.Empty;
            task.AdvanceSubTask();
            foreach (var jToken in assets)
            {
                var asset = (JObject)jToken;
                var name = (string)asset["name"];
                var browser_download_url = (string)asset["browser_download_url"];
                switch (name)
                {
                    case "YMCL.Desktop.linux.arm.AppImage" when architecture == "linux-arm":
                    case "YMCL.Desktop.linux.arm64.AppImage" when architecture == "linux-arm64":
                    case "YMCL.Desktop.linux.x64.AppImage" when architecture == "linux-x64":
                    case "YMCL.Desktop.osx.mac.x64.app.zip" when architecture == "osx-x64":
                    case "YMCL.Desktop.osx.mac.arm64.app.zip" when architecture == "osx-arm64":
                    case "YMCL.Desktop.win.x64.installer.exe"
                        when architecture == "win-x64" && Environment.OSVersion.Version.Major >= 10:
                    case "YMCL.Desktop.win.x86.installer.exe"
                        when architecture == "win-x86" && Environment.OSVersion.Version.Major >= 10:
                    case "YMCL.Desktop.win.arm64.installer.exe"
                        when architecture == "win-arm64" && Environment.OSVersion.Version.Major >= 10:
                    case "YMCL.Desktop.win7.x64.exe.zip"
                        when architecture == "win-x64" && Environment.OSVersion.Version.Major < 10:
                    case "YMCL.Desktop.win7.x86.exe.zip"
                        when architecture == "win-x86" && Environment.OSVersion.Version.Major < 10:
                    case "YMCL.Desktop.win7.arm64.exe.zip"
                        when architecture == "win-arm64" && Environment.OSVersion.Version.Major < 10:
                        url = browser_download_url;
                        fileName = name;
                        break;
                }
            }

            task.AdvanceSubTask();
            if (url == null)
            {
                task.FinishWithError();
                return false;
            }

            IO.Disk.Setter.ClearFolder(ConfigPath.UpdateFolderPath);

            var setting = Const.Data.Setting;
            var trueUrl = url;
            if (setting.EnableCustomUpdateUrl)
            {
                trueUrl = setting.CustomUpdateUrl.Replace("{%url%}", url);
            }

            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (_, _, _, _) => true;
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
                using (var response =
                       await client.GetAsync(trueUrl, HttpCompletionOption.ResponseHeadersRead, token))
                {
                    response.EnsureSuccessStatusCode();

                    await using (var downloadStream = await response.Content.ReadAsStreamAsync(token))
                    {
                        await using (var fileStream = new FileStream(
                                         Path.Combine(ConfigPath.UpdateFolderPath, fileName), FileMode.Create,
                                         FileAccess.Write))
                        {
                            var buffer = new byte[8192];
                            int bytesRead;
                            long totalBytesRead = 0;
                            var totalBytes = response.Content.Headers.ContentLength ?? -1;

                            while ((bytesRead =
                                       await downloadStream.ReadAsync(buffer, token)) > 0)
                            {
                                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                                totalBytesRead += bytesRead;

                                if (totalBytes <= 0) continue;
                                var progress = (double)totalBytesRead / totalBytes * 100;
                                task.UpdateValue(progress);
                            }
                        }
                    }
                }

                task.AdvanceSubTask();
                if (architecture is "win-x86" or "win-x64" or "win-arm64" &&
                    Environment.OSVersion.Version.Major >= 10)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Path.Combine(ConfigPath.UpdateFolderPath, fileName)
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                }
                else
                {
                    var dialog = ShowDialogAsync(MainLang.DownloadFinish,
                        MainLang.CurrectSystemNoSupportAutoUpdateTip + "\n" +
                        Path.Combine(ConfigPath.UpdateFolderPath, fileName), b_primary: MainLang.OpenFolder,
                        b_cancel: MainLang.Cancel);
                    if (dialog.Result == ContentDialogResult.Primary)
                    {
                        var launcher = TopLevel.GetTopLevel(App.UiRoot).Launcher;
                        await launcher.LaunchDirectoryInfoAsync(
                            new DirectoryInfo(ConfigPath.TempFolderPath));
                        await Task.Delay(1000);
                        var clipboard = TopLevel.GetTopLevel(App.UiRoot)?.Clipboard;
                        await clipboard.SetTextAsync(Path.Combine(ConfigPath.UpdateFolderPath, fileName));
                        Toast(MainLang.AlreadyCopyToClipBoard +
                              $" : {Path.Combine(ConfigPath.UpdateFolderPath, fileName)}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                task.Cancel();
                Toast(MainLang.CanceledUpdate);
                return true;
            }
            catch (Exception ex)
            {
                ShowShortException(MainLang.UpdateFail, ex);
            }

            task.FinishWithError();
            return false;
        }
        catch (OperationCanceledException)
        {
            task.Cancel();
            Toast(MainLang.CanceledUpdate);
            return true;
        }
        catch
        {
            task.FinishWithError();
            return false;
        }
    }
}