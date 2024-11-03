using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using HtmlAgilityPack;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls.TaskManage;
using YMCL.Main.Public.Langs;
using Path = System.IO.Path;
using PathIcon = YMCL.Main.Public.Controls.PathIcon;

namespace YMCL.Main.Views.Main.Pages.Download.Pages.Java;

public partial class JavaDownloader : UserControl
{
    public JavaDownloader()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += async (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
    }

    public async Task LoadJava()
    {
        var list = new List<JavaDownloaderEntry>
        {
            new()
            {
                Version = "8",
                Url = "https://d.injdk.cn/d/download/openjdk/8/openjdk-8u43-windows-i586.zip",
            },
            new()
            {
                Version = "8",
                Url = "https://d.injdk.cn/d/download/openjdk/8/openjdk-8u43-linux-x64.tar.gz"
            },
            new()
            {
                Version = "11",
                Url = "https://d.injdk.cn/d/download/openjdk/11/openjdk-11+28_linux-x64_bin.tar.gz"
            },
            new()
            {
                Version = "11",
                Url = "https://d.injdk.cn/d/download/openjdk/11/openjdk-11+28_windows-x64_bin.zip"
            },
            new()
            {
                Version = "17",
                Url = "https://d.injdk.cn/d/download/openjdk/17/openjdk-17.0.1_linux-x64_bin.tar.gz"
            },
            new()
            {
                Version = "17",
                Url = "https://d.injdk.cn/d/download/openjdk/17/openjdk-17.0.1_macos-x64_bin.tar.gz"
            },
            new()
            {
                Version = "17",
                Url = "https://d.injdk.cn/d/download/openjdk/17/openjdk-17.0.1_windows-x64_bin.zip"
            },
            new()
            {
                Version = "22",
                Url = "https://d.injdk.cn/d/download/openjdk/22/openjdk-22.0.2_linux-x64_bin.tar.gz"
            },
            new()
            {
                Version = "22",
                Url = "https://d.injdk.cn/d/download/openjdk/22/openjdk-22.0.2_macos-x64_bin.tar.gz"
            },
            new()
            {
                Version = "22",
                Url = "https://d.injdk.cn/d/download/openjdk/22/openjdk-22.0.2_windows-x64_bin.zip"
            },
        };

        list.ForEach(java =>
        {
            if (string.IsNullOrWhiteSpace(java.FileName))
            {
                java.FileName = Path.GetFileName(java.Url);
            }

            switch (java.Version)
            {
                case "8":
                    Java8.Items.Add(java);
                    break;
                case "11":
                    Java11.Items.Add(java);
                    break;
                case "17":
                    Java17.Items.Add(java);
                    break;
                case "22":
                    Java22.Items.Add(java);
                    break;
            }
        });


        return;
        try
        {
            Container.Children.Clear();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            HttpResponseMessage response = await client.GetAsync("http://mirrors.lzu.edu.cn/openjdk/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(responseBody);

            var nodes = doc.DocumentNode.SelectNodes("//table/tbody/tr/td[@class='link']");

            if (nodes != null)
            {
                // 提取版本号和URL，跳过第一个节点
                var versions = nodes.Skip(1).Select(node =>
                {
                    string url = node.ChildNodes[0].Attributes["href"].Value;
                    string text = node.InnerText.Replace("/", ""); // 去除版本号中的斜杠
                    return (Version: NormalizeVersion(text), Url: url); // 标准化版本号
                }).Where(item => item.Version != null).ToList(); // 过滤掉无法标准化的版本号

                // 根据版本号从新到旧排序
                versions.Sort((a, b) =>
                {
                    var versionA = new Version(a.Version);
                    var versionB = new Version(b.Version);
                    return versionB.CompareTo(versionA); // 降序排序
                });

                // 输出排序后的结果
                foreach (var item in versions)
                {
                    Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant, out var c);
                    var bg = c as SolidColorBrush;
                    var root = new Border()
                    {
                        Height = 40,
                        CornerRadius = new CornerRadius(5),
                        ClipToBounds = true, Background = bg,
                        // Child = new Grid()
                        // {
                        //     Children =
                        //     {
                        //         new TextBlock()
                        //         {
                        //             Text = item.Version, Margin = new Thickness(10, 0, 0, 0),
                        //             VerticalAlignment = VerticalAlignment.Center
                        //         },
                        //         new PathIcon()
                        //         {
                        //             Height = 13, Width = 15, VerticalAlignment = VerticalAlignment.Center,
                        //             HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 5, 0),
                        //             Path =
                        //                 "M384,170.5C395.6669921875,170.5,405.6669921875,174.66700744628906,414,183L713,482C721.3330078125,490.3330078125 725.5,500.3330078125 725.5,512 725.5,523.6669921875 721.3330078125,533.6669921875 713,542L414,841C405.6669921875,849.3330078125 395.6669921875,853.5 384,853.5 372,853.5 361.9169921875,849.25 353.75,840.75 345.5830078125,832.25 341.5,822.1669921875 341.5,810.5 341.5,798.8330078125 345.6669921875,788.8330078125 354,780.5L622.5,512 354,243.5C345.6669921875,235.16700744628906 341.5,225.16700744628906 341.5,213.5 341.5,201.83299255371094 345.5830078125,191.75 353.75,183.25 361.9169921875,174.75 372,170.5 384,170.5z"
                        //         }
                        //     }
                        // }
                    };
                    Container.Children.Add(root);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // 标准化版本号，确保至少有两个部分
        static string NormalizeVersion(string version)
        {
            try
            {
                // 尝试解析版本号，如果失败则返回null
                var v = new System.Version(version);
                return v.ToString();
            }
            catch
            {
                // 如果版本号不符合格式，则返回null
                return null;
            }
        }
    }

    private void ControlProperty()
    {
    }

    private async void JavaOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var item = ((ListBox)sender).SelectedItem as JavaDownloaderEntry;
        if (item == null) return;

        var dialog = await Method.Ui.ShowDialogAsync(MainLang.Install,
            MainLang.SureToInstallTheJava.Replace("{Java}", item.FileName), b_primary: MainLang.Ok,
            b_cancel: MainLang.Cancel);
        if (dialog != ContentDialogResult.Primary) return;

        var task = new TaskManager.TaskEntry($"{MainLang.Download} : {item.FileName}");
        Method.IO.TryCreateFolder(Const.String.TempFolderPath);
        task.UpdateTextProgress($"{MainLang.BeginDownload} : {item.FileName}");

        try
        {
            string destinationPath = Path.Combine(Const.String.TempFolderPath, item.FileName);

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
            using (var response = await client.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                await using (var downloadStream = await response.Content.ReadAsStreamAsync())
                {
                    await using (var fileStream = new FileStream(
                                     Path.Combine(Const.String.UpdateFolderPath, destinationPath), FileMode.Create,
                                     FileAccess.Write))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        var totalBytes = response.Content.Headers.ContentLength.HasValue
                            ? response.Content.Headers.ContentLength.Value
                            : -1;

                        while ((bytesRead = await downloadStream.ReadAsync(buffer)) > 0)
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

            Dispatcher.UIThread.Invoke(() =>
            {
                task.UpdateTextProgress($"{MainLang.DownloadFinish} : {item.FileName}");
                task.UpdateTextProgress($"{MainLang.BeginInstall} : {item.FileName}");
            });

            Method.IO.TryCreateFolder(Path.Combine(Const.String.UserDataRootPath, "Java"));
            var path = Path.Combine(Const.String.UserDataRootPath, "Java",
                Path.GetFileNameWithoutExtension(destinationPath));
            Method.IO.TryCreateFolder(path);
            ZipFile.ExtractToDirectory(destinationPath, path);
            Dispatcher.UIThread.Invoke(() =>
            {
                task.UpdateTextProgress($"{MainLang.InstallFinish} : {item.FileName}");
                Const.Notification.main.Show($"{MainLang.InstallFinish} : {item.FileName}");
            });
            _ = Const.Window.main.settingPage.launchSettingPage.AutoSacnJava();
            task.Destory();
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Const.Notification.main.Show($"{MainLang.InstallFail} : {item.FileName}\n{ex.Message}",
                    NotificationType.Error);
                task.Destory();
            });
        }
    }
}