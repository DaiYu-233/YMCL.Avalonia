using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launch
{
    public partial class LaunchSettingPage : UserControl
    {
        List<string> minecraftFolders = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Const.MinecraftFolderDataPath))!;
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
                Method.MarginAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
            };
            AddMinecraftFolderBtn.Click += async (s, e) =>
            {
                var topLevel = TopLevel.GetTopLevel(this);
                var storageProvider = topLevel!.StorageProvider;
                var result = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = Public.Langs.MainLang.SelectMinecraftFolder
                });
                if (result != null && result.Count > 0)
                {
                    var item = result[0];
                    if (item.Name == ".minecraft")
                    {
                        if (!minecraftFolders.Contains(item.Path.AbsolutePath))
                        {
                            minecraftFolders.Add(item.Path.AbsolutePath);
                            File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(minecraftFolders, Formatting.Indented));
                            MinecraftFolderComboBox.Items.Clear();
                            minecraftFolders.ForEach(folder =>
                            {
                                MinecraftFolderComboBox.Items.Add(folder);
                            });
                            MinecraftFolderComboBox.SelectedIndex = MinecraftFolderComboBox.ItemCount - 1;
                            Method.Toast(Public.Langs.MainLang.SuccessAdd + "£º" + item.Path.AbsolutePath, NotificationType.Success);
                        }
                        else
                        {
                            Method.Toast(Public.Langs.MainLang.TheItemAlreadyExist, NotificationType.Error);
                        }
                    }
                    else
                    {
                        Method.Toast(Public.Langs.MainLang.NeedToSelectMinecraftFolder, NotificationType.Error);
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
                Method.Toast(Public.Langs.MainLang.SuccessRemove + "£º" + path, NotificationType.Success);
                if (minecraftFolders.Count == 0)
                {
                    minecraftFolders.Add(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft"));
                    Method.Toast(Public.Langs.MainLang.SuccessAdd + "£º" + Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!, ".minecraft"), NotificationType.Success);
                }
                MinecraftFolderComboBox.Items.Clear();
                minecraftFolders.ForEach(folder =>
                {
                    MinecraftFolderComboBox.Items.Add(folder);
                });
                MinecraftFolderComboBox.SelectedIndex = MinecraftFolderComboBox.ItemCount - 1;
                File.WriteAllText(Const.MinecraftFolderDataPath, JsonConvert.SerializeObject(minecraftFolders, Formatting.Indented));
            };
        }

        private void ControlProperty()
        {
            var setting = JsonConvert.DeserializeObject<Public.Classes.Setting>(File.ReadAllText(Const.SettingDataPath));
            minecraftFolders.ForEach(folder =>
            {
                MinecraftFolderComboBox.Items.Add(folder);
            });
            MinecraftFolderComboBox.SelectedItem = setting.MinecraftFolder;
        }
    }
}
