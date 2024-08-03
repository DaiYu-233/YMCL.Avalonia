using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Personalize;

public partial class PersonalizeSettingPage : UserControl
{
    private bool _firstLoad = true;

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
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            if (_firstLoad)
            {
                _firstLoad = false;
                var setting =
                    JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                WindowTitleBarStyleComboBox.SelectedIndex =
                    setting.WindowTitleBarStyle == WindowTitleBarStyle.System ? 0 : 1;
            }

            await Task.Delay(20);
            ColorPicker.Width = ColorPickerRoot.Bounds.Width - 2 * 6.5 - ColorPickerLabel.Bounds.Width - 30;
            LyricColorPicker.Width = LyricRoot.Bounds.Width - 2 * 6.5 - LyricColorPickerLabel.Bounds.Width - 30;
        };
        SizeChanged += (s, e) =>
        {
            ColorPicker.Width = ColorPickerRoot.Bounds.Width - 2 * 6.5 - ColorPickerLabel.Bounds.Width - 30;
            LyricColorPicker.Width = LyricRoot.Bounds.Width - 2 * 6.5 - LyricColorPickerLabel.Bounds.Width - 30;
        };
        CustomHomePageComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            EditCustomHomePageBtn.IsVisible = CustomHomePageComboBox.SelectedIndex == 1 ? true : false;
            if (CustomHomePageComboBox.SelectedIndex != (int)setting.CustomHomePage)
            {
                setting.CustomHomePage = (CustomHomePageWay)CustomHomePageComboBox.SelectedIndex;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                Method.Ui.RestartApp();
            }
        };
        CustomBackGroundImgComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            EditCustomBackGroundImgBtn.IsVisible = CustomBackGroundImgComboBox.SelectedIndex == 1;
            if (CustomBackGroundImgComboBox.SelectedIndex == 1 == setting.EnableCustomBackGroundImg) return;
            setting.EnableCustomBackGroundImg = CustomBackGroundImgComboBox.SelectedIndex == 1;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            Method.Ui.SetWindowBackGroundImg();
        };
        LyricAlignComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            Const.Window.deskLyric.LyricText.TextAlignment = (TextAlignment)LyricAlignComboBox.SelectedIndex;
            if (setting.DeskLyricAlignment == (TextAlignment)LyricAlignComboBox.SelectedIndex) return;
            setting.DeskLyricAlignment = (TextAlignment)LyricAlignComboBox.SelectedIndex;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        ColorPicker.ColorChanged += (s, e) =>
        {
            var color = ColorPicker.Color;
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.AccentColor != color)
            {
                setting.AccentColor = color;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            Method.Ui.SetAccentColor(color);
        };
        LyricColorPicker.ColorChanged += (s, e) =>
        {
            var color = LyricColorPicker.Color;
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.DeskLyricColor != color)
            {
                setting.DeskLyricColor = color;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            Const.Window.deskLyric.LyricText.Foreground = new SolidColorBrush(color);
        };
        EditCustomHomePageBtn.Click += (s, e) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchFileInfoAsync(new FileInfo(Const.CustomHomePageXamlDataPath));
        };
        LyricSizeSlider.ValueChanged += (s, e) =>
        {
            LyricSizeSliderText.Text = Math.Round(LyricSizeSlider.Value).ToString();
            var color = LyricColorPicker.Color;
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.DeskLyricSize != Math.Round(LyricSizeSlider.Value))
            {
                setting.DeskLyricSize = Math.Round(LyricSizeSlider.Value);
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            Const.Window.deskLyric.LyricText.Transitions = null;
            Const.Window.deskLyric.LyricText.FontSize = Math.Round(LyricSizeSlider.Value);
            Const.Window.deskLyric.Height = setting.DeskLyricSize + 20;
        };
        WindowTitleBarStyleComboBox.SelectionChanged += async (_, _) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if ((WindowTitleBarStyleComboBox.SelectedIndex == 0 &&
                 setting.WindowTitleBarStyle == WindowTitleBarStyle.System) ||
                (WindowTitleBarStyleComboBox.SelectedIndex == 1 &&
                 setting.WindowTitleBarStyle == WindowTitleBarStyle.Ymcl)) return;
            setting.WindowTitleBarStyle = WindowTitleBarStyleComboBox.SelectedIndex == 0
                ? WindowTitleBarStyle.System
                : WindowTitleBarStyle.Ymcl;
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
                    var comboBox = new ComboBox
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        HorizontalAlignment = HorizontalAlignment.Stretch
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
                        setting.WindowTitleBarStyle = comboBox.SelectedIndex == 0
                            ? WindowTitleBarStyle.System
                            : WindowTitleBarStyle.Ymcl;
                        File.WriteAllText(Const.SettingDataPath,
                            JsonConvert.SerializeObject(setting, Formatting.Indented));
                    };
                    await dialog.ShowAsync();
                    break;
                case WindowTitleBarStyle.System:
                    Const.Window.main.TitleBar.IsVisible = false;
                    Const.Window.main.TitleRoot.IsVisible = false;
                    Const.Window.main.Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
                    Const.Window.main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                    Const.Window.main.ExtendClientAreaToDecorationsHint = false;
                    break;
                case WindowTitleBarStyle.Ymcl:
                    Const.Window.main.TitleBar.IsVisible = true;
                    Const.Window.main.TitleRoot.IsVisible = true;
                    Const.Window.main.Root.CornerRadius = new CornerRadius(8);
                    Const.Window.main.WindowState = WindowState.Maximized;
                    Const.Window.main.WindowState = WindowState.Normal;
                    Const.Window.main.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
                    Const.Window.main.ExtendClientAreaToDecorationsHint = true;
                    break;
            }
        };
        OpenFileWayComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            setting.OpenFileWay = (OpenFileWay)OpenFileWayComboBox.SelectedIndex;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        LanguageComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (LanguageComboBox.SelectedItem.ToString().Split(' ')[0] != setting.Language)
            {
                setting.Language = LanguageComboBox.SelectedItem.ToString().Split(' ')[0];
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                Method.Ui.RestartApp();
            }
        };
        ThemeComboBox.SelectionChanged += (_, _) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (ThemeComboBox.SelectedIndex != (int)setting.Theme)
            {
                setting.Theme = (Theme)ThemeComboBox.SelectedIndex;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            Method.Ui.RestartApp();
        };
        LauncherVisibilityComboBox.SelectionChanged += (_, _) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (LauncherVisibilityComboBox.SelectedIndex != (int)setting.LauncherVisibility)
            {
                setting.LauncherVisibility = (LauncherVisibility)LauncherVisibilityComboBox.SelectedIndex;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        };
        EditCustomBackGroundImgBtn.Click += async (_, _) =>
        {
            var path = (await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this),
                    new FilePickerOpenOptions() { Title = MainLang.SelectImgFile }, MainLang.SelectImgFile))
                .FirstOrDefault();

            if (path != null)
            {
                var base64 = Method.Value.BytesToBase64(File.ReadAllBytes(path.Path));

                var setting =
                    JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                setting.WindowBackGroundImgData = base64;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }

            Method.Ui.SetWindowBackGroundImg();
        };
    }

    private void ControlProperty()
    {
        var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
        OpenFileWayComboBox.SelectedIndex = (int)setting.OpenFileWay;
        var langs = new List<string>
        {
            $"zh-CN {MainLang.zh_CN}", $"zh-Hant {MainLang.zh_Hant}", $"en-US {MainLang.en_US}",
            $"ja-JP {MainLang.ja_JP}", $"ru-RU {MainLang.ru_RU}"
        };
        langs.ForEach(lang => { LanguageComboBox.Items.Add(lang); });
        langs.ForEach(lang =>
        {
            var arr = lang.Split(' ');
            if (arr[0] == setting.Language) LanguageComboBox.SelectedItem = lang;
        });
        if (setting.Language == null || setting.Language == string.Empty)
        {
            LanguageComboBox.SelectedItem = "zh-CN";
            setting.Language = "zh-CN";
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        }

        ThemeComboBox.SelectedIndex = (int)setting.Theme;
        LauncherVisibilityComboBox.SelectedIndex = (int)setting.LauncherVisibility;
        CustomHomePageComboBox.SelectedIndex = (int)setting.CustomHomePage;
        LyricAlignComboBox.SelectedIndex = (int)setting.DeskLyricAlignment;
        EditCustomHomePageBtn.IsVisible = CustomHomePageComboBox.SelectedIndex == 1 ? true : false;
        ColorPicker.Color = setting.AccentColor;
        LyricColorPicker.Color = setting.DeskLyricColor;
        LyricSizeSlider.Value = setting.DeskLyricSize;
        LyricSizeSliderText.Text = Math.Round(setting.DeskLyricSize).ToString();
        CustomBackGroundImgComboBox.SelectedIndex = setting.EnableCustomBackGroundImg ? 1 : 0;
        EditCustomBackGroundImgBtn.IsVisible = CustomBackGroundImgComboBox.SelectedIndex == 1;
    }
}