using System;
using Avalonia.Controls;
using YMCL.Main.Public;
using YMCL.Main.Views.Main.Pages.More.Pages.TreasureBox;

namespace YMCL.Main.Views.Main.Pages.More;

public partial class MorePage : UserControl
{
    private readonly TreasureBox treasureBoxPage = new();

    public MorePage()
    {
        InitializeComponent();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            FrameView.Content = treasureBoxPage;
        };
    }
}