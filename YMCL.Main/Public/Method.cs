using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Auth;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Classes.Models.Launch;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Launcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.WindowTask;
using YMCL.Main.Public.Langs;
using MinecraftLaunch;

using File = System.IO.File;
using FileInfo = YMCL.Main.Public.Classes.FileInfo;
using Path = System.IO.Path;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Classes.Models.Install;
using System.Text.RegularExpressions;
using MinecraftLaunch.Components.Analyzer;
using CurseForge.APIClient;
using System.Threading;
using Flurl;
using CurseForge.APIClient.Models.Files;

namespace YMCL.Main.Public
{
    public class Method
    {
        public static class Ui
        {
            public static async void PageLoadAnimation((double, double, double, double) original, (double, double, double, double) target, TimeSpan time, Control control, bool visibility = false)
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
                        Property = Avalonia.Layout.Layoutable.MarginProperty
                    });
                    control.Transitions.Add(new DoubleTransition
                    {
                        Duration = time,
                        Easing = new SineEaseInOut(),
                        Property = Visual.OpacityProperty
                    });
                    if (visibility)
                    {
                        control.IsVisible = true;
                    }
                    control.Margin = new Thickness(tl, tt, tr, tb);
                    control.Opacity = 1;
                    await Task.Delay(time);
                    control.Transitions = transitions;
                }
            }
            public static void Toast(string msg, WindowNotificationManager p_notification = null, NotificationType type = NotificationType.Information, bool time = true, string title = "Yu Minecraft Launcher")
            {
                var notification = p_notification == null ? Const.Notification.main : p_notification;
                var showTitle = Const.AppTitle;
                if (!string.IsNullOrEmpty(title))
                {
                    showTitle = title;
                }
                if (time)
                {
                    showTitle += $" - {DateTime.Now.ToString("HH:mm:ss")}";
                }
                notification.Show(new Notification(showTitle, msg, type));
            }
            public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = "Content", Control p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null, Window p_window = null)
            {
                var content = p_content == null ? new TextBox() { TextWrapping = TextWrapping.Wrap, IsReadOnly = true, FontFamily = (FontFamily)Application.Current.Resources["Font"], Text = msg } : p_content;
                var window = p_window == null ? Const.Window.main : p_window;
                var dialog = new ContentDialog()
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
                var textBox = new TextBox() { FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap, Text = $"{msg} - {ex.Message}\n\n{ex}", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, IsReadOnly = true };
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
                    var rd = (AvaloniaXamlLoader.Load(new Uri("avares://YMCL.Main/Public/Styles/LightTheme.axaml")) as ResourceDictionary)!;
                    Application.Current!.Resources.MergedDictionaries.Add(rd);
                    Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                }
                else if (theme == Theme.Dark)
                {
                    var rd = (AvaloniaXamlLoader.Load(new Uri("avares://YMCL.Main/Public/Styles/DarkTheme.axaml")) as ResourceDictionary)!;
                    Application.Current!.Resources.MergedDictionaries.Add(rd);
                    Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                }
            }
            public static void ShowShortException(string msg, Exception ex)
            {
                Toast($"{msg}\n{ex.Message}", Const.Notification.main, NotificationType.Error);
            }
            public static async Task<bool> UpgradeToAdministratorPrivilegesAsync(Window p_window = null)
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                var windwo = p_window == null ? Const.Window.main : p_window;
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    var result = await ShowDialogAsync(MainLang.UpgradeToAdministratorPrivileges, MainLang.UpgradeToAdministratorPrivilegesTip, b_primary: MainLang.Ok, b_secondary: MainLang.Cancel, p_window: windwo);
                    if (result == ContentDialogResult.Primary)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
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
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            public static void RestartApp()
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Process.GetCurrentProcess().MainModule.FileName!
                };
                Process.Start(startInfo);
                Environment.Exit(0);
            }
        }
        public static class IO
        {
            public static async Task<List<FolderInfo>> OpenFolderPicker(TopLevel topLevel = null, FolderPickerOpenOptions options = null)
            {
                var isPrimaryButtonClick = false;
                var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
                if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
                {
                    if (options != null && topLevel != null)
                    {
                        var storageProvider = topLevel!.StorageProvider;
                        var result = await storageProvider.OpenFolderPickerAsync(options);
                        var list = new List<FolderInfo>();
                        result.ToList().ForEach(item =>
                        {
                            list.Add(new FolderInfo() { Name = item.Name, Path = item.Path.LocalPath });
                        });
                        return list;
                    }
                    else
                    {
                        new Exception("ParameterIsNull");
                        return null;
                    }
                }
                else
                {
                    var textBox = new TextBox() { FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap };
                    ContentDialog dialog = new()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Title = MainLang.InputFolderPath,
                        PrimaryButtonText = MainLang.Ok,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = textBox
                    };
                    dialog.PrimaryButtonClick += (_, _) =>
                    {
                        isPrimaryButtonClick = true;
                    };
                    var result = await dialog.ShowAsync();
                    var path = textBox.Text;
                    if (!Directory.Exists(path) && isPrimaryButtonClick)
                    {
                        Ui.Toast(MainLang.FolderNotExist, Const.Notification.main, NotificationType.Error);
                        return null;
                    }
                    var folder = Path.GetFileName(path);
                    var list = new List<FolderInfo>() { new FolderInfo() { Name = folder, Path = path } };
                    return list;
                }
            }
            public static async Task<List<FileInfo>> OpenFilePicker(TopLevel topLevel = null, FilePickerOpenOptions options = null, string p_title = null)
            {
                var title = p_title == null ? MainLang.InputFilePath : p_title;
                var isPrimaryButtonClick = false;
                var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
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
                            list.Add(new FileInfo() { Name = name, Path = path, FullName = fullName, Extension = extension });
                        });
                        return list;
                    }
                    else
                    {
                        new Exception("ParameterIsNull");
                        return null;
                    }
                }
                else
                {
                    var textBox = new TextBox() { FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap };
                    ContentDialog dialog = new()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Title = title,
                        PrimaryButtonText = MainLang.Ok,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = textBox
                    };
                    dialog.PrimaryButtonClick += (_, _) =>
                    {
                        isPrimaryButtonClick = true;
                    };
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
                    var list = new List<FileInfo>() { new FileInfo() { Name = name, Path = path, FullName = fullName, Extension = extension } };
                    return list;
                }
            }
            public static async Task<string> SaveFilePicker(TopLevel topLevel = null, FilePickerSaveOptions options = null)
            {
                var isPrimaryButtonClick = false;
                var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
                if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
                {
                    if (options != null && topLevel != null)
                    {
                        var storageProvider = topLevel!.StorageProvider;
                        var result = await storageProvider.SaveFilePickerAsync(options);
                        if (result != null)
                        {
                            return result.TryGetLocalPath();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        new Exception("ParameterIsNull");
                        return null;
                    }
                }
                else
                {
                    var textBox = new TextBox() { FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap };
                    ContentDialog dialog = new()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Title = MainLang.InputFilePath,
                        PrimaryButtonText = MainLang.Ok,
                        CloseButtonText = MainLang.Cancel,
                        DefaultButton = ContentDialogButton.Primary,
                        Content = textBox
                    };
                    dialog.PrimaryButtonClick += (_, _) =>
                    {
                        isPrimaryButtonClick = true;
                    };
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
            public static async Task<bool> UploadMicrosoftSkin(string skinpath, string uuid, string model, string accessToken)
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
                            var fileStream = System.IO.File.OpenRead(skinpath);
                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                            formData.Add(fileContent, "file", Path.GetFileName(skinpath));

                            // 发送PUT请求  
                            var response = await client.PutAsync($"https://api.mojang.com/user/profile/{uuid}/skin", formData);

                            // 检查响应状态  
                            if (response.IsSuccessStatusCode)
                            {
                                Method.Ui.Toast(MainLang.ModifySuccess);
                                return true;
                            }
                            else
                            {
                                Method.Ui.Toast(MainLang.ModifyFail);
                                return false;
                            }
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
                return new FileInfo() { Name = name, Path = path, FullName = fullName, Extension = extension };
            }
            public static void CopyDirectory(string sourceDir, string destinationDir)
            {
                // 确保目标目录不存在时创建它  
                IO.TryCreateFolder(destinationDir);

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
                    IO.TryCreateFolder(Path.GetDirectoryName(destPath)!);

                    // 复制文件  
                    File.Copy(newPath, destPath, true);
                }
            }
            public static void TryCreateFolder(string path)
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
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
                float adjust = 1f + percent; // 亮化是1+percent，暗化是1+(negative percent)，即小于1  
                int r = (int)Math.Round(color.R * adjust);
                int g = (int)Math.Round(color.G * adjust);
                int b = (int)Math.Round(color.B * adjust);

                // 确保RGB值在有效范围内  
                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));

                // 创建一个新的颜色（保持Alpha通道不变）  
                return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
            }
            public static string GetTimeStamp()
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(ts.TotalSeconds).ToString();//精确到秒
            }
            public static Bitmap Base64ToBitmap(string base64)
            {
                byte[] imageBytes = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new Bitmap(ms);
                    return bitmap;
                }
            }
            public static string BytesToBase64(byte[] imageBytes)
            {
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            public static string MsToTime(double ms)//转换为分秒格式
            {
                int minute = 0;
                int second = 0;
                second = (int)(ms / 1000);

                string secondStr = string.Empty;
                string minuteStr = string.Empty;

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
                {
                    try
                    {
                        long totalMemory = 0;
                        using (var searcher = new ManagementObjectSearcher("select TotalVisibleMemorySize from Win32_OperatingSystem"))
                        {
                            foreach (ManagementObject share in searcher.Get())
                            {
                                totalMemory = Convert.ToInt64(share["TotalVisibleMemorySize"]);
                            }
                        }

                        Console.WriteLine("系统最大内存: " + totalMemory);

                        return totalMemory;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("获取系统内存信息时发生错误: " + ex.Message);
                        return 0;
                    }
                }
                else if (platform == Platform.Linux)
                {
                    // 尝试读取 /proc/meminfo 文件  
                    try
                    {
                        string meminfo = File.ReadAllText("/proc/meminfo");

                        // 使用 LINQ 查询来找到 "MemTotal" 行  
                        var memTotalLine = meminfo.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                            .FirstOrDefault(line => line.StartsWith("MemTotal:"));

                        // 如果找到 MemTotal 行，解析其值  
                        if (memTotalLine != null)
                        {
                            // 提取 MemTotal 后面的数字，并转换为长整型  
                            string memTotalValueStr = memTotalLine.Split(':')[1].Trim().Split(' ')[0];
                            long memTotalValue = long.Parse(memTotalValueStr);

                            return memTotalValue;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading /proc/meminfo: " + ex.Message);
                        return 0;
                    }
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
                DirectoryInfo di = new DirectoryInfo(dirPath);

                //通过GetFiles方法,获取di目录中的所有文件的大小
                foreach (var fi in di.GetFiles())
                {
                    len += fi.Length;
                }

                //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
                DirectoryInfo[] dis = di.GetDirectories();
                if (dis.Length > 0)
                {
                    for (int i = 0; i < dis.Length; i++)
                    {
                        len += GetDirectoryLength(dis[i].FullName);
                    }
                }
                return len;
            }
            public static string GetCurrentPlatformAndArchitecture()
            {
                // 检测操作系统  
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // 检测架构  
                    if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    {
                        return "win-x64";
                    }
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                    {
                        return "win-x86";
                    }
                    // 其他的 Windows 架构可能也需要处理（比如 ARM）  
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    {
                        return "osx-x64";
                    }
                    // 注意: OSX 上的 ARM 架构可能需要特定检测，因为目前可能是 Apple Silicon (M1/M2)  
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    {
                        return "osx-arm64";
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    {
                        return "linux-x64";
                    }
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                    {
                        return "linux-arm"; // 注意: 这里应该是 linux-arm 而不是 liux-arm  
                    }
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    {
                        return "linux-arm64";
                    }
                }
                // 其他操作系统可能需要额外处理  

                // 如果没有匹配项，返回未知或默认字符串  
                return "unknown";
            }
            public static async Task<Bitmap> LoadImageFromUrlAsync(string url)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] imageData = await client.GetByteArrayAsync(url);
                        using (var stream = new MemoryStream(imageData))
                        {
                            var bitmap = new Bitmap(stream);
                            return bitmap;
                        }
                    }
                }
                catch { return null; }
            }
            public static string ConvertToWanOrYi(double number)
            {
                if (number < 7500)
                {
                    return number.ToString();
                }
                else if (number < 100000000)
                {
                    return $"{Math.Round((double)number / 10000, 1)}万";
                }
                else
                {
                    return $"{Math.Round((double)number / 100000000, 1)}亿";
                }
            }
        }
        public static class Mc
        {
            public static VersionSetting GetVersionSetting(GameEntry entry)
            {
                if (entry == null)
                {
                    Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    return null;
                }
                var filePath = Path.Combine(Path.GetDirectoryName(entry.JarPath)!, Const.VersionSettingFileName);
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(new VersionSetting(), Formatting.Indented));
                }
                var versionSetting = JsonConvert.DeserializeObject<VersionSetting>(File.ReadAllText(filePath));
                return versionSetting;
            }
            public static async Task<bool> InstallClientAsync(string versionId, string customId = null, ForgeInstallEntry forgeInstallEntry = null, FabricBuildEntry fabricBuildEntry = null, QuiltBuildEntry quiltBuildEntry = null, OptiFineInstallEntity optiFineInstallEntity = null, WindowTask p_task = null, bool closeTask = true)
            {
                var shouldReturn = false;
                Regex regex = new Regex(@"[\\/:*?""<>|]");
                MatchCollection matches = regex.Matches(customId);
                if (matches.Count > 0)
                {
                    var str = string.Empty;
                    foreach (Match match in matches)
                    {
                        str += match.Value;
                    }

                    return false;
                }

                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                var resolver = new GameResolver(setting.MinecraftFolder);
                var vanlliaInstaller = new VanlliaInstaller(resolver, versionId, MirrorDownloadManager.Bmcl);
                if (Directory.Exists(Path.Combine(setting.MinecraftFolder, "versions", customId)))
                {
                    Method.Ui.Toast($"{MainLang.FolderAlreadyExists}: {customId}", Const.Notification.main, NotificationType.Error);
                    return false;
                }

                Const.Window.main.downloadPage.autoInstallPage.InstallPreviewRoot.IsVisible = false;
                Const.Window.main.downloadPage.autoInstallPage.InstallableVersionListRoot.IsVisible = true;

                var task = p_task == null ? new WindowTask($"{MainLang.Install}: Vanllia - {versionId}", true) : p_task;
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
                                Method.Ui.Toast($"{MainLang.InstallFail}: Vanllia - {versionId}", Const.Notification.main, NotificationType.Error);
                            });
                            shouldReturn = true;
                        }
                        else
                        {
                            if (forgeInstallEntry == null && quiltBuildEntry == null && fabricBuildEntry == null)
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFinish}: Vanllia - {versionId}", Const.Notification.main, NotificationType.Success);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.ShowShortException($"{MainLang.InstallFail}: Vanllia - {versionId}", ex);
                        });
                        shouldReturn = true;
                    }
                });
                if (shouldReturn) { return false; }

                //Forge
                if (forgeInstallEntry != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath));
                            if (javas.Count <= 0)
                            {
                                Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                                shouldReturn = true;
                            }
                            else
                            {
                                var game = resolver.GetGameEntity(versionId);
                                var forgeInstaller = new ForgeInstaller(game, forgeInstallEntry, javas[0].JavaPath, customId, MirrorDownloadManager.Bmcl);
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
                                        Method.Ui.Toast($"{MainLang.InstallFinish}: Forge - {versionId}", Const.Notification.main, NotificationType.Success);
                                    });
                                }
                                else
                                {
                                    await Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        Method.Ui.Toast($"{MainLang.InstallFail}: Forge - {customId}", Const.Notification.main, NotificationType.Error);
                                    });
                                    shouldReturn = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.ShowShortException($"{MainLang.InstallFail}: Forge - {customId}", ex);
                            });
                            shouldReturn = true;
                        }
                    });
                    if (shouldReturn) { return false; }
                }

                //OptiFine
                if (optiFineInstallEntity != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath));
                            if (javas.Count <= 0)
                            {
                                Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, NotificationType.Error);
                                shouldReturn = true;
                            }
                            else
                            {
                                var game = resolver.GetGameEntity(versionId);
                                var optifineInstaller = new OptifineInstaller(game, optiFineInstallEntity, javas[0].JavaPath, customId, MirrorDownloadManager.Bmcl);
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
                                        Method.Ui.Toast($"{MainLang.InstallFinish}: OptiFine - {versionId}", Const.Notification.main, NotificationType.Success);
                                    });
                                }
                                else
                                {
                                    await Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        Method.Ui.Toast($"{MainLang.InstallFail}: OptiFine - {customId}", Const.Notification.main, NotificationType.Error);
                                    });
                                    shouldReturn = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.ShowShortException($"{MainLang.InstallFail}: OptiFine - {customId}", ex);
                            });
                            shouldReturn = true;
                        }
                    });
                    if (shouldReturn) { return false; }
                }

                //Fabric
                if (fabricBuildEntry != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var fabricInstaller = new FabricInstaller(game, fabricBuildEntry, customId, MirrorDownloadManager.Bmcl);
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
                                    Method.Ui.Toast($"{MainLang.InstallFinish}: Fabric - {versionId}", Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFail}: Fabric - {customId}", Const.Notification.main, NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.ShowShortException($"{MainLang.InstallFail}: Fabric - {customId}", ex);
                            });
                            shouldReturn = true;
                        }
                    });
                    if (shouldReturn) { return false; }
                }

                //Quilt
                if (quiltBuildEntry != null)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var game = resolver.GetGameEntity(versionId);
                            var quiltInstaller = new QuiltInstaller(game, quiltBuildEntry, customId, MirrorDownloadManager.Bmcl);
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
                                    Method.Ui.Toast($"{MainLang.InstallFinish}: Quilt - {versionId}", Const.Notification.main, NotificationType.Success);
                                });
                            }
                            else
                            {
                                await Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    Method.Ui.Toast($"{MainLang.InstallFail}: Quilt - {customId}", Const.Notification.main, NotificationType.Error);
                                });
                                shouldReturn = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                Method.Ui.ShowShortException($"{MainLang.InstallFail}: Quilt - {customId}", ex);
                            });
                            shouldReturn = true;
                        }
                    });
                    if (shouldReturn) { return false; }
                }

                Const.Window.main.Activate();
                if (closeTask)
                {
                    task.Finish();
                    task.Hide();
                }
                return true;
            }
            public static async Task<bool> LaunchClientAsync(string p_id = "", string p_javaPath = "", string p_mcPath = "", double p_maxMem = -1, string p_enableIndependencyCore = "unset", string p_fullUrl = "")
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

                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (string.IsNullOrEmpty(p_id))
                {
                    if ((Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry) != null)
                    {
                        l_id = (Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry).Id;
                    }
                    else
                    {
                        Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                        Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                        return false;
                    }
                }
                else
                {
                    l_id = p_id;
                }
                if (string.IsNullOrEmpty(p_mcPath))
                {
                    l_mcPath = setting.MinecraftFolder;
                }
                else
                {
                    l_mcPath = p_mcPath;
                }
                IGameResolver gameResolver = new GameResolver(l_mcPath);
                gameEntry = gameResolver.GetGameEntity(l_id);
                if (gameEntry == null)
                {
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    Method.Ui.Toast(MainLang.CreateGameEntryFail, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    return false;
                }
                var versionSetting = GetVersionSetting(gameEntry);
                var javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath));
                if (javas.Count == 0)
                {
                    Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error); Const.Window.main.launchPage.LaunchBtn.IsEnabled = true; return false;
                }
                if (string.IsNullOrEmpty(p_javaPath))
                {
                    if (versionSetting.Java.JavaPath == "Global")
                    {
                        if (setting.Java.JavaPath == "Auto")
                        {
                            var javaEntry = JavaUtil.GetCurrentJava(JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath))!, gameEntry);
                            l_javaPath = javaEntry.JavaPath;
                        }
                        else
                        {
                            l_javaPath = setting.Java.JavaPath;
                        }
                        if (l_javaPath == "Auto")
                        {
                            Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error); Const.Window.main.launchPage.LaunchBtn.IsEnabled = true; return false;
                        }
                    }
                    else
                    {
                        if (versionSetting.Java.JavaPath == "Auto")
                        {
                            var javaEntry = JavaUtil.GetCurrentJava(JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath))!, gameEntry);
                            l_javaPath = javaEntry.JavaPath;
                        }
                        else
                        {
                            l_javaPath = versionSetting.Java.JavaPath;
                        }
                        if (l_javaPath == "Auto")
                        {
                            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                            Method.Ui.Toast(MainLang.CannotFandRightJava, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error); return false;
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
                    {
                        l_maxMem = setting.MaxMem;
                    }
                    else
                    {
                        l_maxMem = versionSetting.MaxMem;
                    }
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
                        {
                            l_enableIndependencyCore = false;
                        }
                    }
                }
                else
                {
                    if (p_enableIndependencyCore == "false" || p_enableIndependencyCore == "False")
                    {
                        l_enableIndependencyCore = true;
                    }
                    else
                    {
                        l_enableIndependencyCore = false;
                    }
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

                var task = new WindowTask(MainLang.LaunchProgress, false);
                task.UpdateTextProgress("-----> YMCL", false);
                task.UpdateTextProgress(MainLang.VerifyingAccount);

                var accountData = JsonConvert.DeserializeObject<List<AccountInfo>>(File.ReadAllText(Const.AccountDataPath))[setting.AccountSelectionIndex];
                if (accountData == null)
                {
                    Method.Ui.Toast(MainLang.AccountError, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    task.Hide();
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
                            Method.Ui.Toast(MainLang.AccountError, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                            task.Hide();
                            return false;
                        }
                        break;
                    case AccountType.Microsoft:
                        var profile = JsonConvert.DeserializeObject<MicrosoftAccount>(accountData.Data!);
                        MicrosoftAuthenticator authenticator2 = new(profile, Const.AzureClientId, true);
                        try
                        {
                            account = await authenticator2.AuthenticateAsync();
                        }
                        catch (Exception ex)
                        {
                            Method.Ui.ShowShortException(MainLang.LoginFail, ex);
                            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                            task.Hide();
                            return false;
                        }
                        break;
                    case AccountType.ThirdParty:
                        account = JsonConvert.DeserializeObject<YggdrasilAccount>(accountData.Data!);
                        break;
                }
                if (account == null)
                {
                    Method.Ui.Toast(MainLang.AccountError, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    task.Hide();
                    return false;
                }

                if (string.IsNullOrEmpty(l_id) ||
                string.IsNullOrEmpty(l_mcPath) ||
                string.IsNullOrEmpty(l_javaPath) ||
                l_maxMem == -1)
                {
                    Method.Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    task.Hide();
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
                    Method.Ui.Toast(MainLang.BuildLaunchConfigFail, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Error);
                    Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                    task.Hide();
                    return false;
                }

                Method.Ui.Toast($"java:{l_javaPath},mem:{l_maxMem},core:{l_enableIndependencyCore},mcPath:{l_mcPath}", Const.Notification.main);

                Launcher launcher = new(gameResolver, config);

                await Task.Run(async () =>
                {
                    try
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            var watcher = await launcher.LaunchAsync(l_id);

                            watcher.Exited += async (_, args) =>
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    Method.Ui.Toast($"{MainLang.GameExited}: {args.ExitCode}", Const.Notification.main, NotificationType.Information);

                                    if (args.ExitCode == 0)
                                    {
                                        await Task.Delay(2000);
                                        task.Hide();
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
                                            {
                                                msg = MainLang.NoCrashInfo;
                                            }
                                            else
                                            {
                                                foreach (var report in reports)
                                                {
                                                    msg += $"\n{report.CrashCauses}";
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            msg = MainLang.NoCrashInfo;
                                        }

                                        task.UpdateTextProgress(string.Empty, false);
                                        task.UpdateTextProgress($"YMCL -----> {MainLang.MineratCrashed}");
                                        task.isFinish = true;

                                        var dialogResult = await Method.Ui.ShowDialogAsync(MainLang.MineratCrashed, msg, b_primary: MainLang.Ok);
                                        task.Hide();
                                    }
                                });
                            };
                            watcher.OutputLogReceived += async (_, args) =>
                            {
                                Debug.WriteLine(args.Log);
                                if (setting.ShowGameOutput)
                                {
                                    await Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        task.UpdateTextProgress(args.Original, false);
                                    });
                                }
                            };

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                task.UpdateTextProgress(MainLang.WaitForGameWindowAppear);
                                if (setting.ShowGameOutput)
                                {
                                    task.UpdateTextProgress("\n", false);
                                    task.UpdateTextProgress("-----> JvmOutputLog", false);
                                }
                                Method.Ui.Toast(MainLang.LaunchFinish, Const.Notification.main, Avalonia.Controls.Notifications.NotificationType.Success);
                            });
                            _ = Task.Run(async () =>
                            {
                                watcher.Process.WaitForInputIdle();
                                if (!setting.ShowGameOutput)
                                {
                                    await Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        task.Hide();
                                    });
                                }
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Method.Ui.ShowShortException(MainLang.LaunchFail, ex);
                            Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                            task.Hide();
                        });
                    }
                });
                Const.Window.main.launchPage.LaunchBtn.IsEnabled = true;
                return true;
            }
            public static async Task<bool> ImportModPack(string path)
            {
                var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
                var customId = string.Empty;
                while (true)
                {
                    var textBox = new TextBox() { TextWrapping = TextWrapping.Wrap, FontFamily = (FontFamily)Application.Current.Resources["Font"], Text = Path.GetFileNameWithoutExtension(path) };
                    var dialog = new ContentDialog()
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
                            Method.Ui.Toast($"{MainLang.FolderAlreadyExists}: {textBox.Text}", Const.Notification.main, NotificationType.Error);
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

                var task = new WindowTask($"{MainLang.Unzip} - {Path.GetFileName(path)}");

                IO.TryCreateFolder(Path.Combine(setting.MinecraftFolder, "YMCLTemp"));
                var unzipDirectory = Path.Combine(setting.MinecraftFolder, "YMCLTemp", Path.GetFileNameWithoutExtension(path));//确定临时整合包的路径
                task.UpdateTextProgress(MainLang.UnzipingModPack);
                task.UpdateValueProgress(50);
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(path/*Zip文件路径*/, unzipDirectory/*要解压到的目录*/, true);
                });
                task.UpdateValueProgress(100);
                task.UpdateTextProgress(MainLang.FinsihUnzipModPack);
                task.UpdateTextProgress(MainLang.GetModPackInfo);
                var json = File.ReadAllText(Path.Combine(unzipDirectory, "manifest.json"));//read json
                var info = JsonConvert.DeserializeObject<ModPackEntry.Root>(json);
                task.UpdateTextProgress($"{MainLang.ModPackInfo}:\n    Name : \t\t\t{info.name}\n    Author : \t\t\t{info.author}\n    Version : \t\t\t{info.version}\n    McVersion : \t\t\t{info.minecraft.version}\n    Loader : \t\t\t{info.minecraft.modLoaders[0].id}");
                task.UpdateTitle($"{MainLang.Install} - {Path.GetFileName(path)}");
                var loaders = info.minecraft.modLoaders[0].id.Split('-');
                bool result = false;
                if (loaders[0] == "forge")// 0 加载器类型 1 版本
                {
                    var forges = (await ForgeInstaller.EnumerableFromVersionAsync(info.minecraft.version)).ToList();
                    ForgeInstallEntry enrty = null;
                    foreach (var forge in forges)
                    {
                        if (forge.ForgeVersion == loaders[1])
                        {
                            enrty = forge;
                            break;
                        }
                    }
                    if (enrty == null)
                    {
                        return false;
                    }
                    result = await InstallClientAsync(info.minecraft.version, customId: customId, p_task: task, forgeInstallEntry: enrty, closeTask: false);
                }
                else if (loaders[0] == "fabric")
                {
                    var fabrics = (await FabricInstaller.EnumerableFromVersionAsync(info.minecraft.version)).ToList();
                    FabricBuildEntry enrty = null;
                    foreach (var fabric in fabrics)
                    {
                        if (fabric.BuildVersion == loaders[1])
                        {
                            enrty = fabric;
                            break;
                        }
                    }
                    if (enrty == null)
                    {
                        return false;
                    }
                    result = await InstallClientAsync(info.minecraft.version, customId: customId, p_task: task, fabricBuildEntry: enrty, closeTask: false);
                }
                else
                {
                    return false;
                }
                if (!result) { return false; }
                task.Activate();

                SemaphoreSlim semaphore = new SemaphoreSlim(20); // 允许同时运行的下载任务数  
                int completedDownloads = 0; // 已完成下载的文件数量  
                int successDownloads = 0; // 成功下载的文件数量
                int totalDownloads = info.files.Count; // 总下载文件数量  
                ApiClient cfApiClient = new(Const.CurseForgeApiKey); // 创建一个CurseForge API 客户端
                var tasks = new List<Task>(); // 创建一个任务列表来存储下载任务
                var errors = new List<string>(); // 创建一个列表来存储下载错误

                if (info.files.Count > 0)
                {
                    task.UpdateTitle(MainLang.DownloadModPackMod);
                    task.UpdateTextProgress(MainLang.DownloadModPackMod);
                    info.files.ForEach(file =>
                    {
                        tasks.Add(GetAndDownloadMod(file.projectID, file.fileID));
                    });
                    await Task.WhenAll(tasks);

                    task.UpdateTextProgress($"", false);
                    task.UpdateTextProgress($"{MainLang.TotalNumberOfMod}: {totalDownloads}");
                    task.UpdateTextProgress($"{MainLang.DownloadSuccess}: {successDownloads}");
                    var text = string.Empty;
                    errors.ForEach(error => text += error + "\n");
                    task.UpdateTextProgress($"{MainLang.DownloadFail} ({totalDownloads - successDownloads}): \n{text}");

                    var index = 1;
                    var replaceUrl = true;
                    while (true)
                    {
                        if (index > 7)
                        {
                            break;
                        }
                        task.UpdateTextProgress($"", false);
                        task.UpdateTextProgress(MainLang.DownloadFailedFileAgain + $": {index}");
                        var redownload = errors;
                        totalDownloads = redownload.Count;
                        tasks.Clear();
                        successDownloads = 0;
                        redownload.ForEach(file =>
                        {
                            var dl = file;
                            if (replaceUrl)
                            {
                                dl = dl.Replace("mediafilez.forgecdn.net", "edge.forgecdn.net");
                            }
                            tasks.Add(GetAndDownloadMod(url: dl));
                        });
                        if (replaceUrl)
                        {
                            replaceUrl = false;
                        }
                        else
                        {
                            replaceUrl = true;
                        }
                        errors.Clear();
                        await Task.WhenAll(tasks);
                        task.UpdateTextProgress($"{MainLang.TotalNumberOfMod}: {redownload.Count}");
                        task.UpdateTextProgress($"{MainLang.DownloadSuccess}: {successDownloads}");
                        text = string.Empty;
                        errors.ForEach(error => text += error + "\n");
                        task.UpdateTextProgress($"{MainLang.DownloadFail} ({redownload.Count - successDownloads}): \n{text}");
                        if (errors.Count == 0)
                        {
                            break;
                        }
                        index++;
                    }
                }

                if (!string.IsNullOrEmpty(info.overrides))
                {
                    task.UpdateTitle(MainLang.OverrideModPack);
                    IO.CopyDirectory(Path.Combine(unzipDirectory, info.overrides), Path.Combine(setting.MinecraftFolder, "versions", customId));
                }

                async Task GetAndDownloadMod(int projectId = -1, int fileId = -1, string url = null)
                {
                    await semaphore.WaitAsync(); // 等待进入信号量 
                    string modFileDownloadUrl = string.Empty;
                    string fileName = string.Empty;
                    try
                    {
                        if (url == null)
                        {
                            modFileDownloadUrl = (await cfApiClient.GetModFileDownloadUrlAsync(projectId, fileId)).Data.Replace("edge.forgecdn.net", "mediafilez.forgecdn.net");
                        }
                        else
                        {
                            modFileDownloadUrl = url;
                        }

                        var saveDirectory = Path.Combine(setting.MinecraftFolder, "versions", customId, "mods");
                        Method.IO.TryCreateFolder(saveDirectory);
                        if (string.IsNullOrEmpty(modFileDownloadUrl))
                        {
                            throw new Exception("Failed to get download URL");
                        }
                        else
                        {
                            Uri uri = new Uri(modFileDownloadUrl);
                            fileName = Path.GetFileName(uri.AbsolutePath);
                            var savePath = Path.Combine(saveDirectory, fileName);

                            // 使用HttpClient下载文件  
                            using (HttpClient client = new HttpClient())
                            {
                                // 发送GET请求获取文件流  
                                HttpResponseMessage response = await client.GetAsync(modFileDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                                response.EnsureSuccessStatusCode(); // 确保HTTP成功状态值  

                                // 读取响应内容并保存到文件  
                                using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                                {
                                    // 写入文件  
                                    await contentStream.CopyToAsync(fileStream);
                                }
                                successDownloads++;
                            }
                            // 更新已完成下载的文件数量  
                            Interlocked.Increment(ref completedDownloads);

                            // 打印进度
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                task.UpdateValueProgress((double)(completedDownloads / (double)totalDownloads * 100));
                                task.UpdateTextProgress($"{MainLang.DownloadFinish}: {fileName}");
                            });
                        }
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref completedDownloads);
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            task.UpdateValueProgress((double)(completedDownloads / (double)totalDownloads * 100));
                            task.UpdateTextProgress($"{MainLang.DownloadFail}: {fileName}");
                        });
                        errors.Add(modFileDownloadUrl);
                    }
                    finally
                    {
                        semaphore.Release(); // 释放信号量  
                    }
                }
                task.Destory();
                return true;
            }
        }
    }
}
