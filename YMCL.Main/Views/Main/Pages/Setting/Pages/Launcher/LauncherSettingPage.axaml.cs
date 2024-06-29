using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using YMCL.Main.Public;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;
using static System.Net.Mime.MediaTypeNames;
using Application = Avalonia.Application;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher
{
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
                Method.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            };
            OpenUserDataFolderBtn.Click += async (s, e) =>
            {
                var launcher = TopLevel.GetTopLevel(this).Launcher;
                await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Const.UserDataRootPath));
            };
            CheckUpdateBtn.Click += (_, _) =>
            {
                _ = CheckUpdateAsync();
            };
        }
        private void ControlProperty()
        {
            UserDataFolderPath.Text = Const.UserDataRootPath;
            var userDataSize = Math.Round(Method.GetDirectoryLength(Const.UserDataRootPath) / 1024, 2) >= 512 ? $"{Math.Round(Method.GetDirectoryLength(Const.UserDataRootPath) / 1024 / 1024, 2)} Mib" : $"{Math.Round(Method.GetDirectoryLength(Const.UserDataRootPath) / 1024, 2)} Kib";
            UserDataSize.Text = userDataSize;
            string resourceName = "YMCL.Main.Public.Texts.DateTime.txt";
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream stream = _assembly.GetManifestResourceStream(resourceName);
            using (StreamReader reader = new StreamReader(stream!))
            {
                string result = reader.ReadToEnd();
                Version.Text = $"v{result}";
            }
        }
        private async Task CheckUpdateAsync()
        {
            #region BtnUi
            CheckUpdateBtn.IsEnabled = false;
            ProgressRing ring = new ProgressRing();
            CheckUpdateBtn.Width = CheckUpdateBtn.Bounds.Width;
            CheckUpdateBtn.Content = ring;
            ring.Height = 17;
            ring.Width = 17;
            #endregion
            var url = string.Empty;
            string architecture = Method.GetCurrentPlatformAndArchitecture();
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
                var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.GithubUpdateApiUrl));
                var apiVersion = (string)githubApiJson[0]["name"];
                var localVersion = Version.Text;
                if (apiVersion != localVersion)
                {
                    var dialog = await Method.ShowDialogAsync(MainLang.FoundNewVersion, $"{apiVersion!}\n\n{(string)githubApiJson[0]["html_url"]}", b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
                    if (dialog == ContentDialogResult.Primary)
                    {
                        var task = new WindowTask(MainLang.CheckUpdate, true);
                        task.UpdateTextProgress(MainLang.CheckUpdate);
                        JArray assets = (JArray)githubApiJson[0]["assets"];
                        foreach (JObject asset in assets)
                        {
                            string name = (string)asset["name"];
                            string browser_download_url = (string)asset["browser_download_url"];
                            if (name == "YMCL.Main.linux.arm" && architecture == "linux-arm")
                            {
                                url = browser_download_url;
                            }
                            else if (name == "YMCL.Main.linux.arm64" && architecture == "linux-arm64")
                            {
                                url = browser_download_url;
                            }
                            else if (name == "YMCL.Main.linux.x64" && architecture == "linux-x64")
                            {
                                url = browser_download_url;
                            }
                            else if (name == "YMCL.Main.osx.x64" && architecture == "osx-x64")
                            {
                                url = browser_download_url;
                            }
                            else if (name == "YMCL.Main.osx.arm64" && architecture == "osx-arm64")
                            {
                                url = browser_download_url;
                            }
                            else if (name == "YMCL.Main.win.x64.exe" && architecture == "win-x64")
                            {
                                url = browser_download_url;
                            }
                            else if (name == "YMCL.Main.win.x86.exe" && architecture == "win-x86")
                            {
                                url = browser_download_url;
                            }
                        }
                        if (url == null)
                        {
                            var comboBox = new ComboBox()
                            {
                                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
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
                            foreach (JObject asset in assets)
                            {
                                string name = (string)asset["name"];
                                string browser_download_url = (string)asset["browser_download_url"];
                                if (name == "YMCL.Main.linux.arm" && comboBox.SelectedItem.ToString() == "linux-arm")
                                {
                                    url = browser_download_url;
                                }
                                else if (name == "YMCL.Main.linux.arm64" && comboBox.SelectedItem.ToString() == "linux-arm64")
                                {
                                    url = browser_download_url;
                                }
                                else if (name == "YMCL.Main.linux.x64" && comboBox.SelectedItem.ToString() == "linux-x64")
                                {
                                    url = browser_download_url;
                                }
                                else if (name == "YMCL.Main.osx.x64" && comboBox.SelectedItem.ToString() == "osx-x64")
                                {
                                    url = browser_download_url;
                                }
                                else if (name == "YMCL.Main.osx.arm64" && comboBox.SelectedItem.ToString() == "osx-arm64")
                                {
                                    url = browser_download_url;
                                }
                                else if (name == "YMCL.Main.win.x64.exe" && comboBox.SelectedItem.ToString() == "win-x64")
                                {
                                    url = browser_download_url;
                                }
                                else if (name == "YMCL.Main.win.x86.exe" && comboBox.SelectedItem.ToString() == "win-x86")
                                {
                                    url = browser_download_url;
                                }
                            }
                        }
                        if (url != null)
                        {
                            task.UpdateTextProgress($"{MainLang.GetUpdateUrl}£º{url}", true);
                            var saveFile = Const.Platform == Platform.Windows ? "Update.exe" : "Update";
                            task.UpdateTextProgress($"{MainLang.BeginDownload}£º{Path.Combine(Const.UserDataRootPath, saveFile)}", true);
                            try
                            {
                                //url = "http://127.0.0.1:5500/a.file";
                                var handler = new HttpClientHandler();
                                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                                using (HttpClient client = new HttpClient(handler))
                                {
                                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
                                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                                    {
                                        response.EnsureSuccessStatusCode();

                                        using (var downloadStream = await response.Content.ReadAsStreamAsync())
                                        {
                                            using (var fileStream = new FileStream(Path.Combine(Const.UserDataRootPath, saveFile), FileMode.Create, FileAccess.Write))
                                            {
                                                byte[] buffer = new byte[8192];
                                                int bytesRead;
                                                long totalBytesRead = 0;
                                                long totalBytes = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1;

                                                while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                                {
                                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                                    totalBytesRead += bytesRead;

                                                    if (totalBytes > 0)
                                                    {
                                                        double progress = ((double)totalBytesRead / totalBytes) * 100;
                                                        task.UpdateValueProgress(progress);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    task.UpdateTextProgress($"{MainLang.DownloadFinish}", true);
                                    if (architecture == "win-x86" || architecture == "win-x64")
                                    {
                                        string resourceName = "YMCL.Main.Public.Bins.YMCL.Update.Helper.win.exe";
                                        Assembly assembly = Assembly.GetExecutingAssembly();
                                        using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                                        {
                                            string outputFilePath = Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.win.exe");
                                            using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                                            {
                                                resourceStream.CopyTo(fileStream);
                                            }
                                        }
                                        ProcessStartInfo startInfo = new ProcessStartInfo
                                        {
                                            UseShellExecute = true,
                                            WorkingDirectory = Environment.CurrentDirectory,
                                            FileName = Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.win.exe"),
                                            Arguments = $"{Path.Combine(Const.UserDataRootPath, saveFile)} {Process.GetCurrentProcess().MainModule.FileName}"
                                        };
                                        Process.Start(startInfo);
                                        Environment.Exit(0);
                                    }
                                    //else if (architecture == "linux-x64")
                                    //{
                                    //    string resourceName = "YMCL.Main.Public.Bins.YMCL.Update.Helper.linux";
                                    //    Assembly assembly = Assembly.GetExecutingAssembly();
                                    //    using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                                    //    {
                                    //        string outputFilePath = Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.linux");
                                    //        using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                                    //        {
                                    //            resourceStream.CopyTo(fileStream);
                                    //        }
                                    //    }
                                    //    ProcessStartInfo startInfo = new ProcessStartInfo
                                    //    {
                                    //        UseShellExecute = true,
                                    //        WorkingDirectory = Environment.CurrentDirectory,
                                    //        FileName = Path.Combine(Const.UserDataRootPath, "YMCL.Update.Helper.linux"),
                                    //        Arguments = $"{Path.Combine(Const.UserDataRootPath, saveFile)} {Process.GetCurrentProcess().MainModule.FileName}"
                                    //    };
                                    //    Process.Start(startInfo);
                                    //    Environment.Exit(0);
                                    //}
                                    else
                                    {
                                        var dialog1 = await Method.ShowDialogAsync(MainLang.Update, $"{MainLang.ThisArchitectureCannotAutoUpdate}£º{Path.Combine(Const.UserDataRootPath, saveFile)}", b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
                                        if (dialog1 == ContentDialogResult.Primary)
                                        {
                                            var launcher = TopLevel.GetTopLevel(this).Launcher;
                                            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Const.UserDataRootPath));
                                            await Task.Delay(250);
                                            task.Destory();
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Method.ShowShortException(MainLang.UpdateFail, ex);
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

                }
            }
            catch (Exception ex)
            {
                Method.ShowLongException(MainLang.CheckUpdateFail, ex);
            }
        }
    }
}
