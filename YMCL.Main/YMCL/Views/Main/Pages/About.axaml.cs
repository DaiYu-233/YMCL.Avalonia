﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Afdian.Sdk;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Json;
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
            var info = await Public.Module.IO.Network.Update.CheckUpdateAsync();
            if (!info.Success)
            {
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
                Notice(MainLang.CheckUpdateFail, NotificationType.Error);
                return;
            }

            if (!info.IsNeedUpdate)
            {
                CheckUpdateBtn.IsEnabled = true;
                CheckUpdateBtn.Content = MainLang.CheckUpdate;
                Notice(MainLang.CurrentlyTheLatestVersion, NotificationType.Success);
                return;
            }

            CheckUpdateBtn.IsEnabled = true;
            CheckUpdateBtn.Content = MainLang.CheckUpdate;
            var dialog = ContentDialogResult.None;

            await Dispatcher.UIThread.Invoke(async () =>
            {
                dialog = await ShowDialogAsync(MainLang.FoundNewVersion,
                    $"{info.NewVersion}\n\n{info.GithubUrl}"
                    , b_cancel: MainLang.Cancel, b_secondary: MainLang.SkipThisVersion,
                    b_primary: MainLang.Update);
            });

            if (dialog == ContentDialogResult.Primary)
            {
                var updateAppAsync = await Public.Module.IO.Network.Update.UpdateAppAsync();
                if (!updateAppAsync) Notice(MainLang.UpdateFail, NotificationType.Error);
            }
            else if (dialog == ContentDialogResult.Secondary)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Public.Const.Data.SettingEntry.SkipUpdateVersion = info.NewVersion;
                    Notice(MainLang.SkipVersionTip.Replace("{version}", info.NewVersion), NotificationType.Success);
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
                            Foreground = new SolidColorBrush(Data.SettingEntry.AccentColor),
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
                        Foreground = new SolidColorBrush(Data.SettingEntry.AccentColor),
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