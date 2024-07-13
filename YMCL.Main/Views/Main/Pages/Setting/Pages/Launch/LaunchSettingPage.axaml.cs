using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using MinecraftLaunch.Classes.Interfaces;
using MinecraftLaunch.Classes.Models.Game;
using MinecraftLaunch.Components.Fetcher;
using MinecraftLaunch.Components.Resolver;
using MinecraftLaunch.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launch
{
    public partial class LaunchSettingPage : UserControl
    {
        List<string> minecraftFolders = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath))!;
        List<JavaEntry> javas = JsonConvert.DeserializeObject<List<JavaEntry>>(File.ReadAllText(Const.JavaDataPath))!;
        bool _firstLoad = true;
        public LaunchSettingPage()
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
                var totalMemory = Method.Value.GetTotalMemory(Const.Platform);
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (_firstLoad)
                {
                    _firstLoad = false;
                    if (totalMemory != 0)
                    {
                        MaxMemSlider.Maximum = totalMemory / 1024;
                    }
                    else
                    {
                        MaxMemSlider.Maximum = 65536;
                    }
                    MaxMemSlider.Value = setting.MaxMem;
                }
                if (setting.MinecraftFolder == null || !minecraftFolders.Contains(setting.MinecraftFolder))
                {
                    MinecraftFolderComboBox.SelectedIndex = 0;
                }
                else
                {
                    MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
                }
            };
            IndependencyCoreSwitch.Click += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (IndependencyCoreSwitch.IsChecked != setting.EnableIndependencyCore)
                {
                    setting.EnableIndependencyCore = (bool)IndependencyCoreSwitch.IsChecked!;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                }
            };
            ShowGameOutputSwitch.Click += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (ShowGameOutputSwitch.IsChecked != setting.ShowGameOutput)
                {
                    setting.ShowGameOutput = (bool)ShowGameOutputSwitch.IsChecked!;
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                }
            };
            AddMinecraftFolderBtn.Click += async (s, e) =>
            {
                var result = await Method.IO.OpenFolderPicker(TopLevel.GetTopLevel(this)!, new FolderPickerOpenOptions() { AllowMultiple = false, Title = MainLang.SelectMinecraftFolder });
                if (result != null && result.Count > 0)
                {
                    var item = result[0];
                    if (item.Name == ".minecraft")
                    {
                        if (!minecraftFolders.Contains(item.Path))
                        {
                            minecraftFolders.Add(item.Path);
                            File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(minecraftFolders, Formatting.Indented));
                            MinecraftFolderComboBox.Items.Clear();
                            minecraftFolders.ForEach(folder =>
                            {
                                MinecraftFolderComboBox.Items.Add(folder);
                            });
                            MinecraftFolderComboBox.SelectedIndex = MinecraftFolderComboBox.ItemCount - 1;
                            Method.Ui.Toast(MainLang.SuccessAdd + ": " + item.Path, Const.Notification.main, NotificationType.Success);
                        }
                        else
                        {
                            Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.main, NotificationType.Error);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.Path))
                        {
                            Method.Ui.Toast(MainLang.NeedToSelectMinecraftFolder, Const.Notification.main, NotificationType.Error);
                        }
                    }
                }
            };
            MinecraftFolderComboBox.SelectionChanged += (s, e) =>
            {
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                setting.MinecraftFolder = (string)MinecraftFolderComboBox.SelectionBoxItem;
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            };
            DelSeletedMinecraftFolderBtn.Click += (s, e) =>
            {
                var path = (string)MinecraftFolderComboBox.SelectedItem;
                minecraftFolders.RemoveAt(MinecraftFolderComboBox.SelectedIndex);
                Method.Ui.Toast(MainLang.SuccessRemove + ": " + path, Const.Notification.main, NotificationType.Success);
                if (minecraftFolders.Count == 0)
                {
                    var a = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft");
                    minecraftFolders.Add(a);
                    Method.Ui.Toast(MainLang.SuccessAdd + ": " + a, Const.Notification.main, NotificationType.Success);
                }
                MinecraftFolderComboBox.Items.Clear();
                minecraftFolders.ForEach(folder =>
                {
                    MinecraftFolderComboBox.Items.Add(folder);
                });
                MinecraftFolderComboBox.SelectedIndex = MinecraftFolderComboBox.ItemCount - 1;
                File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(minecraftFolders, Formatting.Indented));
            };
            AutoScanBtn.Click += async (s, e) =>
            {
                JavaFetcher javaFetcher = new JavaFetcher();
                var javaList = await javaFetcher.FetchAsync();
                var repeatJava = string.Empty;
                var successAddJava = string.Empty;
                var repeatJavaCount = 0;
                var successAddCount = 0;
                foreach (JavaEntry java in javaList)
                {
                    if (!javas.Contains(java))
                    {
                        if (successAddCount == 0)
                        {
                            successAddJava += java.JavaPath;
                        }
                        else
                        {
                            successAddJava += "\n" + java.JavaPath;
                        }
                        javas.Add(java);
                        successAddCount++;
                    }
                    else
                    {
                        if (repeatJavaCount == 0)
                        {
                            repeatJava += java.JavaPath;
                        }
                        else
                        {
                            repeatJava += "\n" + java.JavaPath;
                        }
                        repeatJavaCount++;
                    }
                }
                JavaComboBox.Items.Clear();
                JavaComboBox.Items.Add(new JavaEntry() { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" });
                javas.ForEach(java =>
                {
                    JavaComboBox.Items.Add(java);
                });
                JavaComboBox.SelectedIndex = 0;
                File.WriteAllText(Const.JavaDataPath, JsonConvert.SerializeObject(javas, Formatting.Indented));
                Method.Ui.Toast($"{MainLang.ScanJavaSuccess}\n{MainLang.SuccessAdd}: {successAddCount}\n{MainLang.RepeatItem}: {repeatJavaCount}", Const.Notification.main, NotificationType.Success);
            };
            ManualAddBtn.Click += async (s, e) =>
            {
                var list = await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this)!, new FilePickerOpenOptions() { AllowMultiple = false, Title = MainLang.SelectJava });
                list.ForEach(java =>
                {
                    var javaInfo = JavaUtil.GetJavaInfo(java.Path);
                    if (javaInfo == null && !string.IsNullOrEmpty(java.Path))
                    {
                        Method.Ui.Toast(MainLang.TheJavaIsError, Const.Notification.main, NotificationType.Error);
                    }
                    else
                    {
                        if (javas.Contains(javaInfo!))
                        {
                            Method.Ui.Toast(MainLang.TheItemAlreadyExist, Const.Notification.main, NotificationType.Error);
                        }
                        else
                        {
                            javas.Add(javaInfo!);
                        }
                    }
                });
                JavaComboBox.Items.Clear();
                JavaComboBox.Items.Add(new JavaEntry() { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" });
                javas.ForEach(java =>
                {
                    JavaComboBox.Items.Add(java);
                });
                JavaComboBox.SelectedIndex = 0;
                File.WriteAllText(Const.JavaDataPath, JsonConvert.SerializeObject(javas, Formatting.Indented));
            };
            JavaComboBox.SelectionChanged += (s, e) =>
            {
                JavaComboBox.IsVisible = false;
                JavaComboBox.IsVisible = true;
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (JavaComboBox.SelectedIndex == 0)
                {
                    setting.Java = new JavaEntry() { JavaPath = "Auto", JavaVersion = "All" };
                }
                else
                {
                    setting.Java = JavaComboBox.SelectedItem as JavaEntry;
                }
                File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
            };
            MaxMemSlider.ValueChanged += (s, e) =>
            {
                MaxMemText.Text = $"{Math.Round(MaxMemSlider.Value)}M";
                var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
                if (setting.MaxMem != MaxMemSlider.Value)
                {
                    setting.MaxMem = Math.Round(MaxMemSlider.Value);
                    File.WriteAllText(Const.SettingDataPath, JsonConvert.SerializeObject(setting, Formatting.Indented));
                }
            };
        }

        private void ControlProperty()
        {
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            minecraftFolders.ForEach(folder =>
            {
                MinecraftFolderComboBox.Items.Add(folder);
            });
            IndependencyCoreSwitch.IsChecked = setting.EnableIndependencyCore;
            ShowGameOutputSwitch.IsChecked = setting.ShowGameOutput;
            if (javas.Contains(null))
            {
                javas.RemoveAll(item => item == null);
                File.WriteAllText(Const.JavaDataPath, JsonConvert.SerializeObject(javas, Formatting.Indented));
            }
            JavaComboBox.Items.Add(new JavaEntry() { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" });
            javas.ForEach(java =>
            {
                JavaComboBox.Items.Add(java);
            });
            if (setting.Java == new JavaEntry() { JavaPath = "Auto", JavaVersion = "All" })
            {
                JavaComboBox.SelectedIndex = 0;
            }
            else
            {
                JavaComboBox.SelectedItem = setting.Java;
            }
        }
    }
}
