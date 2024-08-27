using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
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
        Loaded += async (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            if (!_firstLoad) return;
            _firstLoad = false;
            CustomUpdateUrlTextBox.IsVisible = CustomUpdateUrlEnableComboBox.SelectedIndex == 1;
            await Task.Delay(10);
            if (CustomUpdateUrlTextBox.IsVisible)
                CustomUpdateUrlEnableComboBox.Width = 150;
            else
                CustomUpdateUrlEnableComboBox.Width = CustomUpdateUrlRoot.Bounds.Width - 2 * 6.5 -
                                                      CustomUpdateUrlLabel.Bounds.Width - 30;
        };
        MusicApiButton.Click += (s, e) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchUriAsync(new Uri("https://gitlab.com/Binaryify/neteasecloudmusicapi"));
        };
        MusicApiTextBox.TextChanged += (s, e) =>
        {
            var setting = Const.Data.Setting;
            setting.MusicApi = MusicApiTextBox.Text;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            Const.String.MusicApiUrl = MusicApiTextBox.Text;
        };
        AutoUpdateSwitch.Click += (s, e) =>
        {
            var setting = Const.Data.Setting;
            if (AutoUpdateSwitch.IsChecked != setting.ShowGameOutput)
            {
                setting.EnableAutoCheckUpdate = (bool)AutoUpdateSwitch.IsChecked!;
                File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        };
        SizeChanged += (_, _) =>
        {
            try
            {
                if (CustomUpdateUrlTextBox.IsVisible)
                    CustomUpdateUrlEnableComboBox.Width = 150;
                else
                    CustomUpdateUrlEnableComboBox.Width = CustomUpdateUrlRoot.Bounds.Width - 2 * 6.5 -
                                                          CustomUpdateUrlLabel.Bounds.Width - 30;
            }
            catch
            {
            }
        };
        DownloadSourceComboBox.SelectionChanged += (s, e) =>
        {
            var setting = Const.Data.Setting;
            if (setting.DownloadSource == (DownloadSource)DownloadSourceComboBox.SelectedIndex) return;
            setting.DownloadSource = (DownloadSource)DownloadSourceComboBox.SelectedIndex;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        CustomUpdateUrlEnableComboBox.SelectionChanged += (s, e) =>
        {
            CustomUpdateUrlTextBox.IsVisible = CustomUpdateUrlEnableComboBox.SelectedIndex == 1;
            if (CustomUpdateUrlTextBox.IsVisible)
                CustomUpdateUrlEnableComboBox.Width = 150;
            else
                CustomUpdateUrlEnableComboBox.Width = CustomUpdateUrlRoot.Bounds.Width - 2 * 6.5 -
                                                      CustomUpdateUrlLabel.Bounds.Width - 30;
            var setting = Const.Data.Setting;
            if (setting.EnableCustomUpdateUrl == (CustomUpdateUrlEnableComboBox.SelectedIndex == 1)) return;
            setting.EnableCustomUpdateUrl = CustomUpdateUrlEnableComboBox.SelectedIndex == 1;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        CustomUpdateUrlTextBox.TextChanged += (_, _) =>
        {
            var setting = Const.Data.Setting;
            if (setting.CustomUpdateUrl == CustomUpdateUrlTextBox.Text) return;
            setting.CustomUpdateUrl = CustomUpdateUrlTextBox.Text;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        MaximumDownloadThreadSlider.ValueChanged += (_, _) =>
        {
            var value = Math.Round(MaximumDownloadThreadSlider.Value);
            MaximumDownloadThreadText.Text = value.ToString();
            MaximumDownloadThreadSlider.Value = value;
            DownloadThreadWarning.IsVisible = MaximumDownloadThreadSlider.Value > 100;
            var setting = Const.Data.Setting;
            if (setting.MaximumDownloadThread == value) return;
            setting.MaximumDownloadThread = value;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
    }

    private void ControlProperty()
    {
        var setting = Const.Data.Setting;
        DownloadSourceComboBox.SelectedIndex = (int)setting.DownloadSource;
        MaximumDownloadThreadText.Text = setting.MaximumDownloadThread.ToString();
        MaximumDownloadThreadSlider.Value = setting.MaximumDownloadThread;
        DownloadThreadWarning.IsVisible = MaximumDownloadThreadSlider.Value > 100;
        CustomUpdateUrlEnableComboBox.SelectedIndex = setting.EnableCustomUpdateUrl ? 1 : 0;
        AutoUpdateSwitch.IsChecked = setting.EnableAutoCheckUpdate;
        CustomUpdateUrlTextBox.Text = setting.CustomUpdateUrl;
        Const.String.MusicApiUrl = setting.MusicApi;
        MusicApiTextBox.Text = setting.MusicApi;
    }
}