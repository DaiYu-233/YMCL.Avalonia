using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes.Data;
using YMCL.Public.Enum;
using YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;

namespace YMCL.Views.Main.Pages.DownloadPages;

public partial class Favourites : UserControl
{
    public Favourites()
    {
        InitializeComponent();
        ListView.SelectionChanged += (_, _) =>
        {
            if (ListView.SelectedItem is not FavouriteResource item) return;
            if (item is { Source: ResourceSource.CurseForge })
            {
                YMCL.App.UiRoot.ViewModel.Download.Nav.SelectedItem = YMCL.App.UiRoot.ViewModel.Download.NavCf;
                YMCL.App.UiRoot.ViewModel.Download._curseForge.CreateNewPage(new SearchTabViewItemEntry()
                {
                    CanClose = true, Host = nameof(DownloadPages.CurseForge),
                    Content = new ModFileResult(item.Id, (item.Title, item.Icon , item.Summary)),
                    Title = $"{item.DisplayType}: {item.Title}"
                });
            }
            else if (item is { Source: ResourceSource.Modrinth })
            {
                YMCL.App.UiRoot.ViewModel.Download.Nav.SelectedItem = YMCL.App.UiRoot.ViewModel.Download.NavMr;
                YMCL.App.UiRoot.ViewModel.Download._modrinth.CreateNewPage(new SearchTabViewItemEntry()
                {
                    CanClose = true, Host = nameof(DownloadPages.Modrinth),
                    Content = new ModrinthPages.ModFileResult(item.Id, (item.Title, item.Icon , item.Summary)),
                    Title = $"{item.DisplayType}: {item.Title}"
                });
            }

            ListView.SelectedItem = null;
        };
    }
}