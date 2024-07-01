using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;
using Avalonia.Platform.Storage;
using System.Threading;
using Avalonia.Threading;
using System.Diagnostics;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Personalize
{
    public partial class PersonalizeSettingPage : UserControl
    {
        bool _firstLoad = true;
        public PersonalizeSettingPage()
        {
            InitializeComponent();
            ControlProperty();
            BindingEvent();
        }

        private void BindingEvent()
        {
            Loaded += async (s, e) =>
            {
                Method.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
                if (_firstLoad)
                {
                    _firstLoad = false;
                    var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                    WindowTitleBarStyleComboBox.SelectedIndex = setting.WindowTitleBarStyle == WindowTitleBarStyle.System ? 0 : 1;
                }
                await Task.Delay(20);
                ColorPicker.Width = ColorPickerRoot.Bounds.Width - 2 * 6.5 - ColorPickerLabel.Bounds.Width - 30;
            };
            SizeChanged += (s, e) =>
            {
                ColorPicker.Width = ColorPickerRoot.Bounds.Width - 2 * 6.5 - ColorPickerLabel.Bounds.Width - 30;
            };
            CustomHomePageComboBox.SelectionChanged += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                EditCustomHomePageBtn.IsVisible = CustomHomePageComboBox.SelectedIndex == 1 ? true : false;
                if (CustomHomePageComboBox.SelectedIndex != (int)setting.CustomHomePage)
                {
                    setting.CustomHomePage = (CustomHomePageWay)CustomHomePageComboBox.SelectedIndex;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                    Method.RestartApp();
                }
            };
            ColorPicker.ColorChanged += (s, e) =>
            {
                var color = ColorPicker.Color;
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (setting.AccentColor != color)
                {
                    setting.AccentColor = color;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                }
                Method.SetAccentColor(color);
            };
            EditCustomHomePageBtn.Click += (s, e) =>
            {
                var launcher = TopLevel.GetTopLevel(this).Launcher;
                launcher.LaunchFileInfoAsync(new FileInfo(Const.CustomHomePageXamlDataPath));
            };
            WindowTitleBarStyleComboBox.SelectionChanged += async (_, _) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if ((WindowTitleBarStyleComboBox.SelectedIndex == 0 && setting.WindowTitleBarStyle == WindowTitleBarStyle.System) || (WindowTitleBarStyleComboBox.SelectedIndex == 1 && setting.WindowTitleBarStyle == WindowTitleBarStyle.Ymcl)) return;
                setting.WindowTitleBarStyle = WindowTitleBarStyleComboBox.SelectedIndex == 0 ? WindowTitleBarStyle.System : WindowTitleBarStyle.Ymcl;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                switch (setting.WindowTitleBarStyle)
                {
                    case WindowTitleBarStyle.Unset:
                        await Task.Delay(350);
                        Const.Window.main.TitleBar.IsVisible = false;
                        Const.Window.main.TitleRoot.IsVisible = false;
                        Const.Window.main.Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                        Const.Window.main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                        Const.Window.main.ExtendClientAreaToDecorationsHint = false;
                        var comboBox = new ComboBox()
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                        };
                        comboBox.Items.Add("System");
                        comboBox.Items.Add("Ymcl");
                        comboBox.SelectedIndex = 0;
                        ContentDialog dialog = new()
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"],
                            Title = MainLang.WindowTitleBarStyle,
                            PrimaryButtonText = MainLang.Ok,
                            DefaultButton = ContentDialogButton.Primary,
                            Content = comboBox
                        };
                        comboBox.SelectionChanged += (_, _) =>
                        {
                            if (comboBox.SelectedIndex == 0)
                            {
                                Const.Window.main.TitleBar.IsVisible = false;
                                Const.Window.main.TitleRoot.IsVisible = false;
                                Const.Window.main.Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                                Const.Window.main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                                Const.Window.main.ExtendClientAreaToDecorationsHint = false;
                            }
                            else
                            {
                                Const.Window.main.TitleBar.IsVisible = true;
                                Const.Window.main.TitleRoot.IsVisible = true;
                                Const.Window.main.Root.CornerRadius = new CornerRadius(8);
                                Const.Window.main.WindowState = WindowState.Maximized;
                                Const.Window.main.WindowState = WindowState.Normal;
                                Const.Window.main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                                Const.Window.main.ExtendClientAreaToDecorationsHint = true;
                            }
                        };
                        dialog.PrimaryButtonClick += (_, _) =>
                        {
                            setting.WindowTitleBarStyle = comboBox.SelectedIndex == 0 ? WindowTitleBarStyle.System : WindowTitleBarStyle.Ymcl;
                            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                        };
                        await dialog.ShowAsync();
                        break;
                    case WindowTitleBarStyle.System:
                        Const.Window.main.TitleBar.IsVisible = false;
                        Const.Window.main.TitleRoot.IsVisible = false;
                        Const.Window.main.Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                        Const.Window.main.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.Default;
                        Const.Window.main.ExtendClientAreaToDecorationsHint = false;
                        break;
                    case WindowTitleBarStyle.Ymcl:
                        Const.Window.main.TitleBar.IsVisible = true;
                        Const.Window.main.TitleRoot.IsVisible = true;
                        Const.Window.main.Root.CornerRadius = new CornerRadius(8);
                        Const.Window.main.WindowState = WindowState.Maximized;
                        Const.Window.main.WindowState = WindowState.Normal;
                        Const.Window.main.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                        Const.Window.main.ExtendClientAreaToDecorationsHint = true;
                        break;
                }
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
            CustomHomePageComboBox.SelectedIndex = (int)setting.CustomHomePage;
            EditCustomHomePageBtn.IsVisible = CustomHomePageComboBox.SelectedIndex == 1 ? true : false;
            ColorPicker.Color = setting.AccentColor;
        }
    }
}
