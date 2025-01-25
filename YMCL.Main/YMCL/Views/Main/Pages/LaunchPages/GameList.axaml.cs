using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using YMCL.Public.Enum;
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
        OpenSelectedMinecraftFolderBtn.Click+= (_, _) =>
        {
            var path = Data.Setting.MinecraftFolder.Path;
            YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        };
    }
}