using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launch;

public partial class LaunchSettingPage : UserControl
{
    private readonly List<JavaEntry> javas =
        JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.String.JavaDataPath))!;

    private readonly List<string> minecraftFolders =
        JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.String.MinecraftFolderDataPath))!;

    private bool _firstLoad = true;

    public LaunchSettingPage()
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
            var setting = Const.Data.Setting;
            if (setting.MinecraftFolder == null || !minecraftFolders.Contains(setting.MinecraftFolder))
                MinecraftFolderComboBox.SelectedIndex = 0;
            else
                MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
            if (_firstLoad)
            {
                _firstLoad = false;
                var totalMemory = Method.Value.GetTotalMemory(Const.Data.Platform);
                if (totalMemory != 0)
                    MaxMemSlider.Maximum = totalMemory / 1024;
                else
                    MaxMemSlider.Maximum = 65536;
                MaxMemSlider.Value = setting.MaxMem;

                while (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    await Task.Run(() =>
                    {
                        using var managementClass = new ManagementClass("Win32_PerfFormattedData_PerfOS_Memory");
                        using var instances = managementClass.GetInstances();
                        foreach (var mo in instances)
                        {
                            var size = long.Parse(mo.Properties["AvailableMBytes"].Value.ToString()!);
                            var used = Math.Round(size / (totalMemory / 1024) * 100, 2);
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                var free = Math.Round(100 - (size / (totalMemory / 1024) * 100) -
                                                      (MaxMemSlider.Value / MaxMemSlider.Maximum * 100), 2);
                                UsedMemText.Text = $"{used}%";
                                UsedMemProgressBar.Value = used;
                                CanUseMemText.Text = $"{free}%";
                            });
                        }
                    });
                    await Task.Delay(500);
                }

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    UsedMemRoot.IsVisible = false;
                    CanUseMemText.IsVisible = false;
                }
            }
        };
        IndependencyCoreSwitch.Click += (s, e) =>
        {
            var setting = Const.Data.Setting;
            if (IndependencyCoreSwitch.IsChecked != setting.EnableIndependencyCore)
            {
                setting.EnableIndependencyCore = (bool)IndependencyCoreSwitch.IsChecked!;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        };
        ShowGameOutputSwitch.Click += (s, e) =>
        {
            var setting = Const.Data.Setting;
            if (ShowGameOutputSwitch.IsChecked != setting.ShowGameOutput)
            {
                setting.ShowGameOutput = (bool)ShowGameOutputSwitch.IsChecked!;
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        };
        AddMinecraftFolderBtn.Click += async (s, e) =>
        {
            var result = await Method.IO.OpenFolderPicker(TopLevel.GetTopLevel(this)!,
                new FolderPickerOpenOptions { AllowMultiple = false, Title = MainLang.SelectMinecraftFolder });
            if (result != null && result.Count > 0)
            {
                var item = result[0];
                if (item.Name == ".minecraft")
                {
                    if (!minecraftFolders.Contains(item.Path))
                    {
                        minecraftFolders.Add(item.Path);
                        File.WriteAllText(Const.String.MinecraftFolderDataPath,
                            JsonConvert.SerializeObject(minecraftFolders, Formatting.Indented));
                        MinecraftFolderComboBox.Items.Clear();
                        minecraftFolders.ForEach(folder => { MinecraftFolderComboBox.Items.Add(folder); });
                        MinecraftFolderComboBox.SelectedIndex = MinecraftFolderComboBox.ItemCount - 1;
                        Method.Ui.Toast(MainLang.SuccessAdd + ": " + item.Path, Const.Notification.main,
                            NotificationType.Success);
                    }
                    else
                    {
                        Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.main, NotificationType.Error);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.Path))
                        Method.Ui.Toast(MainLang.NeedToSelectMinecraftFolder, Const.Notification.main,
                            NotificationType.Error);
                }
            }
        };
        MinecraftFolderComboBox.SelectionChanged += (s, e) =>
        {
            var setting = Const.Data.Setting;
            setting.MinecraftFolder = (string)MinecraftFolderComboBox.SelectionBoxItem;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        LaunchCoreComboBox.SelectionChanged += (s, e) =>
        {
            var setting = Const.Data.Setting;
            setting.LaunchCore = (LaunchCore)LaunchCoreComboBox.SelectedIndex;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        DelSeletedMinecraftFolderBtn.Click += (s, e) =>
        {
            var path = (string)MinecraftFolderComboBox.SelectedItem;
            minecraftFolders.RemoveAt(MinecraftFolderComboBox.SelectedIndex);
            Method.Ui.Toast(MainLang.SuccessRemove + ": " + path, Const.Notification.main, NotificationType.Success);
            if (minecraftFolders.Count == 0)
            {
                var a = Path.Combine(Const.String.UserDataRootPath, ".minecraft");
                minecraftFolders.Add(a);
                Method.Ui.Toast(MainLang.SuccessAdd + ": " + a, Const.Notification.main, NotificationType.Success);
            }

            MinecraftFolderComboBox.Items.Clear();
            minecraftFolders.ForEach(folder => { MinecraftFolderComboBox.Items.Add(folder); });
            MinecraftFolderComboBox.SelectedIndex = MinecraftFolderComboBox.ItemCount - 1;
            File.WriteAllText(Const.String.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(minecraftFolders, Formatting.Indented));
        };
        AutoScanBtn.Click += async (s, e) =>
        {
            var javaFetcher = new JavaFetcher();
            var javaList = await javaFetcher.FetchAsync();
            var repeatJavaCount = 0;
            var successAddCount = 0;
            foreach (var java in javaList)
                if (!javas.Contains(java))
                {
                    javas.Add(java);
                    successAddCount++;
                }
                else
                {
                    repeatJavaCount++;
                }

            JavaComboBox.Items.Clear();
            JavaComboBox.Items.Add(new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" });
            javas.ForEach(java => { JavaComboBox.Items.Add(java); });
            JavaComboBox.SelectedIndex = 0;
            File.WriteAllText(Const.String.JavaDataPath, JsonConvert.SerializeObject(javas, Formatting.Indented));
            Method.Ui.Toast(
                $"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}",
                Const.Notification.main, NotificationType.Success);
        };
        ManualAddBtn.Click += async (s, e) =>
        {
            var list = await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this)!,
                new FilePickerOpenOptions { AllowMultiple = false, Title = MainLang.SelectJava });
            list.ForEach(java =>
            {
                var javaInfo = JavaUtil.GetJavaInfo(java.Path);
                if (javaInfo == null && !string.IsNullOrWhiteSpace(java.Path))
                {
                    Method.Ui.Toast(MainLang.TheJavaIsError, Const.Notification.main, NotificationType.Error);
                }
                else
                {
                    if (javas.Contains(javaInfo!))
                        Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.main, NotificationType.Error);
                    else
                        javas.Add(javaInfo!);
                }
            });
            JavaComboBox.Items.Clear();
            JavaComboBox.Items.Add(new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" });
            javas.ForEach(java => { JavaComboBox.Items.Add(java); });
            JavaComboBox.SelectedIndex = 0;
            File.WriteAllText(Const.String.JavaDataPath, JsonConvert.SerializeObject(javas, Formatting.Indented));
        };
        JavaComboBox.SelectionChanged += (s, e) =>
        {
            // JavaComboBox.IsVisible = false;
            // JavaComboBox.IsVisible = true;
            var setting = Const.Data.Setting;
            if (JavaComboBox.SelectedIndex == 0)
                setting.Java = new JavaEntry { JavaPath = "Auto", JavaVersion = "All" };
            else
                setting.Java = JavaComboBox.SelectedItem as JavaEntry;
            File.WriteAllText(Const.String.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
        };
        MaxMemSlider.ValueChanged += (s, e) =>
        {
            MaxMemText.Text = $"{Math.Round(MaxMemSlider.Value)}M";
            var setting = Const.Data.Setting;
            if (setting.MaxMem != MaxMemSlider.Value)
            {
                setting.MaxMem = Math.Round(MaxMemSlider.Value);
                File.WriteAllText(Const.String.SettingDataPath,
                    JsonConvert.SerializeObject(setting, Formatting.Indented));
            }
        };
    }

    private void ControlProperty()
    {
        var setting = Const.Data.Setting;
        minecraftFolders.ForEach(folder => { MinecraftFolderComboBox.Items.Add(folder); });
        IndependencyCoreSwitch.IsChecked = setting.EnableIndependencyCore;
        ShowGameOutputSwitch.IsChecked = setting.ShowGameOutput;
        if (javas.Contains(null))
        {
            javas.RemoveAll(item => item == null);
            File.WriteAllText(Const.String.JavaDataPath, JsonConvert.SerializeObject(javas, Formatting.Indented));
        }

        JavaComboBox.Items.Add(new JavaEntry { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" });
        javas.ForEach(java => { JavaComboBox.Items.Add(java); });
        if (setting.Java == new JavaEntry { JavaPath = "Auto", JavaVersion = "All" })
            JavaComboBox.SelectedIndex = 0;
        else
            JavaComboBox.SelectedItem = setting.Java;

        LaunchCoreComboBox.SelectedIndex = (int)setting.LaunchCore;
    }
}