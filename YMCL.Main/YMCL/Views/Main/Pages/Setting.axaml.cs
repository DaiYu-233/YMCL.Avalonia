using FluentAvalonia.UI.Controls;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages;

public partial class Setting : UserControl
{
    private readonly SettingPages.Launch _launch = new();

    public Setting()
    {
        InitializeComponent();
        FrameView.Content = _launch;
        BindingEvent();
    }

    private void BindingEvent()
    {
        Nav.SelectionChanged += (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            var page = tag switch
            {
                "launch" => _launch,
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