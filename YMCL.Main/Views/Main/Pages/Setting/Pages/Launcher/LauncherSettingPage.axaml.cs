using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Main.Public;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;
using Application = Avalonia.Application;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher;

public partial class LauncherSettingPage : UserControl
{
    public LauncherSettingPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
        OpenUserDataFolderBtn.Click += async (s, e) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Const.String.UserDataRootPath));
        };
        CheckUpdateBtn.Click += (_, _) => { _ = CheckUpdateAsync(); };
    }

    private void ControlProperty()
    {
        UserDataFolderPath.Text = Const.String.UserDataRootPath;
        var lenth = Method.Value.GetDirectoryLength(Const.String.UserDataRootPath);
        var userDataSize = Math.Round(lenth / 1024, 2) >= 512
            ? $"{Math.Round(lenth / 1024 / 1024, 2)} Mib"
            : $"{Math.Round(lenth / 1024, 2)} Kib";
        UserDataSize.Text = userDataSize;
        var resourceName = "YMCL.Main.Public.Texts.DateTime.txt";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using (var reader = new StreamReader(stream!))
        {
            var result = reader.ReadToEnd();
            Version.Text = $"v{result.Trim()}";
        }
    }

    private async Task CheckUpdateAsync()
    {
        #region BtnUi

        CheckUpdateBtn.IsEnabled = false;
        var ring = new ProgressRing();
        CheckUpdateBtn.Width = CheckUpdateBtn.Bounds.Width;
        CheckUpdateBtn.Content = ring;
        ring.Height = 17;
        ring.Width = 17;

        #endregion

        var url = string.Empty;
        var architecture = Method.Value.GetCurrentPlatformAndArchitecture();
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl));
            var apiVersion = (string)githubApiJson[0]["name"];
            var localVersion = Version.Text;
            if (apiVersion != localVersion)
            {
                var dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion,
                    $"{apiVersion!}\n\n{(string)githubApiJson[0]["html_url"]}", b_cancel: MainLang.Cancel,
                    b_primary: MainLang.Ok);
                if (dialog == ContentDialogResult.Primary)
                {
                    var task = new WindowTask(MainLang.CheckUpdate);
                    task.UpdateTextProgress(MainLang.CheckUpdate);
                    var assets = (JArray)githubApiJson[0]["assets"];

                    var isAloneProgram = false;
                    var resourceName = "YMCL.Main.Public.Texts.IsAloneProgram.txt";
                    var _assembly = Assembly.GetExecutingAssembly();
                    var stream = _assembly.GetManifestResourceStream(resourceName);
                    using (var reader = new StreamReader(stream!))
                    {
                        var result = reader.ReadToEnd();
                        if (string.IsNullOrEmpty(result))
                            isAloneProgram = false;
                        else
                            isAloneProgram = true;
                    }

                    if (!isAloneProgram)
                        foreach (JObject asset in assets)
                        {
                            var name = (string)asset["name"];
                            var browser_download_url = (string)asset["browser_download_url"];
                            if (name == "YMCL.Main.linux.arm.bin" && architecture == "linux-arm")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.linux.arm64.bin" && architecture == "linux-arm64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.linux.x64.bin" && architecture == "linux-x64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.osx.x64.bin" && architecture == "osx-x64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.osx.arm64.bin" && architecture == "osx-arm64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.win.x64.exe" && architecture == "win-x64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.win.x86.exe" && architecture == "win-x86")
                                url = browser_download_url;
                        }
                    else
                        foreach (JObject asset in assets)
                        {
                            var name = (string)asset["name"];
                            var browser_download_url = (string)asset["browser_download_url"];
                            if (name == "YMCL.Main.alone.linux.arm.bin" && architecture == "linux-arm")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.alone.linux.arm64.bin" && architecture == "linux-arm64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.alone.linux.x64.bin" && architecture == "linux-x64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.alone.osx.x64.bin" && architecture == "osx-x64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.alone.osx.arm64.bin" && architecture == "osx-arm64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.alone.win.x64.exe" && architecture == "win-x64")
                                url = browser_download_url;
                            else if (name == "YMCL.Main.alone.win.x86.exe" && architecture == "win-x86")
                                url = browser_download_url;
                        }

                    if (url == null)
                    {
                        var comboBox = new ComboBox
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };
                        comboBox.Items.Add("win-x64");
                        comboBox.Items.Add("win-x86");
                        comboBox.Items.Add("osx-x64");
                        comboBox.Items.Add("osx-arm64");
                        comboBox.Items.Add("linux-x64");
                        comboBox.Items.Add("linux-arm64");
                        comboBox.Items.Add("linux-arm");
                        comboBox.SelectedIndex = 0;
                        ContentDialog dialog1 = new()
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            Title = MainLang.ChooseArchitecture,
                            PrimaryButtonText = MainLang.Ok,
                            DefaultButton = ContentDialogButton.Primary,
                            Content = comboBox
                        };
                        await dialog1.ShowAsync();
                        if (!isAloneProgram)
                            foreach (JObject asset in assets)
                            {
                                var name = (string)asset["name"];
                                var browser_download_url = (string)asset["browser_download_url"];
                                if (name == "YMCL.Main.alone.linux.arm.bin" &&
                                    comboBox.SelectedItem.ToString() == "linux-arm")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.alone.linux.arm64.bin" &&
                                         comboBox.SelectedItem.ToString() == "linux-arm64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.alone.linux.x64.bin" &&
                                         comboBox.SelectedItem.ToString() == "linux-x64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.alone.osx.x64.bin" &&
                                         comboBox.SelectedItem.ToString() == "osx-x64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.alone.osx.arm64.bin" &&
                                         comboBox.SelectedItem.ToString() == "osx-arm64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.alone.win.x64.exe" &&
                                         comboBox.SelectedItem.ToString() == "win-x64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.alone.win.x86.exe" &&
                                         comboBox.SelectedItem.ToString() == "win-x86") url = browser_download_url;
                            }
                        else
                            foreach (JObject asset in assets)
                            {
                                var name = (string)asset["name"];
                                var browser_download_url = (string)asset["browser_download_url"];
                                if (name == "YMCL.Main.linux.arm.bin" &&
                                    comboBox.SelectedItem.ToString() == "linux-arm")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.linux.arm64.bin" &&
                                         comboBox.SelectedItem.ToString() == "linux-arm64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.linux.x64.bin" &&
                                         comboBox.SelectedItem.ToString() == "linux-x64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.osx.x64.bin" &&
                                         comboBox.SelectedItem.ToString() == "osx-x64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.osx.arm64.bin" &&
                                         comboBox.SelectedItem.ToString() == "osx-arm64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.win.x64.exe" &&
                                         comboBox.SelectedItem.ToString() == "win-x64")
                                    url = browser_download_url;
                                else if (name == "YMCL.Main.win.x86.exe" &&
                                         comboBox.SelectedItem.ToString() == "win-x86") url = browser_download_url;
                            }
                    }

                    if (url != null)
                    {
                        var setting =
                            JsonConvert.DeserializeObject<Public.Classes.Setting>(
                                File.ReadAllText(Const.String.SettingDataPath));
                        var trueUrl = url;
                        if (setting.EnableCustomUpdateUrl)
                        {
                            trueUrl = setting.CustomUpdateUrl.Replace("{%url%}", url);
                        }

                        task.UpdateTextProgress($"{MainLang.GetUpdateUrl}: {trueUrl}");
                        var saveFile = Const.Data.Platform == Platform.Windows ? "Update.exe" : "Update";
                        task.UpdateTextProgress(
                            $"{MainLang.BeginDownload}: {Path.Combine(Const.String.UserDataRootPath, saveFile)}");
                        try
                        {
                            var handler = new HttpClientHandler();
                            handler.ServerCertificateCustomValidationCallback =
                                (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };
                            ServicePointManager.SecurityProtocol =
                                SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                            using (var client = new HttpClient(handler))
                            {
                                client.DefaultRequestHeaders.Add("User-Agent",
                                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
                                using (var response =
                                       await client.GetAsync(trueUrl, HttpCompletionOption.ResponseHeadersRead))
                                {
                                    response.EnsureSuccessStatusCode();

                                    using (var downloadStream = await response.Content.ReadAsStreamAsync())
                                    {
                                        using (var fileStream = new FileStream(
                                                   Path.Combine(Const.String.UserDataRootPath, saveFile), FileMode.Create,
                                                   FileAccess.Write))
                                        {
                                            var buffer = new byte[8192];
                                            int bytesRead;
                                            long totalBytesRead = 0;
                                            var totalBytes = response.Content.Headers.ContentLength.HasValue
                                                ? response.Content.Headers.ContentLength.Value
                                                : -1;

                                            while ((bytesRead =
                                                       await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                            {
                                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                                totalBytesRead += bytesRead;

                                                if (totalBytes > 0)
                                                {
                                                    var progress = (double)totalBytesRead / totalBytes * 100;
                                                    task.UpdateValueProgress(progress);
                                                }
                                            }
                                        }
                                    }
                                }

                                task.UpdateTextProgress($"{MainLang.DownloadFinish}");
                                if (architecture == "win-x86" || architecture == "win-x64")
                                {
                                    var resourceName1 = "YMCL.Main.Public.Bins.YMCL.Update.Helper.win.exe";
                                    var assembly = Assembly.GetExecutingAssembly();
                                    using (var resourceStream = assembly.GetManifestResourceStream(resourceName1))
                                    {
                                        var outputFilePath = Path.Combine(Const.String.UserDataRootPath,
                                            "YMCL.Update.Helper.win.exe");
                                        using (var fileStream = new FileStream(outputFilePath, FileMode.Create,
                                                   FileAccess.Write))
                                        {
                                            resourceStream.CopyTo(fileStream);
                                        }
                                    }

                                    var startInfo = new ProcessStartInfo
                                    {
                                        UseShellExecute = true,
                                        WorkingDirectory = Environment.CurrentDirectory,
                                        FileName = Path.Combine(Const.String.UserDataRootPath, "YMCL.Update.Helper.win.exe"),
                                        Arguments =
                                            $"\"{Path.Combine(Const.String.UserDataRootPath, saveFile)}\" \"{Process.GetCurrentProcess().MainModule.FileName}\"",
                                        Verb = "runas"
                                    };
                                    Process.Start(startInfo);
                                    Environment.Exit(0);
                                }
                                else
                                {
                                    var dialog1 = await Method.Ui.ShowDialogAsync(MainLang.Update,
                                        $"{MainLang.ThisArchitectureCannotAutoUpdate}: {Path.Combine(Const.String.UserDataRootPath, saveFile)}",
                                        b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
                                    if (dialog1 == ContentDialogResult.Primary)
                                    {
                                        var launcher = TopLevel.GetTopLevel(this).Launcher;
                                        await launcher.LaunchDirectoryInfoAsync(
                                            new DirectoryInfo(Const.String.UserDataRootPath));
                                        await Task.Delay(250);
                                        task.Destory();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Method.Ui.ShowShortException(MainLang.UpdateFail, ex);
                        }
                    }
                    else
                    {
                        CheckUpdateBtn.IsEnabled = true;
                        CheckUpdateBtn.Content = MainLang.CheckUpdate;
                        task.Destory();
                    }
                }
                else
                {
                    CheckUpdateBtn.IsEnabled = true;
                    CheckUpdateBtn.Content = MainLang.CheckUpdate;
                }
            }
            else
            {
                Method.Ui.Toast(MainLang.CurrentlyTheLatestVersion, Const.Notification.main);
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
            }
        }
        catch (Exception ex)
        {
            CheckUpdateBtn.IsEnabled = true;
            CheckUpdateBtn.Content = MainLang.CheckUpdate;
            Method.Ui.ShowLongException(MainLang.CheckUpdateFail, ex);
        }
    }
}