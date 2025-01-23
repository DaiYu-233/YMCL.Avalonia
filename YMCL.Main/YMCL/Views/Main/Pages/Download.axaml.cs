using FluentAvalonia.UI.Controls;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages;

public partial class Download : UserControl
{
    public readonly DownloadPages.AutoInstall _autoInstall = new();
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
            var page = tag switch
            {
                "autoInstall" => _autoInstall,
                _ => FrameView.Content as UserControl
            };
            FrameView.Content = page;
            _ = Animator.PageLoading.LevelTwoPage(page);
        };
        Loaded += (_, _) =>
        {
            _ = Animator.PageLoading.LevelTwoPage(FrameView.Content as UserControl);
        };
    }
}