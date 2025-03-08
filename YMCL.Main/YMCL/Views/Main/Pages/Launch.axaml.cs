using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Base.Models.Game;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Setting;
using YMCL.Public.Langs;
using YMCL.Public.Module.Value;

namespace YMCL.Views.Main.Pages;

public partial class Launch : UserControl
{
    public readonly LaunchPages.GameList _gameList = new();

    public Launch()
    {
        InitializeComponent();
        GameListFrame.Content = _gameList;
        Public.Module.Ui.Special.LaunchUi.LoadGames();
        BindingEvent();
    }

    private void BindingEvent()
    {
        GameListBtn.Click += (_, _) => { _ = OpenGameList(); };
        GameSettingBtn.Click += (_, _) => { _ = OpenGameSetting(); };
        Data.UiProperty.PropertyChanged += (o, e) =>
        {
            if (e.PropertyName != nameof(UiProperty.SelectedMinecraft)) return;
            if (!_gameList.CanCloseGameList) return;
            _ = CloseGameList();
        };
        LaunchBtn.Click += (_, _) => { Data.UiProperty.SelectedMinecraft.LaunchAction?.Invoke(); };
        Loaded += async (_, _) =>
        {
            if (!GameSettingFrame.IsVisible && !GameListFrame.IsVisible) return;
            LaunchConsoleRoot.IsVisible = false;
            LaunchConsoleRoot.Opacity = 0;
            await System.Threading.Tasks.Task.Delay(210);
            LaunchConsoleRoot.IsVisible = true;
        };
    }

    public async System.Threading.Tasks.Task OpenGameList()
    {
        GameListFrame.IsVisible = true;
        LaunchConsoleRoot.Opacity = 0;
        GameListFrame.Margin = new Thickness(10);
        GameListFrame.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
        await System.Threading.Tasks.Task.Delay(210);
    }

    public async System.Threading.Tasks.Task CloseGameList()
    {
        GameListFrame.Margin = new Thickness(40);
        GameListFrame.Opacity = 0;
        await System.Threading.Tasks.Task.Delay(210);
        LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
        GameListFrame.IsVisible = false;
    }

    public async System.Threading.Tasks.Task OpenGameSetting(MinecraftEntry? entry = null)
    {
        GameSettingFrame.Content = new LaunchPages.GameSetting(entry);
        GameSettingFrame.IsVisible = true;
        LaunchConsoleRoot.Opacity = 0;
        GameSettingFrame.Margin = new Thickness(10);
        GameSettingFrame.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
        await System.Threading.Tasks.Task.Delay(210);
    }

    public async System.Threading.Tasks.Task CloseGameSetting()
    {
        GameSettingFrame.Margin = new Thickness(40);
        GameSettingFrame.Opacity = 0;
        await System.Threading.Tasks.Task.Delay(210);
        LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
        GameSettingFrame.IsVisible = false;
    }

    private async void SaveSkin(object? sender, RoutedEventArgs e)
    {
        var path = (await TopLevel.GetTopLevel(YMCL.App.UiRoot).StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = MainLang.ExportLogFile,
                SuggestedFileName = $"{Data.Setting.Account.Name}.png",
                FileTypeChoices =
                [
                    new FilePickerFileType("Image File") { Patterns = ["*.png"] }
                ]
            }))?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        await File.WriteAllBytesAsync(path, Converter.Base64ToBytes(Data.Setting.Account.Skin));
        Notice($"{MainLang.SaveFinish} - {Path.GetFileName(path)}", NotificationType.Success);
    }
}