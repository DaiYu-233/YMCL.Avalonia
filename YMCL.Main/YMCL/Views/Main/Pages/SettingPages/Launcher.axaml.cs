using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Langs;
using YMCL.Public.Module.Util.Extension;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Launcher : UserControl
{
    public Launcher()
    {
        DataContext = Data.Instance;
        InitializeComponent();
        BindingEvent();
    }

    private void BindingEvent()
    {
        ExportButton.Click += async (_, _) =>
        {
            var ui = new CheckBox() { Content = MainLang.ExchangeSetting_Ui };
            var net = new CheckBox() { Content = MainLang.ExchangeSetting_Net };
            var launch = new CheckBox() { Content = MainLang.ExchangeSetting_Launch };
            var other = new CheckBox() { Content = MainLang.ExchangeSetting_Other };
            var accounts = new CheckBox() { Content = MainLang.ExchangeSetting_Account };
            var panel = new StackPanel()
            {
                Spacing = 5,
                Children =
                {
                    ui,
                    net,
                    launch,
                    other,
                    accounts
                }
            };
            var cr = await ShowDialogAsync(MainLang.Export, MainLang.ExportSettingTip, panel, b_primary: MainLang.Ok,
                b_cancel: MainLang.Cancel);
            if (cr != ContentDialogResult.Primary) return;
            var hex = Public.Module.App.Setting.Export(ui.IsChecked ?? false, net.IsChecked ?? false,
                launch.IsChecked ?? false,
                other.IsChecked ?? false, accounts.IsChecked ?? false);
            var cr1 = await ShowDialogAsync(MainLang.ExportSuccess, "ヾ(•ω•`)o",
                b_primary: MainLang.CopyToClipBoard, b_secondary: MainLang.SaveAs, b_cancel: MainLang.Ok);
            if (cr1 == ContentDialogResult.Primary)
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                await clipboard.SetTextAsync(hex);
            }

            if (cr1 == ContentDialogResult.Secondary)
            {
                var path = (await TopLevel.GetTopLevel(YMCL.App.UiRoot).StorageProvider.SaveFilePickerAsync(
                    new FilePickerSaveOptions
                    {
                        Title = MainLang.Export,
                        SuggestedFileName = $"YMCL.Setting.Export.DaiYu",
                        FileTypeChoices =
                        [
                            new FilePickerFileType("DaiYu File") { Patterns = ["*.DaiYu"] }
                        ]
                    }))?.Path.LocalPath;
                if (string.IsNullOrWhiteSpace(path)) return;
                await File.WriteAllTextAsync(path, hex);
                Notice(MainLang.ExportSuccess, NotificationType.Success);
            }
        };
        ImportButton.Click += async (_, _) =>
        {
            var cr = await ShowDialogAsync(MainLang.Import, "ヾ(•ω•`)o",
                b_primary: MainLang.ReadClipBoard,
                b_secondary: MainLang.OpenFile,
                b_cancel: MainLang.Cancel);
            var hex = string.Empty;
            if (cr == ContentDialogResult.None) return;
            if (cr == ContentDialogResult.Primary)
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                hex = await clipboard.GetTextAsync();
            }

            if (cr == ContentDialogResult.Secondary)
            {
                var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        AllowMultiple = false, Title = MainLang.OpenFile, FileTypeFilter =
                        [
                            new FilePickerFileType("DaiYu File")
                            {
                                Patterns =
                                [
                                    "*.DaiYu"
                                ]
                            }
                        ]
                    });
                if (files.Count == 0) return;
                hex = await File.ReadAllTextAsync(files[0].Path.LocalPath);
            }

            if (string.IsNullOrWhiteSpace(hex))
            {
                Notice(MainLang.ImportFailed, NotificationType.Error);
                return;
            }

            var info = Public.Module.App.Setting.Import(hex);
            if (!info.success || info.data == null)
            {
                Notice(MainLang.ImportFailed, NotificationType.Error);
                return;
            }


            var data = JsonConvert.DeserializeObject<ExchangeSettingEntry.Data>(JsonConvert.SerializeObject(info.data));
            if (data.UiSettings != null)
            {
                data.UiSettings.WindowBackGroundImgData = " ( ...... ) ";
            }

            if (data.AccountSettings != null)
            {
                foreach (var a in data.AccountSettings)
                {
                    a.Skin = " ( ...... ) ";
                    a.Data = " ( ...... ) ";
                }
            }
            
            var cr1 = await ShowDialogAsync(MainLang.Import,
                JsonConvert.SerializeObject(data, Formatting.Indented), b_primary: MainLang.Import,
                b_cancel: MainLang.Cancel);
            if (cr1 != ContentDialogResult.Primary) return;
            Public.Module.App.Setting.Replace(Data.SettingEntry, info.data);
        };
    }
}