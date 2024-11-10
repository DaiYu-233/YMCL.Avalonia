using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Downloader;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Classes.Models.Game;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.TaskManage;
using YMCL.Main.Public.Langs;
using FileInfo = YMCL.Main.Public.Classes.FileInfo;

namespace YMCL.Main.Public;

public abstract partial class Method
{
    // ReSharper disable once InconsistentNaming
    public static class IO
    {
        public static string GetMacAddress()
        {
            string macAddress = string.Empty;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress = ni.GetPhysicalAddress().ToString();
                    break;
                }
            }

            return macAddress.Replace("-", "").ToLower(); // 移除MAC地址中的"-"并转为小写
        }

        public static async Task DownloadFileAsync(string url, string file, TaskManager.TaskEntry? task = null)
        {
            var downloadOpt = new DownloadConfiguration()
            {
                ChunkCount = Const.Data.Setting.MaxFileFragmentation,
                ParallelDownload = true
            };
            var downloader = new DownloadService(downloadOpt);

            if (task != null)
            {
                downloader.DownloadProgressChanged += (sender, args) =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        task.UpdateValueProgress(args.ProgressPercentage);
                    });
                };
            }
            
            await downloader.DownloadFileTaskAsync(url, file);
        }
        public static async Task DownloadFileWithoutFileNameAsync(string url, DirectoryInfo file, TaskManager.TaskEntry? task = null)
        {
            var downloadOpt = new DownloadConfiguration()
            {
                ChunkCount = Const.Data.Setting.MaxFileFragmentation,
                ParallelDownload = true
            };
            var downloader = new DownloadService(downloadOpt);

            if (task != null)
            {
                downloader.DownloadProgressChanged += (sender, args) =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        task.UpdateValueProgress(args.ProgressPercentage);
                    });
                };
            }
            
            await downloader.DownloadFileTaskAsync(url, file);
        }

        public static Bitmap LoadBitmapFromAppFile(string uri)
        {
            var memoryStream = new MemoryStream();
            var stream = AssetLoader.Open(new Uri("resm:" + uri));
            stream!.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return new Bitmap(memoryStream);
        }

        public static async Task<string> TranslateStringAsync(string text, string lang = "zh-Hans", int timeout = 5)
        {
            string result = null;
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (_, _, _, _) => true;
                using var client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(timeout);
                client.DefaultRequestHeaders.Add("Authorization", Const.Data.TranslateToken);
                var json = $"[{{\"Text\": \"{text}\"}}]";
                var response =
                    await client.PostAsync(
                        $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={lang}&textType=plain",
                        new StringContent(json, Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                string translatedText =
                    ((JObject)JArray.Parse(responseContent)[0]["translations"][0])["text"].ToString();
                if (!string.IsNullOrWhiteSpace(translatedText))
                {
                    result = translatedText;
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        public static async Task HandleFileDrop(List<IStorageItem> items)
        {
            var files = new List<FileInfo>();
            items.ForEach(item =>
            {
                var path = item.TryGetLocalPath();
                if (Directory.Exists(path))
                {
                    var dirInfo = new DirectoryInfo(path);
                    var files1 = dirInfo.GetFiles();
                    foreach (var file in files1) files.Add(Method.IO.GetFileInfoFromPath(file.FullName));
                }
                else if (File.Exists(path))
                {
                    files.Add(Method.IO.GetFileInfoFromPath(path));
                }
            });

            if (files.Count == 0) return;
            var jarFile = new List<FileInfo>();
            var zipFile = new List<FileInfo>();
            var audioFile = new List<FileInfo>();
            files.ForEach(file =>
            {
                switch (file.Extension)
                {
                    case ".jar":
                        jarFile.Add(file);
                        break;
                    case ".zip":
                        zipFile.Add(file);
                        break;
                    case ".mp3":
                    case ".ogg":
                    case ".flac":
                    case ".wav":
                        audioFile.Add(file);
                        break;
                    default:
                        Method.Ui.Toast($"{MainLang.UnsupportedFileType} - {file.Extension}\n{file.Path}",
                            type: NotificationType.Error);
                        break;
                }
            });
            if (jarFile.Count > 0)
            {
                var entry = Const.Window.main.launchPage.VersionListView.SelectedItem as GameEntry;
                if (entry.Type == "BedRock")
                {
                    Method.Ui.Toast(MainLang.UnableToAddModsForBedrockEdition, type: NotificationType.Error);
                    return;
                }

                if (null == entry)
                {
                    Method.Ui.Toast(MainLang.NoChooseGameOrCannotFindGame, type: NotificationType.Error);
                    return;
                }

                Const.Window.main.Nav.SelectedItem = Const.Window.main.NavLaunch;
                var text = string.Empty;
                jarFile.ForEach(jar => { text += $"{jar.FullName}\n"; });
                var result = await Method.Ui.ShowDialogAsync(
                    MainLang.AddTheFollowingFilesAsModsToTheCurrentVersion + "?",
                    text, b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
                if (result == ContentDialogResult.Primary)
                {
                    Method.IO.TryCreateFolder(Path.Combine(Path.GetDirectoryName(entry.JarPath)!, "mods"));
                    jarFile.ForEach(jar =>
                    {
                        File.Copy(jar.Path, Path.Combine(Path.GetDirectoryName(entry.JarPath)!, "mods", jar.FullName),
                            true);
                    });
                    Method.Ui.Toast(MainLang.SuccessAdd, type: NotificationType.Success);
                }
            }

            if (zipFile.Count > 0)
            {
                Const.Window.main.Nav.SelectedItem = Const.Window.main.NavLaunch;
                var text = string.Empty;
                zipFile.ForEach(zip => { text += $"{zip.FullName}\n"; });
                var result = await Method.Ui.ShowDialogAsync(
                    MainLang.InstallTheFollowingFilesAsAnIntegrationPackageCurseforgeFormat + "?", text,
                    b_cancel: MainLang.Cancel, b_primary: MainLang.Ok);
                if (result == ContentDialogResult.Primary)
                    foreach (var file in zipFile)
                    {
                        var importResult = await Method.Mc.ImportModPackFromLocal(file.Path);
                        if (!importResult)
                            Method.Ui.Toast($"{MainLang.ImportFailed}: {file.FullName}", type: NotificationType.Error);
                        else
                            Method.Ui.Toast($"{MainLang.ImportSuccess}: {file.FullName}",
                                type: NotificationType.Success);
                    }
            }

            if (audioFile.Count > 0)
            {
                Const.Window.main.Nav.SelectedItem = Const.Window.main.NavMusic;
                foreach (var file in audioFile)
                    using (var reader = new MediaFoundationReader(file.Path))
                    {
                        var time = Method.Value.MsToTime(reader.TotalTime.TotalMilliseconds);
                        var song = new PlaySongListViewItemEntry
                        {
                            DisplayDuration = time,
                            Duration = reader.TotalTime.TotalMilliseconds,
                            Img = null,
                            SongName = file.Name,
                            Authors = file.Extension.TrimStart('.'),
                            Path = file.Path,
                            Type = PlaySongListViewItemEntry.PlaySongListViewItemEntryType.Local
                        };
                        Const.Window.main.musicPage.PlaySongList.Add(song);
                        Const.Window.main.musicPage.PlayListView.Items.Add(song);
                    }

                File.WriteAllText(Const.String.PlayerDataPath,
                    JsonConvert.SerializeObject(Const.Window.main.musicPage.PlaySongList, Formatting.Indented));
                Const.Window.main.musicPage.PlayListView.SelectedIndex =
                    Const.Window.main.musicPage.PlayListView.Items.Count - 1;
            }
        }

        public static async Task<List<FolderInfo>?> OpenFolderPicker(TopLevel topLevel = null,
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
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    TextWrapping = TextWrapping.Wrap
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
                var result = await dialog.ShowAsync(Const.Window.main);
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
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    TextWrapping = TextWrapping.Wrap
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
                var result = await dialog.ShowAsync(Const.Window.main);
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

        public static async Task<string> SaveFilePicker(TopLevel? topLevel = null, FilePickerSaveOptions? options = null)
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
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    TextWrapping = TextWrapping.Wrap
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
                var result = await dialog.ShowAsync(Const.Window.main);
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
                        if (!typeof(Plugin.IPlugin).IsAssignableFrom(type))
                        {
                            Console.WriteLine("未继承插件接口");
                            continue;
                        }

                        var instance = Activator.CreateInstance(type) as Plugin.IPlugin;
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

        public static async Task HandleTextDrop(string p_text)
        {
            var text = WebUtility.UrlDecode(p_text.Trim());
            if (!Regex.IsMatch(text, @"^authlib-injector")) return;
            var match = Regex.Match(text, @"https?://[^\s:]+");
            if (!match.Success) return;
            var url = match.Value;
            Const.Window.main.Nav.SelectedItem = Const.Window.main.NavSetting;
            Const.Window.main.settingPage.Nav.SelectedItem = Const.Window.main.settingPage.NavAccount;
            Const.Window.main.settingPage.accountSettingPage.YggdrasilLogin(server1: url);
        }
    }
}