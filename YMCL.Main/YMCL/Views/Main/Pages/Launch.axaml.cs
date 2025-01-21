using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YMCL.Views.Main.Pages;

public partial class Launch : UserControl
{
    public readonly LaunchPages.GameList _gameList = new();
    public readonly LaunchPages.GameSetting _gameSetting = new();

    public Launch()
    {
        InitializeComponent();
        GameListFrame.Content = _gameList;
        GameSettingFrame.Content = _gameSetting;
        Public.Module.Ui.Special.LaunchUi.LoadGames();
        BindingEvent();
    }

    private void BindingEvent()
    {
        GameListBtn.Click += (_, _) => { _ = OpenGameList(); };
        Data.Setting.PropertyChanged += (o, e) =>
        {
            if (e.PropertyName != nameof(Public.Classes.Setting.SelectedGame)) return;
            if (!_gameList.CanCloseGameList) return;
            _ = CloseGameList();
        };
        LaunchBtn.Click += (_, _) => { Data.Setting.SelectedGame.LaunchAction?.Invoke(); };
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
        GameListFrame.Margin = new Thickness(30);
        GameListFrame.Opacity = 0;
        await System.Threading.Tasks.Task.Delay(210);
        LaunchConsoleRoot.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
        GameListFrame.IsVisible = false;
    }
}