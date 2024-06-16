using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Newtonsoft.Json;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Personalize
{
    public partial class PersonalizeSettingPage : UserControl
    {
        public PersonalizeSettingPage()
        {
            InitializeComponent();
            ControlProperty();
            BindingEvent();
        }

        private void BindingEvent()
        {
            Loaded += (s, e) =>
            {
                Method.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            };
            OpenFileWayComboBox.SelectionChanged += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                setting.OpenFileWay = (OpenFileWay)OpenFileWayComboBox.SelectedIndex;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            };
            LanguageComboBox.SelectionChanged += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (LanguageComboBox.SelectedItem.ToString().Split(' ')[0] != setting.Language)
                {
                    setting.Language = LanguageComboBox.SelectedItem.ToString().Split(' ')[0];
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                    Method.RestartApp();
                }
            };
            ThemeComboBox.SelectionChanged += (_, _) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (ThemeComboBox.SelectedIndex != (int)setting.Theme)
                {
                    setting.Theme = (Theme)ThemeComboBox.SelectedIndex;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                }
                Method.RestartApp();
            };
        }

        private void ControlProperty()
        {
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            OpenFileWayComboBox.SelectedIndex = (int)setting.OpenFileWay;
            var langs = new List<string>() { "zh-CN 简体中文", "zh-Hant 繁體中文", "en-US English", "ja-JP 日本語", "ru-RU Русский язык" };
            langs.ForEach(lang =>
            {
                LanguageComboBox.Items.Add(lang);
            });
            langs.ForEach(lang =>
            {
                var arr = lang.Split(' ');
                if (arr[0] == setting.Language)
                {
                    LanguageComboBox.SelectedItem = lang;
                }
            });
            if (setting.Language == null || setting.Language == string.Empty)
            {
                LanguageComboBox.SelectedItem = "zh-CN 简体中文";
                setting.Language = "zh-CN";
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
            ThemeComboBox.SelectedIndex = (int)setting.Theme;
        }
    }
}
