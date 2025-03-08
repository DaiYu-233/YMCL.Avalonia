using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Operate;
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
        try
        {
            var architecture = IO.Disk.Getter.GetCurrentPlatformAndArchitecture();
            if (string.IsNullOrWhiteSpace(architecture))
                return false;
            if (architecture is "win-x64" or "win-arm64" or "win-x86" && Environment.OSVersion.Version.Major >= 10)
            {
                return await UpdateByAutoInstaller(architecture);
            }
            else
            {
                return await UpdateByReplaceFile(architecture);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public static async Task<bool> UpdateByReplaceFile(string architecture)
    {
        var cr = await ShowDialogAsync(MainLang.Update, MainLang.CurrectSystemNoSupportAutoUpdateTip,
            b_primary: MainLang.SaveAs, b_cancel: MainLang.Cancel);
        if (cr != ContentDialogResult.Primary)
        {
            return false;
        }

        var fn = architecture switch
        {
            "linux-arm" => "YMCL.Desktop.linux.arm.AppImage",
            "linux-arm64" => "YMCL.Desktop.linux.arm64.AppImage",
            "linux-x64" => "YMCL.Desktop.linux.x64.AppImage",
            "osx-x64" => "YMCL.Desktop.osx.mac.x64.app.zip",
            "osx-arm64" => "YMCL.Desktop.osx.mac.arm64.app.zip",
            "win-x64" => Environment.OSVersion.Version.Major >= 10
                ? "YMCL.Desktop.win.x64.installer.exe"
                : "YMCL.Desktop.win7.x64.exe.zip",
            "win-x86" => Environment.OSVersion.Version.Major >= 10
                ? "YMCL.Desktop.win.x86.installer.exe"
                : "YMCL.Desktop.win7.x86.exe.zip",
            "win-arm64" => Environment.OSVersion.Version.Major >= 10
                ? "YMCL.Desktop.win.arm64.installer.exe"
                : "YMCL.Desktop.win7.arm64.exe.zip",
            _ => "File.unknown"
        };

        var path = (await TopLevel.GetTopLevel(YMCL.App.UiRoot).StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = MainLang.SaveAs,
                SuggestedFileName = fn,
            }))?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            Notice(MainLang.CanceledUpdate);
            return true;
        }

        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        var url = string.Empty;

        var task = new TaskEntry(MainLang.CheckUpdate,
        [
            new SubTask(MainLang.CheckUpdate),
            new SubTask(MainLang.GetUpdateUrl),
            new SubTask(MainLang.DownloadResource),
            new SubTask(MainLang.Update),
        ], TaskState.Running);
        task.UpdateAction(() => { cts.Cancel(); });
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl, token));
            var assets = (JArray)githubApiJson[0]["assets"];
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
                        break;
                }
            }

            task.AdvanceSubTask();
            if (url == null)
            {
                task.FinishWithError();
                return false;
            }

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
                        await using (var fileStream = new FileStream(path, FileMode.Create,
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
                task.FinishWithSuccess();
                return true;
            }
            catch (OperationCanceledException)
            {
                task.Cancel();
                Notice(MainLang.CanceledUpdate, NotificationType.Warning);
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
            Notice(MainLang.CanceledUpdate, NotificationType.Warning);
            return true;
        }
        catch
        {
            task.FinishWithError();
            return false;
        }
    }

    public static async Task<bool> UpdateByAutoInstaller(string architecture)
    {
        YMCL.App.UiRoot.Nav.SelectedItem = YMCL.App.UiRoot.NavTask;
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        var url = string.Empty;
        var fileName = string.Empty;

        var task = new TaskEntry(MainLang.CheckUpdate,
        [
            new SubTask(MainLang.CheckUpdate),
            new SubTask(MainLang.GetUpdateUrl),
            new SubTask(MainLang.DownloadResource),
            new SubTask(MainLang.Update),
        ], TaskState.Running);
        task.UpdateAction(() => { cts.Cancel(); });
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl, token));
            var assets = (JArray)githubApiJson[0]["assets"];
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
                var startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Path.Combine(ConfigPath.UpdateFolderPath, fileName)
                };
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch (OperationCanceledException)
            {
                task.Cancel();
                Notice(MainLang.CanceledUpdate, NotificationType.Warning);
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
            Notice(MainLang.CanceledUpdate, NotificationType.Warning);
            return true;
        }
        catch
        {
            task.FinishWithError();
            return false;
        }
    }
}