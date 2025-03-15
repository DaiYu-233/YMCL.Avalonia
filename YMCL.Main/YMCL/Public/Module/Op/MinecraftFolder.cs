using System.IO;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Op;

public class MinecraftFolder
{
    public static async Task AddByUi(Control sender)
    {
        var list = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = MainLang.SelectMinecraftFolder, AllowMultiple = true });
        if (list.Count < 1) return;
        foreach (var storageFolder in list)
        {
            var path = storageFolder.Path.LocalPath;
            path = path.Trim().TrimEnd(Path.DirectorySeparatorChar);
            var folder = Path.GetFileName(path);
            var parentDirectoryPath = Path.GetDirectoryName(path);
            var name = string.Empty;
            if (parentDirectoryPath != null)
            {
                name = Path.GetFileName(parentDirectoryPath);
            }

            if (folder != ".minecraft")
            {
                if (Directory.Exists(Path.Combine(path, ".minecraft")))
                {
                    path = Path.Combine(path, ".minecraft");
                    name = folder;
                }
            }

            var textbox = new TextBox
            {
                Watermark = MainLang.DisplayName,
                TextWrapping = TextWrapping.Wrap,
                MaxLength = 60, Text = name
            };
            var textbox1 = new TextBox
            {
                Text = path,
                TextWrapping = TextWrapping.Wrap
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
                        textbox1
                    }
                },
                PrimaryButtonText = MainLang.Ok,
                CloseButtonText = MainLang.Cancel,
                IsPrimaryButtonEnabled = false
            };
            dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
            textbox.TextChanged += (_, _) =>
            {
                dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text);
            };
            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary) return;
            var entry = new Classes.Data.MinecraftFolder
                { Name = textbox.Text, Path = textbox1.Text };
            Data.MinecraftFolders.Add(entry);
            Data.SettingEntry.MinecraftFolder = entry;
        }

        await File.WriteAllTextAsync(ConfigPath.MinecraftFolderDataPath,
            JsonConvert.SerializeObject(Data.MinecraftFolders, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }

    public static void RemoveSelected()
    {
        var item = Data.SettingEntry.MinecraftFolder;
        if (item == null) return;
        Data.MinecraftFolders.Remove(item);
        if (Data.MinecraftFolders.Count == 0)
        {
            var path = Path.Combine(ConfigPath.UserDataRootPath, ".minecraft");
            IO.Disk.Setter.TryCreateFolder(path);
            var folder = new Classes.Data.MinecraftFolder { Name = "Minecraft Folder", Path = path };
            Data.MinecraftFolders.Add(folder);
            Data.SettingEntry.MinecraftFolder = folder;
        }
        else
        {
            Data.SettingEntry.MinecraftFolder = Data.MinecraftFolders[0];
        }

        File.WriteAllText(ConfigPath.MinecraftFolderDataPath,
            JsonConvert.SerializeObject(Data.MinecraftFolders, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }
}