using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Classes;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Ui;

public class Shower
{
    public static void Toast(string msg, NotificationType type = NotificationType.Information, 
        bool time = true, string title = "Yu Minecraft Launcher")
    {
        var showTitle = Const.String.AppTitle;
        if (!string.IsNullOrWhiteSpace(title)) showTitle = title;
        if (time) showTitle += $" - {DateTime.Now:HH:mm:ss}";
        Data.Notification.Show(new Notification(showTitle, msg, type));
    }
    
    public static void ShowShortException(string msg, Exception ex)
    {
        Toast($"{msg}\n{ex.Message}", NotificationType.Error);
    }
    
    public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = "Content",
        Control p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null,
        Window p_window = null)
    {
        var content = p_content == null
            ? new TextBox
            {
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Text = msg
            }
            : p_content;
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
        var result = await dialog.ShowAsync(TopLevel.GetTopLevel(YMCL.App.UiRoot));
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
            if (!updateAppAsync) Toast(MainLang.UpdateFail);
        }
        else if (dialog == ContentDialogResult.Secondary)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Const.Data.Setting.SkipUpdateVersion = info.NewVersion;
                Toast(MainLang.SkipVersionTip.Replace("{version}", info.NewVersion));
            });
        }
    }
}