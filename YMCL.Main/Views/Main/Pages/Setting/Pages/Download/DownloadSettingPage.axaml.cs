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
        MaximumDownloadThreadSlider.ValueChanged += (_, _) =>
        {
            var value = Math.Round(MaximumDownloadThreadSlider.Value);
            MaximumDownloadThreadText.Text = value.ToString();
            MaximumDownloadThreadSlider.Value = value;
            DownloadThreadWarning.IsOpen = MaximumDownloadThreadSlider.Value > 32;
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
        DownloadThreadWarning.IsOpen = MaximumDownloadThreadSlider.Value > 32;
    }
}