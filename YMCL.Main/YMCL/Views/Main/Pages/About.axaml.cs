using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Afdian.Sdk;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module;

namespace YMCL.Views.Main.Pages;

public partial class About : UserControl
{
    public About()
    {
        InitializeComponent();
        InitViewData();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            var length = Public.Module.IO.Disk.Getter.GetDirectoryLength(ConfigPath.UserDataRootPath);
            var userDataSize = Math.Round(length / 1024, 2) >= 512
                ? $"{Math.Round(length / 1024 / 1024, 2)} Mib"
                : $"{Math.Round(length / 1024, 2)} Kib";
            UserDataSize.Text = userDataSize;
        };
        OpenUserDataFolderBtn.Click += async (_, _) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(ConfigPath.UserDataRootPath));
        };
        CheckUpdateBtn.Click += async (_, _) =>
        {
            CheckUpdateBtn.IsEnabled = false;
            var ring = new ProgressRing();
            CheckUpdateBtn.Width = CheckUpdateBtn.Bounds.Width;
            CheckUpdateBtn.Content = ring;
            ring.Height = 17;
            ring.Width = 17;
            var updateAvailable = await Public.Module.IO.Network.Update.CheckUpdateAsync();
            if (!updateAvailable.Success)
            {
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
                Notice(MainLang.CheckUpdateFail);
                return;
            }

            if (!updateAvailable.IsNeedUpdate)
            {
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
                Notice(MainLang.CurrentlyTheLatestVersion);
                return;
            }

            CheckUpdateBtn.IsEnabled = true;
            CheckUpdateBtn.Content = MainLang.CheckUpdate;
            var dialog = ContentDialogResult.None;
            if (Environment.OSVersion.Version.Major < 10)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    dialog = await ShowDialogAsync(MainLang.FoundNewVersion,
                        $"{MainLang.WinSevenAutoUpdateTip.Replace("{url}", updateAvailable.GithubUrl).Replace("{version}", updateAvailable.NewVersion)}"
                        , b_cancel: MainLang.Cancel,
                        b_primary: MainLang.OpenBrowser);
                });
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    dialog = await ShowDialogAsync(MainLang.FoundNewVersion,
                        $"{updateAvailable.NewVersion}\n\n{updateAvailable.GithubUrl}"
                        , b_cancel: MainLang.Cancel,
                        b_primary: MainLang.Update);
                });
            }

            if (dialog == ContentDialogResult.Primary)
            {
                if (Environment.OSVersion.Version.Major < 10)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var launcher = TopLevel.GetTopLevel(App.UiRoot).Launcher;
                        await launcher.LaunchUriAsync(new Uri(updateAvailable.GithubUrl));
                    });
                }
                else
                {
                    var updateAppAsync = await Public.Module.IO.Network.Update.UpdateAppAsync();
                    if (!updateAppAsync) Notice(MainLang.UpdateFail);
                }
            }
            else if (dialog == ContentDialogResult.Secondary)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Data.Setting.SkipUpdateVersion = updateAvailable.NewVersion;
                    Notice(MainLang.SkipVersionTip.Replace("{version}", updateAvailable.NewVersion));
                });
            }
        };
    }

    private async void InitViewData()
    {
        UserDataFolderPath.Text = ConfigPath.UserDataRootPath;
        const string resourceName = "YMCL.Public.Texts.DateTime.txt";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using (var reader = new StreamReader(stream!))
        {
            var result = await reader.ReadToEndAsync();
            Version.Text = $"v{result.Trim()}";
        }

        await System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                var afdianClient =
                    new AfdianClient("5f710d20e0aa11edb6cf5254001e7c00", "FseYBK8u9Vvr7CJxhk4Dw6aMN5WcqgUf");
                var page = 1;
                List<AfdianSponsor.ListItem> list = [];
                while (true)
                {
                    var json = await afdianClient.QuerySponsorAsync(page);
                    if (string.IsNullOrWhiteSpace(json)) break;
                    var data = JsonConvert.DeserializeObject<AfdianSponsor.Root>(json);
                    if (data is not { ec: 200 } || data.data.list.Count == 0) break;
                    data.data.list.ForEach(x => { list.Add(x); });
                    if (page >= data.data.total_page) break;
                    page++;
                }

                list.OrderBy(x => x.all_sum_amount).ToList().ForEach(x =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var block = new TextBlock
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                            TextWrapping = TextWrapping.Wrap, TextDecorations = null,
                            Foreground = new SolidColorBrush(Data.Setting.AccentColor),
                            Text = $"{x.user.name} ￥{x.all_sum_amount}"
                        };
                        var link = new HyperlinkButton
                        {
                            Margin = new Thickness(3), Content = block
                        };
                        SponsorPanel.Children.Add(link);
                    });
                });
            }
            catch
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var block = new TextBlock
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                        TextWrapping = TextWrapping.Wrap, TextDecorations = null,
                        Foreground = new SolidColorBrush(Data.Setting.AccentColor),
                        Text = MainLang.LoadFail
                    };
                    var link = new HyperlinkButton
                    {
                        Margin = new Thickness(3), Content = block
                    };
                    SponsorPanel.Children.Add(link);
                });
            }
        });
    }
}