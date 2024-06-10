using Avalonia.Controls;
using System;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Music
{
    public partial class MusicPage : UserControl
    {
        public MusicPage()
        {
            InitializeComponent();
            BindingEvent();
        }

        private void BindingEvent()
        {
            Loaded += (s, e) =>
            {
                Method.MarginAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            };
        }
    }
}
