using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Ursa.Controls;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Langs;
using Notification = Ursa.Controls.Notification;
using Setting = YMCL.Public.Enum.Setting;

namespace YMCL.Public.Module.Ui;

public class Shower
{
    public static void Notice(string msg, NotificationType type = NotificationType.Information,
        bool time = true, string title = "Yu Minecraft Launcher")
    {
        var showTitle = Const.String.AppTitle;
        if (!string.IsNullOrWhiteSpace(title)) showTitle = title;
        if (time) showTitle += $" - {DateTime.Now:HH:mm:ss}";

        var notification = new Notification(showTitle, msg, type);
        UiProperty.NotificationCards.Insert(0, new NotificationEntry(notification, notification.Type));

        switch (Data.Setting.NoticeWay)
        {
            case Setting.NoticeWay.Bubble:
                NotificationBubble(msg, type);
                break;
            case Setting.NoticeWay.Card:
                NotificationCard(msg, type, showTitle);
                break;
        }
    }

    public static void NotificationBubble(string msg, NotificationType type)
    {
        var toast = new Toast(msg, type);
        Data.Toast.Show(toast, toast.Type, classes: ["Light"]);
    }

    public static void NotificationCard(string msg, NotificationType type, string title)
    {
        var notification = new Notification(title, msg, type);
        Data.Notification.Show(notification, notification.Type, classes: ["Light"]);
    }

    public static void ShowShortException(string msg, Exception ex)
    {
        Notice($"{msg}\n{ex.Message}", NotificationType.Error);
    }

    public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = null,
        Control p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null,
        TopLevel? p_host = null)
    {
        var content = p_content == null
            ? new SelectableTextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Text = msg
            }
            : p_content;
        if (!string.IsNullOrWhiteSpace(msg) && p_content != null)
        {
            content = new StackPanel()
            {
                Spacing = 15,
                Children =
                {
                    new SelectableTextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Text = msg
                    },
                    content
                }
            };
        }
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
        var result = await dialog.ShowAsync(p_host ?? TopLevel.GetTopLevel(YMCL.App.UiRoot));
        return result;
    }

    public static async void ShowLongException(string msg, Exception ex)
    {
        var textBox = new TextBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            TextWrapping = TextWrapping.Wrap,
            Text = $"{msg} - {ex.Message}\n\n{ex}",
            HorizontalAlignment = HorizontalAlignment.Center,
            IsReadOnly = true
        };
        await ShowDialogAsync(MainLang.GetException, p_content: textBox, b_primary: MainLang.Ok);
    }

    public static async Task ShowAutoUpdateDialog(CheckUpdateInfo info)
    {
        if (Const.Data.Setting.SkipUpdateVersion == info.NewVersion) return;
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
            var updateAppAsync = await IO.Network.Update.UpdateAppAsync();
            if (!updateAppAsync) Notice(MainLang.UpdateFail, NotificationType.Error);
        }
        else if (dialog == ContentDialogResult.Secondary)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Const.Data.Setting.SkipUpdateVersion = info.NewVersion;
                Notice(MainLang.SkipVersionTip.Replace("{version}", info.NewVersion), NotificationType.Success);
            });
        }
    }

    public static async Task<int> ShowDialogWithComboBox(string[] items, string title = "Title", string? msg = null, TopLevel? p_host = null)
    {
        var comboBox = new ComboBox
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        foreach (var item in items)
        {
            comboBox.Items.Add(item);
        }

        comboBox.SelectedIndex = 0;
        Control content = string.IsNullOrWhiteSpace(msg)
            ? comboBox
            : new StackPanel
            {
                Spacing = 15,
                Children =
                {
                    new TextBlock
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        TextWrapping = TextWrapping.Wrap,
                        Text = msg
                    },
                    comboBox
                }
            };
        ContentDialog dialog = new()
        {
            FontFamily = (FontFamily)Application.Current.Resources["Font"],
            Title = title,
            PrimaryButtonText = MainLang.Ok,
            CloseButtonText = MainLang.Cancel,
            DefaultButton = ContentDialogButton.Primary,
            Content = content
        };
        var dialogResult = await dialog.ShowAsync(TopLevel.GetTopLevel(p_host ?? TopLevel.GetTopLevel(YMCL.App.UiRoot)));
        if (dialogResult == ContentDialogResult.Primary)
            return comboBox.SelectedIndex;
        return -1;
    }
}