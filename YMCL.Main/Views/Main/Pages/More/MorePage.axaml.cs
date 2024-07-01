using Avalonia.Controls;
using System;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.More
{
    public partial class MorePage : UserControl
    {
        Pages.TreasureBox.TreasureBox treasureBoxPage = new();
        public MorePage()
        {
            InitializeComponent();
            BindingEvent();
        }

        private void BindingEvent()
        {
            Loaded += (s, e) =>
            {
                Method.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
                FrameView.Content = treasureBoxPage;
            };
        }
    }
}
