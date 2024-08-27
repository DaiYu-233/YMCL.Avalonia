using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using System.Timers;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using CurseForge.APIClient;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Install;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Analyzer;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StarLight_Core.Authentication;
using StarLight_Core.Launch;
using StarLight_Core.Models.Authentication;
using StarLight_Core.Models.Launch;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.PageTaskEntry;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;
using File = System.IO.File;
using FileInfo = YMCL.Main.Public.Classes.FileInfo;
using Path = System.IO.Path;
using static YMCL.Main.Public.Plugin;
using Account = MinecraftLaunch.Classes.Models.Auth.Account;
using LaunchConfig = MinecraftLaunch.Classes.Models.Launch.LaunchConfig;
using MicrosoftAccount = MinecraftLaunch.Classes.Models.Auth.MicrosoftAccount;
using Timer = System.Timers.Timer;
using YggdrasilAccount = MinecraftLaunch.Classes.Models.Auth.YggdrasilAccount;

namespace YMCL.Main.Public;

public class Method
{
    public static class Ui
    {
        public static async void PageLoadAnimation((double, double, double, double) original,
            (double, double, double, double) target, TimeSpan time, Control control, bool visibility = false)
        {
            var (ol, ot, or, ob) = original;
            var (tl, tt, tr, tb) = target;

            var transitions = control.Transitions;

            if (control != null && control.Transitions != null)
            {
                control.Transitions.Clear();
                control.Margin = new Thickness(ol, ot, or, ob);
                control.Opacity = 0;
                control.Transitions.Add(new ThicknessTransition
                {
                    Duration = time,
                    Easing = new SineEaseInOut(),
                    Property = Layoutable.MarginProperty
                });
                control.Transitions.Add(new DoubleTransition
                {
                    Duration = time,
                    Easing = new SineEaseInOut(),
                    Property = Visual.OpacityProperty
                });
                if (visibility) control.IsVisible = true;
                control.Margin = new Thickness(tl, tt, tr, tb);
                control.Opacity = 1;
                await Task.Delay(time);
                control.Transitions = transitions;
            }
        }

        public static void Toast(string msg, WindowNotificationManager p_notification = null,
            NotificationType type = NotificationType.Information, bool time = true,
            string title = "Yu Minecraft Launcher")
        {
            var notification = p_notification == null ? Const.Notification.main : p_notification;
            var showTitle = Const.String.AppTitle;
            if (!string.IsNullOrEmpty(title)) showTitle = title;
            if (time) showTitle += $" - {DateTime.Now.ToString("HH:mm:ss")}";
            notification.Show(new Notification(showTitle, msg, type));
        }

        public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = "Content",
            Control p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null,
            Window p_window = null)
        {
            var content = p_content == null
                ? new TextBox
                {
                    TextWrapping = TextWrapping.Wrap, IsReadOnly = true,
                    FontFamily = (FontFamily)Application.Current.Resources["Font"], Text = msg
                }
                : p_content;
            var window = p_window == null ? Const.Window.main : p_window;
            var dialog = new ContentDialog
            {
                PrimaryButtonText = b_primary,
                Content = content,
                DefaultButton = ContentDialogButton.Primary,
                CloseButtonText = b_cancel,
                SecondaryButtonText = b_secondary,
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Title = title
            };
            var result = await dialog.ShowAsync(window);
            return result;
        }

