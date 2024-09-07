using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Afdian.Sdk;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher;

public partial class LauncherSettingPage : UserControl
{
    public LauncherSettingPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    public async Task AutoUpdate()
    {
        if (!Const.Data.Setting.EnableAutoCheckUpdate) return;
        var updateAvailable = await Method.Ui.CheckUpdateAsync();
        if (!updateAvailable.Item1) return;
        if (!updateAvailable.Item2) return;
        if (Const.Data.Setting.SkipUpdateVersion == updateAvailable.Item3) return;
        ContentDialogResult dialog = ContentDialogResult.None;
        if (Environment.OSVersion.Version.Major < 10)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion,
                    $"{MainLang.WinSevenAutoUpdateTip.Replace("{url}", updateAvailable.Item4).Replace("{version}", updateAvailable.Item3)}"
                    , b_cancel: MainLang.Cancel, b_secondary: MainLang.SkipThisVersion,
                    b_primary: MainLang.OpenBrowser);
            });
        }
        else
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion,
                    $"{updateAvailable.Item3!}\n\n{updateAvailable.Item4}"
                    , b_cancel: MainLang.Cancel, b_secondary: MainLang.SkipThisVersion,
                    b_primary: MainLang.Update);
            });
        }

        if (dialog == ContentDialogResult.Primary)
        {
            if (Environment.OSVersion.Version.Major < 10)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var launcher = TopLevel.GetTopLevel(Const.Window.main).Launcher;
                    await launcher.LaunchUriAsync(new Uri(updateAvailable.Item4));
                });
            }
            else
            {
                var updateAppAsync = await Method.Ui.UpdateAppAsync();
                if (!updateAppAsync) Method.Ui.Toast(MainLang.UpdateFail);
            }
        }
        else if (dialog == ContentDialogResult.Secondary)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Const.Data.Setting.SkipUpdateVersion = updateAvailable.Item3;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
                Method.Ui.Toast(MainLang.SkipVersionTip.Replace("{version}", updateAvailable.Item3));
            });
        }
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            var lenth = Method.Value.GetDirectoryLength(Const.String.UserDataRootPath);
            var userDataSize = Math.Round(lenth / 1024, 2) >= 512
                ? $"{Math.Round(lenth / 1024 / 1024, 2)} Mib"
                : $"{Math.Round(lenth / 1024, 2)} Kib";
            UserDataSize.Text = userDataSize;
        };
        OpenUserDataFolderBtn.Click += async (s, e) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Const.String.UserDataRootPath));
        };
        CheckUpdateBtn.Click += async (_, _) =>
        {
            CheckUpdateBtn.IsEnabled = false;
            var ring = new ProgressRing();
            CheckUpdateBtn.Width = CheckUpdateBtn.Bounds.Width;
            CheckUpdateBtn.Content = ring;
            ring.Height = 17;
            ring.Width = 17;
            var updateAvailable = await Method.Ui.CheckUpdateAsync();
            if (!updateAvailable.Item1)
            {
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
                Method.Ui.Toast(MainLang.CheckUpdateFail);
                return;
            }

            if (!updateAvailable.Item2)
            {
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
                Method.Ui.Toast(MainLang.CurrentlyTheLatestVersion);
                return;
            }

            CheckUpdateBtn.IsEnabled = true;
            CheckUpdateBtn.Content = MainLang.CheckUpdate;
            ContentDialogResult dialog = ContentDialogResult.None;
            if (Environment.OSVersion.Version.Major < 10)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion,
                        $"{MainLang.WinSevenAutoUpdateTip.Replace("{url}", updateAvailable.Item4).Replace("{version}", updateAvailable.Item3)}"
                        , b_cancel: MainLang.Cancel,
                        b_primary: MainLang.OpenBrowser);
                });
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion,
                        $"{updateAvailable.Item3!}\n\n{updateAvailable.Item4}"
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
                        var launcher = TopLevel.GetTopLevel(Const.Window.main).Launcher;
                        await launcher.LaunchUriAsync(new Uri(updateAvailable.Item4));
                    });
                }
                else
                {
                    var updateAppAsync = await Method.Ui.UpdateAppAsync();
                    if (!updateAppAsync) Method.Ui.Toast(MainLang.UpdateFail);
                }
            }
            else if (dialog == ContentDialogResult.Secondary)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Const.Data.Setting.SkipUpdateVersion = updateAvailable.Item3;
                    File.WriteAllText(Const.String.SettingDataPath,
                        JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
                    Method.Ui.Toast(MainLang.SkipVersionTip.Replace("{version}", updateAvailable.Item3));
                });
            }
        };
    }

    private async void ControlProperty()
    {
        UserDataFolderPath.Text = Const.String.UserDataRootPath;
        var resourceName = "YMCL.Main.Public.Texts.DateTime.txt";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using (var reader = new StreamReader(stream!))
        {
            var result = reader.ReadToEnd();
            Version.Text = $"v{result.Trim()}";
        }

        await Task.Run(async () =>
        {
            AfdianClient afdianClient =
                new AfdianClient("5f710d20e0aa11edb6cf5254001e7c00", "FseYBK8u9Vvr7CJxhk4Dw6aMN5WcqgUf");
            var page = 1;
            List<AfdianSponsor.ListItem> list = new();
            while (true)
            {
                var json = afdianClient.QuerySponsor(page);
                if (string.IsNullOrWhiteSpace(json)) break;
                var data = JsonConvert.DeserializeObject<AfdianSponsor.Root>(json);
                if (data is not { ec: 200 } || data.data.list.Count == 0) break;
                data.data.list.ForEach(x =>
                {
                    list.Add(x);
                });
                if (page >= data.data.total_page) break;
                page++;
            }

            list.OrderBy(x => x.all_sum_amount).ToList().ForEach(x =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var block = new TextBlock()
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"], FontSize = 14,
                        TextWrapping = TextWrapping.Wrap, TextDecorations = null,
                        Foreground = new SolidColorBrush(Const.Data.Setting.AccentColor),
                        Text = $"{x.user.name} {MainLang.CNYSymbol}{x.all_sum_amount}"
                    };
                    var link = new HyperlinkButton()
                    {
                        Margin = new Thickness(3), Content = block
                    };
                    SponsorPanel.Children.Add(link);
                });
            });
        });
    }
}