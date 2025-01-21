using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Views.Main.Pages.LaunchPages;

public partial class GameList : UserControl
{
    public bool CanCloseGameList;

    public GameList()
    {
        InitializeComponent();
        DataContext = Data.Instance;
        CloseBtn.Click += (_, _) => { _ = App.UiRoot.ViewModel.Launch.CloseGameList(); };
        RefreshListBtn.Click += (_, _) => { LaunchUi.LoadGames(); };
        GameListView.PointerEntered += (_, _) => CanCloseGameList = true;
        GameListView.PointerExited += (_, _) => CanCloseGameList = false;
    }
}