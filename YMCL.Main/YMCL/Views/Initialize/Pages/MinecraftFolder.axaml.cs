using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module.App;

namespace YMCL.Views.Initialize.Pages;

public partial class MinecraftFolder : UserControl
{
    public MinecraftFolder()
    {
        InitializeComponent();
        MinecraftFolderListBox.ItemsSource = Data.Instance.MinecraftFolders;
        ManualAddMinecraftFolderBtn.Click += async (_, _) =>
        {
            var list = await TopLevel.GetTopLevel(this).StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { Title = MainLang.SelectMinecraftFolder });
            if (list.Count < 1) return;
            var textbox = new TextBox
            {
                Watermark = MainLang.DisplayName,
                TextWrapping = TextWrapping.Wrap,
                MaxLength = 60
            };
            var dialog = new ContentDialog
            {
                Title = MainLang.AddFolder,
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        textbox,
                        new TextBox
                        {
                            Text = list[0].Path.LocalPath,
                            IsReadOnly = true,
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                },
                PrimaryButtonText = MainLang.Ok,
                CloseButtonText = MainLang.Cancel,
                IsPrimaryButtonEnabled = false
            };
            textbox.TextChanged += (_, _) =>
            {
                dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
            };
            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            Data.Instance.MinecraftFolders.Add(new Public.Classes.MinecraftFolder
                { Name = textbox.Text, Path = list[0].Path.LocalPath });
            File.WriteAllText(ConfigPath.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(Data.Instance.MinecraftFolders, Formatting.Indented));
        };
        MinecraftFolderListBox.SelectionChanged += (_, _) =>
        {
            Data.Instance.Setting.MinecraftFolder = MinecraftFolderListBox.SelectedItem as Public.Classes.MinecraftFolder;
        };
    }
}