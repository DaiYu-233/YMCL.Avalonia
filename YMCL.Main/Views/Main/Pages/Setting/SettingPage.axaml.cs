using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting
{
    public partial class SettingPage : UserControl
    {
        Pages.Launch.LaunchSettingPage launchSettingPage = new();
        public SettingPage()
        {
            InitializeComponent();
            ControlProperty();
            BindingEvent();
        }

        private void ControlProperty()
        {
            FrameView.Content = launchSettingPage;
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
