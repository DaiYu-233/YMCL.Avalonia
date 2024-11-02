using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using YMCL.Main.Public;
using YMCL.Main.Views.Main.Pages.More.Pages.GameUpdateLog;
using YMCL.Main.Views.Main.Pages.More.Pages.TreasureBox;

namespace YMCL.Main.Views.Main.Pages.More;

public partial class MorePage : UserControl
{
    public readonly TreasureBox _treasureBoxPage = new();
    public readonly GameUpdateLog _gameUpdateLog = new();

    public MorePage()
    {
        InitializeComponent();
        BindingEvent();
        FrameView.Content = _treasureBoxPage;
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
        };
        Nav.SelectionChanged += async (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "treasureBox":
                    _treasureBoxPage.Root.IsVisible = false;
                    FrameView.Content = _treasureBoxPage;
                    break;
                case "gameUpdateLog":
                    _gameUpdateLog.Root.IsVisible = false;
                    FrameView.Content = _gameUpdateLog;
                    break;
            }
            _ = Const.Window.main.FocusButton();
        };
    }
}