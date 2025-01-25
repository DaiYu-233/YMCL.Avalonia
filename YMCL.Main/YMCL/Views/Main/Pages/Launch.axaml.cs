using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MinecraftLaunch.Classes.Models.Game;

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
        Data.Setting.PropertyChanged += (o, e) =>
        {
            if (e.PropertyName != nameof(Public.Classes.Setting.SelectedGame)) return;
            if (!_gameList.CanCloseGameList) return;
            _ = CloseGameList();
        };
        LaunchBtn.Click += (_, _) => { Data.Setting.SelectedGame.LaunchAction?.Invoke(); };
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

    public async System.Threading.Tasks.Task OpenGameSetting(GameEntry? entry = null)
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
}