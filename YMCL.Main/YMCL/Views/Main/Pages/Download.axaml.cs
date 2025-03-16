using FluentAvalonia.UI.Controls;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages;

public partial class Download : UserControl
{
    public readonly DownloadPages.AutoInstall _autoInstall = new();
    public readonly DownloadPages.CurseForge _curseForge = new();
    public readonly DownloadPages.Modrinth _modrinth = new();
    public readonly DownloadPages.Favourites _favourites = new();
    public NavigationView NavigationView => Nav;
    public ContentControl Frame => FrameView;

    public Download()
    {
        InitializeComponent();FrameView.Content = _autoInstall;
        BindingEvent();
    }

    private void BindingEvent()
    {
        Nav.SelectionChanged += (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            UserControl page = tag switch
            {
                "autoInstall" => _autoInstall,
                "curseForge" => _curseForge,
                "modrinth" => _modrinth,
                "favourite" => _favourites,
                _ => null
            };
            if (page == null) return;
            FrameView.Content = page;
            _ = Animator.PageLoading.LevelTwoPage(page);
        };
        Loaded += (_, _) =>
        {
            _ = Animator.PageLoading.LevelTwoPage(FrameView.Content as UserControl);
        };
    }
}