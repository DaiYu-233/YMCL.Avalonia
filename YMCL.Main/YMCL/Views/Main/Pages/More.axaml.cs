using FluentAvalonia.UI.Controls;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages;

public partial class More : UserControl
{
    public readonly MorePages.TreasureBox _treasureBox = new();
    public readonly MorePages.GameUpdateLog _gameUpdateLog = new();

    public More()
    {
        InitializeComponent();
        FrameView.Content = _treasureBox;
        BindingEvent();
    }

    private void BindingEvent()
    {
        Nav.SelectionChanged += (o, e) =>
        {
            var tag = ((e.SelectedItem as NavigationViewItem).Tag as string)!;
            var page = tag switch
            {
                "treasureBox" => _treasureBox,
                "gameUpdateLog" => _gameUpdateLog,
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