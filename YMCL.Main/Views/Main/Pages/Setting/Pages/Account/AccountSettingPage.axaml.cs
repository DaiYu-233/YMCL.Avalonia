using Avalonia.Controls;
using Newtonsoft.Json;
using System;
using System.IO;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Account
{
    public partial class AccountSettingPage : UserControl
    {
        public AccountSettingPage()
        {
            InitializeComponent(); 
            ControlProperty();
            BindingEvent();
        }

        private void BindingEvent()
        {
            Loaded += (s, e) =>
            {
                Method.MarginAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            };
        }

        private void ControlProperty()
        {
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
        }
    }
}
