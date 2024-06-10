using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Styling;
using System;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Launch
{
    public partial class LaunchPage : UserControl
    {
        public LaunchPage()
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