        public static async void ShowLongException(string msg, Exception ex)
        {
            var textBox = new TextBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                Text = $"{msg} - {ex.Message}\n\n{ex}", HorizontalAlignment = HorizontalAlignment.Center,
                IsReadOnly = true
            };
            await ShowDialogAsync(MainLang.GetException, p_content: textBox, b_primary: MainLang.Ok);
        }

        public static void SetAccentColor(Color color)
        {
            Application.Current.Resources["SystemAccentColor"] = color;
            Application.Current.Resources["SystemAccentColorLight1"] = Value.ColorVariant(color, 0.15f);
            Application.Current.Resources["SystemAccentColorLight2"] = Value.ColorVariant(color, 0.30f);
            Application.Current.Resources["SystemAccentColorLight3"] = Value.ColorVariant(color, 0.45f);
            Application.Current.Resources["SystemAccentColorDark1"] = Value.ColorVariant(color, -0.15f);
            Application.Current.Resources["SystemAccentColorDark2"] = Value.ColorVariant(color, -0.30f);
            Application.Current.Resources["SystemAccentColorDark3"] = Value.ColorVariant(color, -0.45f);
        }

        public static void ToggleTheme(Theme theme)
        {
            if (theme == Theme.Light)
            {
                var rd =
                    (AvaloniaXamlLoader.Load(new Uri("avares://YMCL.Main/Public/Styles/LightTheme.axaml")) as
                        ResourceDictionary)!;
                Application.Current!.Resources.MergedDictionaries.Add(rd);
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            }
            else if (theme == Theme.Dark)
            {
                var rd =
                    (AvaloniaXamlLoader.Load(new Uri("avares://YMCL.Main/Public/Styles/DarkTheme.axaml")) as
                        ResourceDictionary)!;
                Application.Current!.Resources.MergedDictionaries.Add(rd);
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            }
        }

        public static void SetWindowBackGroundImg()
        {
            var setting = Const.Data.Setting;
            if (setting.EnableCustomBackGroundImg && !string.IsNullOrEmpty(setting.WindowBackGroundImgData))
            {
                Application.Current.Resources["Opacity"] = 0.875;
                var bitmap = Value.Base64ToBitmap(setting.WindowBackGroundImgData);
                Const.Window.main.BackGroundImg.Source = bitmap;
                Const.Window.taskCenter.BackGroundImg.Source = bitmap;
            }
            else
            {
                Application.Current.Resources["Opacity"] = 1.0;
                Const.Window.main.BackGroundImg.Source = null;
                Const.Window.taskCenter.BackGroundImg.Source = null;
            }
        }

        public static void CheckLauncher()
        {
            async void UnofficialToast()
            {
                await Method.Ui.ShowDialogAsync("Error !", MainLang.UnofficialTip);
                UnofficialToast();
            }

            var name = Const.Window.main.settingPage.launcherSettingPage.Version.Text;
            var author = Const.Window.main.settingPage.launcherSettingPage.AuthorLabel.Content;
            Task.Run(async () =>
            {
                string url = "https://player.daiyu.fun/a.json";
                HttpClient client = new HttpClient();

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<string>>(responseBody);
                    foreach (var se in list)
                    {
                        if (se.Equals(name))
                        {
                            Dispatcher.UIThread.Invoke(
                                () => { UnofficialToast(); });
                        }

                        if (se.Equals(author))
                        {
                            Dispatcher.UIThread.Invoke(
                                () => { UnofficialToast(); });
                        }
                    }
                }
                catch
                {
                }
            });
        }

        public static void ShowShortException(string msg, Exception ex)
        {
            Toast($"{msg}\n{ex.Message}", Const.Notification.main, NotificationType.Error);
        }

        public static async Task<bool> UpgradeToAdministratorPrivilegesAsync(Window p_window = null)
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var windwo = p_window == null ? Const.Window.main : p_window;
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var result = await ShowDialogAsync(MainLang.UpgradeToAdministratorPrivileges,
                    MainLang.UpgradeToAdministratorPrivilegesTip, b_primary: MainLang.Ok, b_secondary: MainLang.Cancel,
                    p_window: windwo);
                if (result == ContentDialogResult.Primary)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Process.GetCurrentProcess().MainModule.FileName!,
                        Verb = "runas"
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                    return true;
                }

                return false;
            }

            return true;
        }

        public static void RestartApp()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName!
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        public static async Task<(bool, string, string)> CheckUpdateAsync()
        {
            try
            {
                var resourceName = "YMCL.Main.Public.Texts.DateTime.txt";
                var _assembly = Assembly.GetExecutingAssembly();
                var stream = _assembly.GetManifestResourceStream(resourceName);
                var version = string.Empty;
                using (var reader = new StreamReader(stream!))
                {
                    var result = reader.ReadToEnd();
                    version = $"v{result.Trim()}";
                }

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
                var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl));
                var apiVersion = (string)githubApiJson[0]["name"]!;
                return (apiVersion != version, apiVersion, $"{apiVersion!}\n\n{(string)githubApiJson[0]["html_url"]}");
            }
            catch
            {
                return (false, string.Empty, string.Empty);
            }
        }

        public static async Task<bool> UpdateAppAsync()
        {
            try
            {
                var architecture = Value.GetCurrentPlatformAndArchitecture();
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
                var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl));
                var task = new WindowTask(MainLang.CheckUpdate);
                task.UpdateTextProgress(MainLang.CheckUpdate);
                var assets = (JArray)githubApiJson[0]["assets"];
                var url = String.Empty;
                var fileName = string.Empty;
                foreach (var jToken in assets)
                {
                    var asset = (JObject)jToken;
                    var name = (string)asset["name"];
                    var browser_download_url = (string)asset["browser_download_url"];
                    switch (name)
                    {
                        case "YMCL.Main.linux.arm.AppImage" when architecture == "linux-arm":
                        case "YMCL.Main.linux.arm64.AppImage" when architecture == "linux-arm64":
                        case "YMCL.Main.linux.x64.AppImage" when architecture == "linux-x64":
                        case "YMCL.Main.osx.mac.x64.app.zip" when architecture == "osx-x64":
                        case "YMCL.Main.osx.mac.arm64.app.zip" when architecture == "osx-arm64":
                        case "YMCL.Main.win.x64.installer.exe" when architecture == "win-x64":
                        case "YMCL.Main.win.x86.installer.exe" when architecture == "win-x86":
                            url = browser_download_url;
                            fileName = name;
                            break;
                    }
                }

                if (url == null)
                {
                    task.Destory();
                    return false;
                }

                var setting = Const.Data.Setting;
                var trueUrl = url;
                if (setting.EnableCustomUpdateUrl)
                {
                    trueUrl = setting.CustomUpdateUrl.Replace("{%url%}", url);
                }

                task.UpdateTextProgress($"{MainLang.GetUpdateUrl}: {trueUrl}");
                task.UpdateTextProgress(
                    $"{MainLang.BeginDownload}: {Path.Combine(Const.String.TempFolderPath, fileName)}");
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
                           await client.GetAsync(trueUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        await using (var downloadStream = await response.Content.ReadAsStreamAsync())
                        {
                            await using (var fileStream = new FileStream(
                                             Path.Combine(Const.String.TempFolderPath, fileName), FileMode.Create,
                                             FileAccess.Write))
                            {
                                var buffer = new byte[8192];
                                int bytesRead;
                                long totalBytesRead = 0;
                                var totalBytes = response.Content.Headers.ContentLength.HasValue
                                    ? response.Content.Headers.ContentLength.Value
                                    : -1;

                                while ((bytesRead =
                                           await downloadStream.ReadAsync(buffer)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                                    totalBytesRead += bytesRead;

                                    if (totalBytes <= 0) continue;
                                    var progress = (double)totalBytesRead / totalBytes * 100;
                                    task.UpdateValueProgress(progress);
                                }
                            }
                        }
                    }

                    task.UpdateTextProgress($"{MainLang.DownloadFinish}");
                    if (architecture == "win-x86" || architecture == "win-x64")
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            FileName = Path.Combine(Const.String.TempFolderPath, fileName)
                        };
                        Process.Start(startInfo);
                        Environment.Exit(0);
                    }
                    else
                    {
                        var launcher = TopLevel.GetTopLevel(Const.Window.main).Launcher;
                        await launcher.LaunchDirectoryInfoAsync(
                            new DirectoryInfo(Const.String.TempFolderPath));
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.UpdateFail, ex);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public static class IO
    {
        public static async Task<List<FolderInfo>> OpenFolderPicker(TopLevel topLevel = null,
            FolderPickerOpenOptions options = null)
        {
            var isPrimaryButtonClick = false;
            var setting = Const.Data.Setting;
            if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
            {
                if (options != null && topLevel != null)
                {
                    var storageProvider = topLevel!.StorageProvider;
                    var result = await storageProvider.OpenFolderPickerAsync(options);
                    var list = new List<FolderInfo>();
                    result.ToList().ForEach(item =>
                    {
                        list.Add(new FolderInfo { Name = item.Name, Path = item.Path.LocalPath });
                    });
                    return list;
                }

                new Exception("ParameterIsNull");
                return null;
            }
            else
            {
                var textBox = new TextBox
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap
                };
                ContentDialog dialog = new()
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = MainLang.InputFolderPath,
                    PrimaryButtonText = MainLang.Ok,
                    CloseButtonText = MainLang.Cancel,
                    DefaultButton = ContentDialogButton.Primary,
                    Content = textBox
                };
                dialog.PrimaryButtonClick += (_, _) => { isPrimaryButtonClick = true; };
                var result = await dialog.ShowAsync();
                var path = textBox.Text;
                if (!Directory.Exists(path) && isPrimaryButtonClick)
                {
                    Ui.Toast(MainLang.FolderNotExist, Const.Notification.main, NotificationType.Error);
                    return null;
                }

                var folder = Path.GetFileName(path);
                var list = new List<FolderInfo> { new() { Name = folder, Path = path } };
                return list;
            }
        }

        public static async Task<List<FileInfo>> OpenFilePicker(TopLevel topLevel = null,
            FilePickerOpenOptions options = null, string p_title = null)
        {
            var title = p_title == null ? MainLang.InputFilePath : p_title;
            var isPrimaryButtonClick = false;
            var setting = Const.Data.Setting;
            if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
            {
                if (options != null && topLevel != null)
                {
                    var storageProvider = topLevel!.StorageProvider;
                    var result = await storageProvider.OpenFilePickerAsync(options);
                    var list = new List<FileInfo>();
                    result.ToList().ForEach(item =>
                    {
                        var path = item.Path.LocalPath;
                        var fullName = Path.GetFileName(path);
                        var name = Path.GetFileNameWithoutExtension(fullName);
                        var extension = Path.GetExtension(fullName);
                        list.Add(new FileInfo { Name = name, Path = path, FullName = fullName, Extension = extension });
                    });
                    return list;
                }

                new Exception("ParameterIsNull");
                return null;
            }
            else
            {
                var textBox = new TextBox
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap
                };
                ContentDialog dialog = new()
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = title,
                    PrimaryButtonText = MainLang.Ok,
                    CloseButtonText = MainLang.Cancel,
                    DefaultButton = ContentDialogButton.Primary,
                    Content = textBox
                };
                dialog.PrimaryButtonClick += (_, _) => { isPrimaryButtonClick = true; };
                var result = await dialog.ShowAsync();
                var path = textBox.Text;
                if (!File.Exists(path) && isPrimaryButtonClick)
                {
                    Ui.Toast(MainLang.FileNotExist, Const.Notification.main, NotificationType.Error);
                    return null;
                }

                var fullName = Path.GetFileName(path);
                var name = Path.GetFileNameWithoutExtension(fullName);
                var extension = Path.GetExtension(fullName);
                var list = new List<FileInfo>
                    { new() { Name = name, Path = path, FullName = fullName, Extension = extension } };
                return list;
            }
        }

        public static async Task<string> SaveFilePicker(TopLevel topLevel = null, FilePickerSaveOptions options = null)
        {
            var isPrimaryButtonClick = false;
            var setting = Const.Data.Setting;
            if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
            {
                if (options != null && topLevel != null)
                {
                    var storageProvider = topLevel!.StorageProvider;
                    var result = await storageProvider.SaveFilePickerAsync(options);
                    if (result != null)
                        return result.TryGetLocalPath();
                    return null;
                }

                new Exception("ParameterIsNull");
                return null;
            }
            else
            {
                var textBox = new TextBox
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap
                };
                ContentDialog dialog = new()
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = MainLang.InputFilePath,
                    PrimaryButtonText = MainLang.Ok,
                    CloseButtonText = MainLang.Cancel,
                    DefaultButton = ContentDialogButton.Primary,
                    Content = textBox
                };
                dialog.PrimaryButtonClick += (_, _) => { isPrimaryButtonClick = true; };
                var result = await dialog.ShowAsync();
                var path = textBox.Text;
                if (!Directory.Exists(Path.GetDirectoryName(path)) && isPrimaryButtonClick)
                {
                    Ui.Toast(MainLang.FolderNotExist, Const.Notification.main, NotificationType.Error);
                    return null;
                }

                return path;
            }
        }

        public static async Task<bool> UploadMicrosoftSkin(string skinpath, string uuid, string model,
            string accessToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // 设置Authorization头部  
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    // 创建multipart/form-data内容  
                    using (var formData = new MultipartFormDataContent())
                    {
                        // 添加model部分  
                        formData.Add(new StringContent(model), "model");

                        // 添加file部分  
                        var fileStream = File.OpenRead(skinpath);
                        var fileContent = new StreamContent(fileStream);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                        formData.Add(fileContent, "file", Path.GetFileName(skinpath));

                        // 发送PUT请求  
                        var response = await client.PutAsync($"https://api.mojang.com/user/profile/{uuid}/skin",
                            formData);

                        // 检查响应状态  
                        if (response.IsSuccessStatusCode)
                        {
                            Ui.Toast(MainLang.ModifySuccess);
                            return true;
                        }

                        Ui.Toast(MainLang.ModifyFail);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Ui.ShowShortException(MainLang.ModifyFail, ex);
                return false;
            }
        }

        public static FileInfo GetFileInfoFromPath(string path)
        {
            if (!File.Exists(path)) return null;
            var fullName = Path.GetFileName(path);
            var name = Path.GetFileNameWithoutExtension(fullName);
            var extension = Path.GetExtension(fullName);
            return new FileInfo { Name = name, Path = path, FullName = fullName, Extension = extension };
        }

        public static void CallEnabledPlugin()
        {
            var list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
            var list1 = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.PluginDataPath));
            var directoryInfo = new DirectoryInfo(Const.String.PluginFolderPath);
            var dlls = directoryInfo.GetFiles();
            var paths = new List<string>();
            foreach (var item in dlls) paths.Add(item.FullName);
            list1.ForEach(x =>
            {
                if (!paths.Contains(x)) list.Remove(x);
            });
            File.WriteAllText(Const.String.PluginDataPath, JsonConvert.SerializeObject(list, Formatting.Indented));
            foreach (var item in dlls)
            {
                if (list.Contains(item.FullName))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(item.FullName);
                        var manifestModuleName = asm.ManifestModule.ScopeName;
                        var type = asm.GetType("YMCL.Plugin.Main");
                        if (!typeof(IPlugin).IsAssignableFrom(type))
                        {
                            Console.WriteLine("未继承插件接口");
                            continue;
                        }

                        var instance = Activator.CreateInstance(type) as IPlugin;
                        var protocolInfo = instance.GetPluginInformation();
                        if (list.Contains(item.FullName)) instance.OnLaunch();
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static void ClearFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine(MainLang.FolderNotExist + folderPath);
                return;
            }

            foreach (string file in Directory.GetFiles(folderPath))
            {
                File.Delete(file);
            }

            foreach (string dir in Directory.GetDirectories(folderPath))
            {
                ClearFolder(dir);
                Directory.Delete(dir);
            }
        }

        public static void CopyDirectory(string sourceDir, string destinationDir)
        {
            // 确保目标目录不存在时创建它  
            TryCreateFolder(destinationDir);

            // 获取源目录中的所有文件和子目录  
            foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                // 获取目录名称（不包含路径）  
                var dirName = Path.GetFileName(dirPath);

                // 在目标目录中创建相同的目录结构  
                var destDirPath = Path.Combine(destinationDir, dirName);
                Directory.CreateDirectory(destDirPath);
            }

            // 复制文件  
            foreach (var newPath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                // 获取文件的名称（不包含路径）  
                var fileName = Path.GetFileName(newPath);

                // 构建目标路径  
                var destPath = newPath.Replace(sourceDir, destinationDir);

                // 确保目标文件的目录存在  
                TryCreateFolder(Path.GetDirectoryName(destPath)!);

                // 复制文件  
                File.Copy(newPath, destPath, true);
            }
        }

        public static void TryCreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                var directoryInfo = new DirectoryInfo(path);
                directoryInfo.Create();
            }
        }
    }

    public static class Value
    {
        public static Color ColorVariant(Color color, float percent)
        {
            // 确保百分比在-1到1之间  
            percent = Math.Max(-1f, Math.Min(1f, percent));

            // 计算调整后的RGB值  
            var adjust = 1f + percent; // 亮化是1+percent，暗化是1+(negative percent)，即小于1  
            var r = (int)Math.Round(color.R * adjust);
            var g = (int)Math.Round(color.G * adjust);
            var b = (int)Math.Round(color.B * adjust);

            // 确保RGB值在有效范围内  
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            // 创建一个新的颜色（保持Alpha通道不变）  
            return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }

        public static string GetTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString(); //精确到秒
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            var imageBytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(imageBytes))
            {
                var bitmap = new Bitmap(ms);
                return bitmap;
            }
        }

        public static string BytesToBase64(byte[] imageBytes)
        {
            var base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        public static string MsToTime(double ms) //转换为分秒格式
        {
            var minute = 0;
            var second = 0;
            second = (int)(ms / 1000);

            var secondStr = string.Empty;
            var minuteStr = string.Empty;

            if (second > 60)
            {
                minute = second / 60;
                second = second % 60;
            }

            secondStr = second < 10 ? $"0{second}" : $"{second}";
            minuteStr = minute < 10 ? $"0{minute}" : $"{minute}";

            return $"{minuteStr}:{secondStr}";
        }

        public static double GetTotalMemory(Platform platform)
        {
            if (platform == Platform.Windows)
                try
                {
                    long totalMemory = 0;
                    using (var searcher =
                           new ManagementObjectSearcher("select TotalVisibleMemorySize from Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject share in searcher.Get())
                            totalMemory = Convert.ToInt64(share["TotalVisibleMemorySize"]);
                    }

                    Console.WriteLine("系统最大内存: " + totalMemory);

                    return totalMemory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取系统内存信息时发生错误: " + ex.Message);
                    return 0;
                }

            if (platform == Platform.Linux)
                // 尝试读取 /proc/meminfo 文件  
                try
                {
                    var meminfo = File.ReadAllText("/proc/meminfo");

                    // 使用 LINQ 查询来找到 "MemTotal" 行  
                    var memTotalLine = meminfo.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .FirstOrDefault(line => line.StartsWith("MemTotal:"));

                    // 如果找到 MemTotal 行，解析其值  
                    if (memTotalLine != null)
                    {
                        // 提取 MemTotal 后面的数字，并转换为长整型  
                        var memTotalValueStr = memTotalLine.Split(':')[1].Trim().Split(' ')[0];
                        var memTotalValue = long.Parse(memTotalValueStr);

                        return memTotalValue;
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading /proc/meminfo: " + ex.Message);
                    return 0;
                }

            return 0;
        }

        public static double GetDirectoryLength(string dirPath)
        {
            //判断给定的路径是否存在,如果不存在则退出
            if (!Directory.Exists(dirPath))
                return 0;
            double len = 0;

            //定义一个DirectoryInfo对象
            var di = new DirectoryInfo(dirPath);

            //通过GetFiles方法,获取di目录中的所有文件的大小
            foreach (var fi in di.GetFiles()) len += fi.Length;

            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            var dis = di.GetDirectories();
            if (dis.Length > 0)
                for (var i = 0; i < dis.Length; i++)
                    len += GetDirectoryLength(dis[i].FullName);
            return len;
        }

        public static string GetCurrentPlatformAndArchitecture()
        {
            // 检测操作系统  
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // 检测架构  
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "win-x64";
                if (RuntimeInformation.ProcessArchitecture == Architecture.X86) return "win-x86";
                // 其他的 Windows 架构可能也需要处理（比如 ARM）  
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "osx-x64";
                // 注意: OSX 上的 ARM 架构可能需要特定检测，因为目前可能是 Apple Silicon (M1/M2)  
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) return "osx-arm64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "linux-x64";
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                    return "linux-arm"; // 注意: 这里应该是 linux-arm 而不是 liux-arm  
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) return "linux-arm64";
            }
            // 其他操作系统可能需要额外处理  

            // 如果没有匹配项，返回未知或默认字符串  
            return "unknown";
        }

        public static async Task<Bitmap> LoadImageFromUrlAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var imageData = await client.GetByteArrayAsync(url);
                    using (var stream = new MemoryStream(imageData))
                    {
                        var bitmap = new Bitmap(stream);
                        return bitmap;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static string ConvertToWanOrYi(double number)
        {
            if (number < 7500)
                return number.ToString();
            if (number < 100000000)
                return $"{Math.Round(number / 10000, 1)}万";
            return $"{Math.Round(number / 100000000, 1)}亿";
        }
    }

    public static class Mc
    {
        public static VersionSetting GetVersionSetting(GameEntry entry)
        {
            if (entry == null)
            {
                Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
                return null;
            }

            var filePath = Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.String.VersionSettingFileName);
            if (!File.Exists(filePath))
                File.WriteAllText(filePath, JsonConvert.SerializeObject(new VersionSetting(), Formatting.Indented));
            var versionSetting = JsonConvert.DeserializeObject<VersionSetting>(File.ReadAllText(filePath));
            return versionSetting;
        }

        public static async Task<bool> InstallClientUsingMinecraftLaunchAsync(string versionId, string customId = null,
            ForgeInstallEntry forgeInstallEntry = null, FabricBuildEntry fabricBuildEntry = null,
            QuiltBuildEntry quiltBuildEntry = null, OptiFineInstallEntity optiFineInstallEntity = null,
            WindowTask p_task = null, bool closeTask = true)
        {
            var shouldReturn = false;
            var regex = new Regex(@"[\\/:*?""<>|]");
            var matches = regex.Matches(customId);
            if (matches.Count > 0)
            {
                var str = string.Empty;
                foreach (Match match in matches) str += match.Value;
                Ui.Toast($"{MainLang.IncludeSpecialWord}: {str}", Const.Notification.main,
                    NotificationType.Error);
                return false;
            }

            var setting = Const.Data.Setting;
            var resolver = new GameResolver(setting.MinecraftFolder);
            var vanlliaInstaller = new VanlliaInstaller(resolver, versionId, MirrorDownloadManager.Bmcl);
            if (Directory.Exists(Path.Combine(setting.MinecraftFolder, "versions", customId)))
            {
                Ui.Toast($"{MainLang.FolderAlreadyExists}: {customId}", Const.Notification.main,
                    NotificationType.Error);
                return false;
            }

            MirrorDownloadManager.IsUseMirrorDownloadSource = setting.DownloadSource == DownloadSource.BmclApi;

            Const.Window.main.downloadPage.autoInstallPage.InstallPreviewRoot.IsVisible = false;
            Const.Window.main.downloadPage.autoInstallPage.InstallableVersionListRoot.IsVisible = true;

            var task = p_task == null ? new WindowTask($"{MainLang.Install}: Vanllia - {versionId}") : p_task;
            task.UpdateTextProgress("-----> Vanllia", false);

            //Vanllia
            await Task.Run(async () =>
            {
                try
                {
                    vanlliaInstaller.ProgressChanged += (_, x) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTextProgress(x.ProgressStatus);
                            task.UpdateValueProgress(x.Progress * 100);
                        });
                    };

                    var result = await vanlliaInstaller.InstallAsync();

                    if (!result)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.Toast($"{MainLang.InstallFail}: Vanllia - {versionId}", Const.Notification.main,
                                NotificationType.Error);
                        });
                        shouldReturn = true;
                    }
                    else
                    {
                        if (forgeInstallEntry == null && quiltBuildEntry == null && fabricBuildEntry == null)
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFinish}: Vanllia - {versionId}",
                                    Const.Notification.main, NotificationType.Success);
                            });
                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Ui.ShowShortException($"{MainLang.InstallFail}: Vanllia - {versionId}", ex);
                    });
                    shouldReturn = true;
                }
            });
            if (shouldReturn) return false;

            //Forge
            if (forgeInstallEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(
                            File.ReadAllText(Const.String.JavaDataPath));
                        if (javas.Count <= 0)
                        {
                            Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                            shouldReturn = true;
                        }
                        else
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var forgeInstaller = new ForgeInstaller(game, forgeInstallEntry, javas[0].JavaPath,
                                customId, MirrorDownloadManager.Bmcl);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTitle($"{MainLang.Install}: Forge - {versionId}");
                                task.UpdateTextProgress("-----> Forge", false);
                            });
                            forgeInstaller.ProgressChanged += (_, x) =>
                            {
                                Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.UpdateTextProgress(x.ProgressStatus);
                                    task.UpdateValueProgress(x.Progress * 100);
                                });
                            };

                            var result = await forgeInstaller.InstallAsync();

                            if (result)
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Ui.Toast($"{MainLang.InstallFinish}: Forge - {versionId}",
                                        Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Ui.Toast($"{MainLang.InstallFail}: Forge - {customId}", Const.Notification.main,
                                        NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: Forge - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            //OptiFine
            if (optiFineInstallEntity != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(
                            File.ReadAllText(Const.String.JavaDataPath));
                        if (javas.Count <= 0)
                        {
                            Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                            shouldReturn = true;
                        }
                        else
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var optifineInstaller = new OptifineInstaller(game, optiFineInstallEntity,
                                javas[0].JavaPath, customId, MirrorDownloadManager.Bmcl);
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTitle($"{MainLang.Install}: OptiFine - {versionId}");
                                task.UpdateTextProgress("-----> OptiFine", false);
                            });
                            optifineInstaller.ProgressChanged += (_, x) =>
                            {
                                Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.UpdateTextProgress(x.ProgressStatus);
                                    task.UpdateValueProgress(x.Progress * 100);
                                });
                            };

                            var result = await optifineInstaller.InstallAsync();

                            if (result)
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Ui.Toast($"{MainLang.InstallFinish}: OptiFine - {versionId}",
                                        Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Ui.Toast($"{MainLang.InstallFail}: OptiFine - {customId}",
                                        Const.Notification.main, NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: OptiFine - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            //Fabric
            if (fabricBuildEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var game = resolver.GetGameEntity(versionId);
                        var fabricInstaller =
                            new FabricInstaller(game, fabricBuildEntry, customId, MirrorDownloadManager.Bmcl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTitle($"{MainLang.Install}: Fabric - {versionId}");
                            task.UpdateTextProgress("-----> Fabric", false);
                        });
                        fabricInstaller.ProgressChanged += (_, x) =>
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(x.ProgressStatus);
                                task.UpdateValueProgress(x.Progress * 100);
                            });
                        };

                        var result = await fabricInstaller.InstallAsync();

                        if (result)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFinish}: Fabric - {versionId}", Const.Notification.main,
                                    NotificationType.Success);
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFail}: Fabric - {customId}", Const.Notification.main,
                                    NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: Fabric - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            //Quilt
            if (quiltBuildEntry != null)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var game = resolver.GetGameEntity(versionId);
                        var quiltInstaller =
                            new QuiltInstaller(game, quiltBuildEntry, customId, MirrorDownloadManager.Bmcl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTitle($"{MainLang.Install}: Quilt - {versionId}");
                            task.UpdateTextProgress("-----> Quilt", false);
                        });
                        quiltInstaller.ProgressChanged += (_, x) =>
                        {
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(x.ProgressStatus);
                                task.UpdateValueProgress(x.Progress * 100);
                            });
                        };

                        var result = await quiltInstaller.InstallAsync();

                        if (result)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFinish}: Quilt - {versionId}", Const.Notification.main,
                                    NotificationType.Success);
                            });
                        }
                        else
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Ui.Toast($"{MainLang.InstallFail}: Quilt - {customId}", Const.Notification.main,
                                    NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Ui.ShowShortException($"{MainLang.InstallFail}: Quilt - {customId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) return false;
            }

            Const.Window.main.Activate();
            if (closeTask) task.Destory();
            return true;
        }

        public static async Task<bool> LaunchClientUsingMinecraftLaunchAsync(string p_id = "", string p_javaPath = "",
            string p_mcPath = "",
            double p_maxMem = -1, string p_enableIndependencyCore = "unset", string p_fullUrl = "")
        {
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = false;
            GameEntry gameEntry = null;
            Account account = null;
            var l_id = string.Empty;
            var l_ip = string.Empty;
            var l_port = 25565;
            var l_javaPath = string.Empty;
            var l_mcPath = string.Empty;
            double l_maxMem = -1;
            var l_enableIndependencyCore = true;

            var setting = Const.Data.Setting;
            if (string.IsNullOrEmpty(p_id))
            {
                if (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry != null)
                {
                    l_id = (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry).Id;
                }
                else
                {
                    Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    return false;
                }
            }
            else
            {
                l_id = p_id;
            }

            if (string.IsNullOrEmpty(p_mcPath))
                l_mcPath = setting.MinecraftFolder;
            else
                l_mcPath = p_mcPath;
            IGameResolver gameResolver = new GameResolver(l_mcPath);
            gameEntry = gameResolver.GetGameEntity(l_id);
            if (gameEntry == null)
            {
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Ui.Toast(MainLang.CreateGameEntryFail, Const.Notification.main, NotificationType.Error);
                return false;
            }

            var versionSetting = GetVersionSetting(gameEntry);
            var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath));
            if (javas.Count == 0)
            {
                Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return false;
            }

            if (string.IsNullOrEmpty(p_javaPath))
            {
                if (versionSetting.Java.JavaPath == "Global")
                {
                    if (setting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = setting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        return false;
                    }
                }
                else
                {
                    if (versionSetting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = versionSetting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        return false;
                    }
                }
            }
            else
            {
                l_javaPath = p_javaPath;
            }

            if (p_maxMem == -1)
            {
                if (versionSetting.MaxMem == -1)
                    l_maxMem = setting.MaxMem;
                else
                    l_maxMem = versionSetting.MaxMem;
            }
            else
            {
                l_maxMem = p_maxMem;
            }

            if (p_enableIndependencyCore == "unset")
            {
                if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Global)
                {
                    l_enableIndependencyCore = setting.EnableIndependencyCore;
                }
                else
                {
                    if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Off)
                        l_enableIndependencyCore = false;
                }
            }
            else
            {
                if (p_enableIndependencyCore == "false" || p_enableIndependencyCore == "False")
                    l_enableIndependencyCore = true;
                else
                    l_enableIndependencyCore = false;
            }

            if (string.IsNullOrEmpty(p_fullUrl))
            {
                if (!string.IsNullOrEmpty(versionSetting.AutoJoinServerIp))
                {
                    if (versionSetting.AutoJoinServerIp.Contains(':'))
                    {
                        var arr = versionSetting.AutoJoinServerIp.Split(':');
                        l_ip = arr[0];
                        l_port = Convert.ToInt16(arr[1]);
                    }
                    else
                    {
                        l_ip = versionSetting.AutoJoinServerIp;
                        l_port = 25565;
                    }
                }
            }
            else
            {
                if (p_fullUrl.Contains(':'))
                {
                    var arr = p_fullUrl.Split(':');
                    l_ip = arr[0];
                    l_port = Convert.ToInt16(arr[1]);
                }
                else
                {
                    l_ip = versionSetting.AutoJoinServerIp;
                    l_port = 25565;
                }
            }

            var task = new WindowTask($"{MainLang.Launch} - {gameEntry.Id}", false);
            task.UpdateTextProgress("-----> YMCL", false);
            task.UpdateTextProgress(MainLang.VerifyingAccount);

            var accountData =
                JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath))[
                    setting.AccountSelectionIndex];
            if (accountData == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            switch (accountData.AccountType)
            {
                case AccountType.Offline:
                    if (!string.IsNullOrEmpty(accountData.Name))
                    {
                        OfflineAuthenticator authenticator1 = new(accountData.Name);
                        account = authenticator1.Authenticate();
                    }
                    else
                    {
                        Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        return false;
                    }

                    break;
                case AccountType.Microsoft:
                    var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(accountData.Data!);
                    MicrosoftAuthenticator authenticator2 = new(profile, Const.String.AzureClientId, true);
                    try
                    {
                        account = await authenticator2.AuthenticateAsync();
                    }
                    catch (Exception ex)
                    {
                        Ui.ShowShortException(MainLang.LoginFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        return false;
                    }

                    break;
                case AccountType.ThirdParty:
                    account = JsonConvert.DeserializeObject<YggdrasilAccount>(accountData.Data!);
                    break;
            }

            if (account == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            if (string.IsNullOrEmpty(l_id) ||
                string.IsNullOrEmpty(l_mcPath) ||
                string.IsNullOrEmpty(l_javaPath) ||
                l_maxMem == -1)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            var config = new LaunchConfig
            {
                Account = account,
                JvmConfig = new JvmConfig(l_javaPath)
                {
                    MaxMemory = Convert.ToInt32(l_maxMem)
                },
                IsEnableIndependencyCore = l_enableIndependencyCore,
                LauncherName = "YMCL",
                ServerConfig = new ServerConfig(l_port, l_ip)
            };
            if (config == null)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            Launcher launcher = new(gameResolver, config);

            await Task.Run(async () =>
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var watcher = await launcher.LaunchAsync(l_id);
                        _ = Task.Run(() => { IO.CallEnabledPlugin(); });

                        watcher.Exited += async (_, args) =>
                        {
                            await Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                if (setting.LauncherVisibility !=
                                    LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                                {
                                    Const.Window.main.Show();
                                    Const.Window.main.WindowState = WindowState.Normal;
                                    Const.Window.main.Activate();
                                }

                                Ui.Toast($"{MainLang.GameExited}: {args.ExitCode}", Const.Notification.main);

                                if (args.ExitCode == 0)
                                {
                                    await Task.Delay(2000);
                                    task.Destory();
                                    Const.Window.main.Focus();
                                }
                                else
                                {
                                    var crashAnalyzer = new GameCrashAnalyzer(gameEntry, l_enableIndependencyCore);
                                    var reports = crashAnalyzer.AnalysisLogs();
                                    var msg = string.Empty;
                                    try
                                    {
                                        if (reports == null || reports.Count() == 0)
                                            msg = MainLang.NoCrashInfo;
                                        else
                                            foreach (var report in reports)
                                                msg += $"\n{report.CrashCauses}";
                                    }
                                    catch
                                    {
                                        msg = MainLang.NoCrashInfo;
                                    }

                                    task.UpdateTextProgress(string.Empty, false);
                                    task.UpdateTextProgress($"YMCL -----> {MainLang.MineratCrashed}");
                                    task.isFinish = true;

                                    var dialogResult = await Ui.ShowDialogAsync(MainLang.MineratCrashed, msg,
                                        b_primary: MainLang.Ok);
                                    task.Destory();
                                }
                            });
                        };
                        watcher.OutputLogReceived += async (_, args) =>
                        {
                            Console.WriteLine(args.Log);
                            if (setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.UpdateTextProgress(args.Original, false);
                                });
                            else
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.entry.UpdateTextProgress(args.Original, false);
                                });
                        };

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTextProgress(MainLang.WaitForGameWindowAppear);
                            if (setting.ShowGameOutput)
                            {
                                task.UpdateTextProgress("\n", false);
                                task.UpdateTextProgress("-----> JvmOutputLog", false);
                            }

                            Ui.Toast(MainLang.LaunchFinish, Const.Notification.main, NotificationType.Success);
                        });
                        _ = Task.Run(async () =>
                        {
                            watcher.Process.WaitForInputIdle();

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                switch (setting.LauncherVisibility)
                                {
                                    case LauncherVisibility.AfterLaunchExitLauncher:
                                        Environment.Exit(0);
                                        break;
                                    case LauncherVisibility.AfterLaunchMakeLauncherMinimize:
                                    case LauncherVisibility.AfterLaunchMinimizeAndShowWhenGameExit:
                                        Const.Window.main.WindowState = WindowState.Minimized;
                                        break;
                                    case LauncherVisibility.AfterLaunchHideAndShowWhenGameExit:
                                        Const.Window.main.Hide();
                                        break;
                                    case LauncherVisibility.AfterLaunchKeepLauncherVisible:
                                    default:
                                        break;
                                }
                            });

                            if (!setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.Hide(); });
                        });
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Ui.ShowShortException(MainLang.LaunchFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                    });
                }
            });
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
            return true;
        }

        public static async Task<bool> LaunchClientUsingStarLightAsync(string p_id = "", string p_javaPath = "",
            string p_mcPath = "",
            double p_maxMem = -1, string p_enableIndependencyCore = "unset", string p_fullUrl = "")
        {
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = false;
            GameEntry gameEntry = null;
            BaseAccount account = null;
            var l_id = string.Empty;
            var l_ip = string.Empty;
            var l_port = 25565;
            var l_javaPath = string.Empty;
            var l_mcPath = string.Empty;
            double l_maxMem = -1;
            var l_enableIndependencyCore = true;

            var setting = Const.Data.Setting;
            if (string.IsNullOrEmpty(p_id))
            {
                if (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry != null)
                {
                    l_id = (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry).Id;
                }
                else
                {
                    Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    return false;
                }
            }
            else
            {
                l_id = p_id;
            }

            if (string.IsNullOrEmpty(p_mcPath))
                l_mcPath = setting.MinecraftFolder;
            else
                l_mcPath = p_mcPath;
            IGameResolver gameResolver = new GameResolver(l_mcPath);
            gameEntry = gameResolver.GetGameEntity(l_id);
            if (gameEntry == null)
            {
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                Ui.Toast(MainLang.CreateGameEntryFail, Const.Notification.main, NotificationType.Error);
                return false;
            }

            var versionSetting = GetVersionSetting(gameEntry);
            var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath));
            if (javas.Count == 0)
            {
                Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return false;
            }

            if (string.IsNullOrEmpty(p_javaPath))
            {
                if (versionSetting.Java.JavaPath == "Global")
                {
                    if (setting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = setting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        return false;
                    }
                }
                else
                {
                    if (versionSetting.Java.JavaPath == "Auto")
                    {
                        var javaEntry = JavaUtil.GetCurrentJava(
                            JsonConvert.DeserializeObject<List<JavaEntry>>(
                                File.ReadAllText(Const.String.JavaDataPath))!,
                            gameEntry);
                        l_javaPath = javaEntry.JavaPath;
                    }
                    else
                    {
                        l_javaPath = versionSetting.Java.JavaPath;
                    }

                    if (l_javaPath == "Auto")
                    {
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                        return false;
                    }
                }
            }
            else
            {
                l_javaPath = p_javaPath;
            }

            if (p_maxMem == -1)
            {
                if (versionSetting.MaxMem == -1)
                    l_maxMem = setting.MaxMem;
                else
                    l_maxMem = versionSetting.MaxMem;
            }
            else
            {
                l_maxMem = p_maxMem;
            }

            if (p_enableIndependencyCore == "unset")
            {
                if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Global)
                {
                    l_enableIndependencyCore = setting.EnableIndependencyCore;
                }
                else
                {
                    if (versionSetting.EnableIndependencyCore == VersionSettingEnableIndependencyCore.Off)
                        l_enableIndependencyCore = false;
                }
            }
            else
            {
                if (p_enableIndependencyCore == "false" || p_enableIndependencyCore == "False")
                    l_enableIndependencyCore = true;
                else
                    l_enableIndependencyCore = false;
            }

            if (string.IsNullOrEmpty(p_fullUrl))
            {
                if (!string.IsNullOrEmpty(versionSetting.AutoJoinServerIp))
                {
                    if (versionSetting.AutoJoinServerIp.Contains(':'))
                    {
                        var arr = versionSetting.AutoJoinServerIp.Split(':');
                        l_ip = arr[0];
                        l_port = Convert.ToInt16(arr[1]);
                    }
                    else
                    {
                        l_ip = versionSetting.AutoJoinServerIp;
                        l_port = 25565;
                    }
                }
            }
            else
            {
                if (p_fullUrl.Contains(':'))
                {
                    var arr = p_fullUrl.Split(':');
                    l_ip = arr[0];
                    l_port = Convert.ToInt16(arr[1]);
                }
                else
                {
                    l_ip = versionSetting.AutoJoinServerIp;
                    l_port = 25565;
                }
            }

            var task = new WindowTask($"{MainLang.Launch} - {gameEntry.Id}", false);
            task.UpdateTextProgress("-----> YMCL", false);
            task.UpdateTextProgress(MainLang.VerifyingAccount);

            var accountData =
                JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.String.AccountDataPath))[
                    setting.AccountSelectionIndex];
            if (accountData == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }


            switch (accountData.AccountType)
            {
                case AccountType.Offline:
                    if (!string.IsNullOrEmpty(accountData.Name))
                    {
                        account = new OfflineAuthentication(accountData.Name).OfflineAuth();
                    }
                    else
                    {
                        Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        return false;
                    }

                    break;
                case AccountType.Microsoft:
                    var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(accountData.Data!);
                    var entry = new MicrosoftAuthentication(Const.String.AzureClientId);
                    try
                    {
                        account = await entry.MicrosoftAuthAsync(new GetTokenResponse()
                        {
                            AccessToken = profile.AccessToken, RefreshToken = profile.RefreshToken,
                            ClientId = Const.String.AzureClientId
                        }, progress => { task.UpdateTextProgress(progress); }, profile.RefreshToken);
                    }
                    catch (Exception ex)
                    {
                        Ui.ShowShortException(MainLang.LoginFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                        return false;
                    }

                    break;
                case AccountType.ThirdParty:
                    var profile1 = JsonConvert.DeserializeObject<YggdrasilAccount>(accountData.Data!);
                    account = new StarLight_Core.Models.Authentication.YggdrasilAccount()
                    {
                        ServerUrl = profile1.YggdrasilServerUrl, ClientToken = profile1.ClientToken,
                        Name = profile1.Name, Uuid = profile1.Uuid.ToString(), AccessToken = profile1.AccessToken
                    };
                    break;
            }

            if (account == null)
            {
                Ui.Toast(MainLang.AccountError, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            if (string.IsNullOrEmpty(l_id) ||
                string.IsNullOrEmpty(l_mcPath) ||
                string.IsNullOrEmpty(l_javaPath) ||
                l_maxMem == -1)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            var config = new StarLight_Core.Models.Launch.LaunchConfig()
            {
                GameCoreConfig = new GameCoreConfig()
                {
                    Root = l_mcPath,
                    Version = l_id,
                    Ip = l_ip,
                    Port = l_port.ToString(),
                    IsVersionIsolation = l_enableIndependencyCore
                },
                Account = new StarLight_Core.Models.Authentication.Account() { BaseAccount = account },
                JavaConfig = new JavaConfig()
                {
                    JavaPath = l_javaPath,
                    MaxMemory = Convert.ToInt32(l_maxMem)
                }
            };

            if (config == null)
            {
                Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, NotificationType.Error);
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                task.Destory();
                return false;
            }

            var launcher = new MinecraftLauncher(config);

            await Task.Run(async () =>
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var watcher = await launcher.LaunchAsync(async x =>
                        {
                            Console.WriteLine(x.Description);
                            if (setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.UpdateTextProgress(x.Description, true);
                                });
                            else
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.entry.UpdateTextProgress(x.Description, true);
                                });
                        });

                        _ = Task.Run(() => { IO.CallEnabledPlugin(); });

                        watcher.OutputReceived += async a =>
                        {
                            Console.WriteLine(a);
                            if (setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.UpdateTextProgress(a, true); });
                            else
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.entry.UpdateTextProgress(a, true);
                                });
                        };
                        watcher.ErrorReceived += async a =>
                        {
                            Console.WriteLine(a);
                            if (setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.UpdateTextProgress(a, true); });
                            else
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    task.entry.UpdateTextProgress(a, true);
                                });
                        };

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            task.UpdateTextProgress(MainLang.WaitForGameWindowAppear);
                            if (setting.ShowGameOutput)
                            {
                                task.UpdateTextProgress("\n", false);
                                task.UpdateTextProgress("-----> JvmOutputLog", false);
                            }

                            Ui.Toast(MainLang.LaunchFinish, Const.Notification.main, NotificationType.Success);
                        });
                        _ = Task.Run(async () =>
                        {
                            watcher.Process.WaitForInputIdle();

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                switch (setting.LauncherVisibility)
                                {
                                    case LauncherVisibility.AfterLaunchExitLauncher:
                                        Environment.Exit(0);
                                        break;
                                    case LauncherVisibility.AfterLaunchMakeLauncherMinimize:
                                    case LauncherVisibility.AfterLaunchMinimizeAndShowWhenGameExit:
                                        Const.Window.main.WindowState = WindowState.Minimized;
                                        break;
                                    case LauncherVisibility.AfterLaunchHideAndShowWhenGameExit:
                                        Const.Window.main.Hide();
                                        break;
                                    case LauncherVisibility.AfterLaunchKeepLauncherVisible:
                                    default:
                                        break;
                                }
                            });

                            if (!setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.Hide(); });
                        });
                        _ = Task.Run(async () =>
                        {
                            watcher.Process.Exited += async (a, b) =>
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    if (setting.LauncherVisibility !=
                                        LauncherVisibility.AfterLaunchMakeLauncherMinimize)
                                    {
                                        Const.Window.main.Show();
                                        Const.Window.main.WindowState = WindowState.Normal;
                                        Const.Window.main.Activate();
                                    }

                                    Ui.Toast($"{MainLang.GameExited}", Const.Notification.main);
                                    task.Destory();
                                });
                            };
                            if (!setting.ShowGameOutput)
                                await Dispatcher.UIThread.InvokeAsync(() => { task.Hide(); });
                        });
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Ui.ShowShortException(MainLang.LaunchFail, ex);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        task.Destory();
                    });
                }
            });
            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
            return true;
        }

        public static async Task<bool> ImportModPackFromLocal(string path, bool confirmBox = true,
            string p_customId = null)
        {
            var setting = Const.Data.Setting;
            var customId = string.Empty;
            while (true)
            {
                if (!confirmBox) break;
                var textBox = new TextBox
                {
                    TextWrapping = TextWrapping.Wrap, FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Text = Path.GetFileNameWithoutExtension(path)
                };
                var dialog = new ContentDialog
                {
                    PrimaryButtonText = MainLang.Ok,
                    Content = textBox,
                    DefaultButton = ContentDialogButton.Primary,
                    CloseButtonText = MainLang.Cancel,
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = $"{MainLang.Install} - {Path.GetFileName(path)}"
                };
                var dialogResult = await dialog.ShowAsync(Const.Window.main);
                if (dialogResult == ContentDialogResult.Primary)
                {
                    if (Directory.Exists(Path.Combine(setting.MinecraftFolder, "versions", textBox.Text)))
                    {
                        Ui.Toast($"{MainLang.FolderAlreadyExists}: {textBox.Text}", Const.Notification.main,
                            NotificationType.Error);
                    }
                    else
                    {
                        customId = textBox.Text;
                        break;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(customId))
            {
                customId = p_customId;
            }

            var task = new WindowTask($"{MainLang.Unzip} - {Path.GetFileName(path)}");

            IO.TryCreateFolder(Path.Combine(setting.MinecraftFolder, "YMCLTemp"));
            var unzipDirectory =
                Path.Combine(setting.MinecraftFolder, "YMCLTemp", Path.GetFileNameWithoutExtension(path)); //确定临时整合包的路径
            task.UpdateTextProgress(MainLang.UnzipingModPack);
            task.UpdateValueProgress(50);
            await Task.Run(() => { ZipFile.ExtractToDirectory(path /*Zip文件路径*/, unzipDirectory /*要解压到的目录*/, true); });
            task.UpdateValueProgress(100);
            task.UpdateTextProgress(MainLang.FinsihUnzipModPack);
            task.UpdateTextProgress(MainLang.GetModPackInfo);
            var json = File.ReadAllText(Path.Combine(unzipDirectory, "manifest.json")); //read json
            var info = JsonConvert.DeserializeObject<ModPackEntry.Root>(json);
            task.UpdateTextProgress(
                $"{MainLang.ModPackInfo}:\n    Name : \t\t\t{info.name}\n    Author : \t\t\t{info.author}\n    Version : \t\t\t{info.version}\n    McVersion : \t\t\t{info.minecraft.version}\n    Loader : \t\t\t{info.minecraft.modLoaders[0].id}");
            task.UpdateTitle($"{MainLang.Install} - {Path.GetFileName(path)}");
            var loaders = info.minecraft.modLoaders[0].id.Split('-');
            var result = false;
            if (loaders[0] == "forge")
            {
                var forges = (await ForgeInstaller.EnumerableFromVersionAsync(info.minecraft.version)).ToList();
                ForgeInstallEntry enrty = null;
                foreach (var forge in forges)
                    if (forge.ForgeVersion == loaders[1])
                    {
                        enrty = forge;
                        break;
                    }

                if (enrty == null) return false;
                result = await InstallClientUsingMinecraftLaunchAsync(info.minecraft.version, customId, p_task: task,
                    forgeInstallEntry: enrty, closeTask: false);
            }
            else if (loaders[0] == "fabric")
            {
                var fabrics = (await FabricInstaller.EnumerableFromVersionAsync(info.minecraft.version)).ToList();
                FabricBuildEntry enrty = null;
                foreach (var fabric in fabrics)
                    if (fabric.BuildVersion == loaders[1])
                    {
                        enrty = fabric;
                        break;
                    }

                if (enrty == null) return false;
                result = await InstallClientUsingMinecraftLaunchAsync(info.minecraft.version, customId, p_task: task,
                    fabricBuildEntry: enrty, closeTask: false);
            }
            else
            {
                task.Destory();
                return false;
            }

            if (!result) return false;
            task.Activate();

            var semaphore = new SemaphoreSlim(20); // 允许同时运行的下载任务数  
            var completedDownloads = 0; // 已完成下载的文件数量  
            var successDownloads = 0; // 成功下载的文件数量
            var totalDownloads = info.files.Count; // 总下载文件数量  
            ApiClient cfApiClient = new(Const.String.CurseForgeApiKey); // 创建一个CurseForge API 客户端
            var tasks = new List<Task>(); // 创建一个任务列表来存储下载任务
            var errors = new List<string>(); // 创建一个列表来存储下载错误

            if (info.files.Count > 0)
            {
                task.UpdateTitle(MainLang.DownloadModPackMod);
                task.UpdateTextProgress(MainLang.DownloadModPackMod);
                info.files.ForEach(file => { tasks.Add(GetAndDownloadMod(file.projectID, file.fileID)); });
                await Task.WhenAll(tasks);

                task.UpdateTextProgress("", false);
                task.UpdateTextProgress($"{MainLang.TotalNumberOfMod}: {totalDownloads}");
                task.UpdateTextProgress($"{MainLang.DownloadSuccess}: {successDownloads}");
                var text = string.Empty;
                errors.ForEach(error => text += error + "\n");
                task.UpdateTextProgress($"{MainLang.DownloadFail} ({totalDownloads - successDownloads}): \n{text}");

                var index = 1;
                var replaceUrl = true;
                while (true)
                {
                    if (index > 7) break;
                    task.UpdateTextProgress("", false);
                    task.UpdateTextProgress(MainLang.DownloadFailedFileAgain + $": {index}");
                    var redownload = errors;
                    totalDownloads = redownload.Count;
                    tasks.Clear();
                    successDownloads = 0;
                    redownload.ForEach(file =>
                    {
                        var dl = file;
                        if (replaceUrl) dl = dl.Replace("mediafilez.forgecdn.net", "edge.forgecdn.net");
                        tasks.Add(GetAndDownloadMod(url: dl));
                    });
                    if (replaceUrl)
                        replaceUrl = false;
                    else
                        replaceUrl = true;
                    errors.Clear();
                    await Task.WhenAll(tasks);
                    task.UpdateTextProgress($"{MainLang.TotalNumberOfMod}: {redownload.Count}");
                    task.UpdateTextProgress($"{MainLang.DownloadSuccess}: {successDownloads}");
                    text = string.Empty;
                    errors.ForEach(error => text += error + "\n");
                    task.UpdateTextProgress(
                        $"{MainLang.DownloadFail} ({redownload.Count - successDownloads}): \n{text}");
                    if (errors.Count == 0) break;
                    index++;
                }
            }

            if (!string.IsNullOrEmpty(info.overrides))
            {
                task.UpdateTitle(MainLang.OverrideModPack);
                IO.CopyDirectory(Path.Combine(unzipDirectory, info.overrides),
                    Path.Combine(setting.MinecraftFolder, "versions", customId));
            }

            async Task GetAndDownloadMod(int projectId = -1, int fileId = -1, string url = null)
            {
                await semaphore.WaitAsync();
                var modFileDownloadUrl = string.Empty;
                var fileName = string.Empty;
                try
                {
                    if (url == null)
                        modFileDownloadUrl =
                            (await cfApiClient.GetModFileDownloadUrlAsync(projectId, fileId)).Data.Replace(
                                "edge.forgecdn.net", "mediafilez.forgecdn.net");
                    else
                        modFileDownloadUrl = url;

                    var saveDirectory = Path.Combine(setting.MinecraftFolder, "versions", customId, "mods");
                    IO.TryCreateFolder(saveDirectory);
                    if (string.IsNullOrEmpty(modFileDownloadUrl)) throw new Exception("Failed to get download URL");

                    var uri = new Uri(modFileDownloadUrl);
                    fileName = Path.GetFileName(uri.AbsolutePath);
                    var savePath = Path.Combine(saveDirectory, fileName);
                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(modFileDownloadUrl,
                            HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                               fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None,
                                   4096, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }

                        successDownloads++;
                    }

                    Interlocked.Increment(ref completedDownloads);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        task.UpdateValueProgress(completedDownloads / (double)totalDownloads * 100);
                        task.UpdateTextProgress($"{MainLang.DownloadFinish}: {fileName}");
                    });
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref completedDownloads);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        task.UpdateValueProgress(completedDownloads / (double)totalDownloads * 100);
                        task.UpdateTextProgress($"{MainLang.DownloadFail}: {fileName}");
                    });
                    errors.Add(modFileDownloadUrl);
                }
                finally
                {
                    semaphore.Release();
                }
            }

            task.Destory();
            return true;
        }

        public static async Task ImportModPackFromCurseForge(ModFileListViewItemEntry item, string customId)
        {
            var shouldReturn = false;
            var fN = item.DisplayName;
            if (Path.GetExtension(fN) != ".zip") fN += ".zip";
            var path = Path.Combine(Const.String.TempFolderPath, fN);
            var task = new TaskEntry($"{MainLang.Download} - {fN}", true, false);
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(
                               item.DownloadUrl.Replace("edge.forgecdn.net", "mediafilez.forgecdn.net"),
                               HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                                   FileShare.None, 4096, true))
                        {
                            var buffer = new byte[4096];
                            var totalBytesRead = 0L;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                                var progressPercentage = (int)(totalBytesRead * 100 / totalBytes);
                                task.UpdateValueProgress(progressPercentage);
                            }

                            Method.Ui.Toast($"{MainLang.DownloadFinish}: {item.DisplayName}");
                            task.Destory();
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using (var response = await httpClient.GetAsync(item.DownloadUrl,
                                   HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode(); // 确保HTTP成功状态值  

                            var totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write,
                                       FileShare.None, 4096, true))
                            {
                                var buffer = new byte[4096];
                                var totalBytesRead = 0L;
                                int bytesRead;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;
                                    var progressPercentage = (int)(totalBytesRead * 100 / totalBytes);
                                    task.UpdateValueProgress(progressPercentage);
                                }

                                Method.Ui.Toast($"{MainLang.DownloadFinish}: {item.DisplayName}");
                                task.Destory();
                            }
                        }
                    }
                }
                catch
                {
                    shouldReturn = true;
                    Method.Ui.Toast($"{MainLang.DownloadFail}: {item.DisplayName}");
                    task.Destory();
                }
            }

            if (shouldReturn) return;
            ImportModPackFromLocal(path, false, customId);
        }
    }

    public class Debouncer
    {
        private Timer _timer;
        private Action _action;
        private double _interval = 1000;

        public Debouncer(Action action, double interval = 1000)
        {
            _action = action;
            _interval = interval;
            _timer = new Timer(_interval);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = false;
        }

        public void Trigger()
        {
            _timer.Stop();
            _timer.Start();
        }

        private void OnTimerElapsed(Object source, ElapsedEventArgs e)
        {
            _timer.Stop();
            _action?.Invoke();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}