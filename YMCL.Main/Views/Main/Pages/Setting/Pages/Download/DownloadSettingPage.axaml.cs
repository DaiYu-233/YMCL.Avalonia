using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Download;

public partial class DownloadSettingPage : UserControl
{
    private bool _firstLoad = true;

    public DownloadSettingPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            if (!_firstLoad) return;
            _firstLoad = false;
        };

        DownloadSourceComboBox.SelectionChanged += (s, e) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.DownloadSource == (DownloadSource)DownloadSourceComboBox.SelectedIndex) return;
            setting.DownloadSource = (DownloadSource)DownloadSourceComboBox.SelectedIndex;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        CustomUpdateUrlEnableComboBox.SelectionChanged += (s, e) =>
        {
            CustomUpdateUrlTextBox.IsVisible = CustomUpdateUrlEnableComboBox.SelectedIndex == 1;
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.EnableCustomUpdateUrl == (CustomUpdateUrlEnableComboBox.SelectedIndex == 1)) return;
            setting.EnableCustomUpdateUrl = CustomUpdateUrlEnableComboBox.SelectedIndex == 1;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        CustomUpdateUrlTextBox.TextChanged += (_, _) =>
        {
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.CustomUpdateUrl == CustomUpdateUrlTextBox.Text) return;
            setting.CustomUpdateUrl = CustomUpdateUrlTextBox.Text;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        MaximumDownloadThreadSlider.ValueChanged += (_, _) =>
        {
            var value = Math.Round(MaximumDownloadThreadSlider.Value);
            MaximumDownloadThreadText.Text = value.ToString();
            MaximumDownloadThreadSlider.Value = value;
            DownloadThreadWarning.IsVisible = MaximumDownloadThreadSlider.Value > 100;
            var setting =
                JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.MaximumDownloadThread == value) return;
            setting.MaximumDownloadThread = value;
            File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
    }

    private void ControlProperty()
    {
        var setting =
            JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
        DownloadSourceComboBox.SelectedIndex = (int)setting.DownloadSource;
        MaximumDownloadThreadText.Text = setting.MaximumDownloadThread.ToString();
        MaximumDownloadThreadSlider.Value = setting.MaximumDownloadThread;
        DownloadThreadWarning.IsVisible = MaximumDownloadThreadSlider.Value > 100;
        CustomUpdateUrlEnableComboBox.SelectedIndex = setting.EnableCustomUpdateUrl ? 1 : 0;
        CustomUpdateUrlTextBox.Text = setting.CustomUpdateUrl;
        CustomUpdateUrlTextBox.IsVisible = CustomUpdateUrlEnableComboBox.SelectedIndex == 1;
    }
}