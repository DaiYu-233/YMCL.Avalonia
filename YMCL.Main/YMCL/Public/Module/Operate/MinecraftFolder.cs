using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO;

namespace YMCL.Public.Module.Operate;

public class MinecraftFolder
{
    public static async Task AddByUi(Control sender)
    {
        var list = await TopLevel.GetTopLevel(sender).StorageProvider.OpenFolderPickerAsync(
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
        textbox.TextChanged += (_, _) => { dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textbox.Text); };
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;
        var entry = new Public.Classes.MinecraftFolder
            { Name = textbox.Text, Path = list[0].Path.LocalPath };
        Data.MinecraftFolders.Add(entry);
        Data.Setting.MinecraftFolder = entry;
        await File.WriteAllTextAsync(ConfigPath.MinecraftFolderDataPath,
            JsonConvert.SerializeObject(Data.MinecraftFolders, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }

    public static void RemoveSelected()
    {
        var item = Data.Setting.MinecraftFolder;
        if (item == null) return;
        Data.MinecraftFolders.Remove(item);
        if (Data.MinecraftFolders.Count == 0)
        {
            var path = Path.Combine(ConfigPath.UserDataRootPath, ".minecraft");
            IO.Disk.Setter.TryCreateFolder(path);
            var folder = new Classes.MinecraftFolder { Name = "Minecraft Folder", Path = path };
            Data.MinecraftFolders.Add(folder);
            Data.Setting.MinecraftFolder = folder;
        }
        else
        {
            Data.Setting.MinecraftFolder = Data.MinecraftFolders[0];
        }

        File.WriteAllText(ConfigPath.MinecraftFolderDataPath,
            JsonConvert.SerializeObject(Data.MinecraftFolders, Formatting.Indented));
        Public.Module.Ui.Special.AggregateSearchUi.UpdateAllAggregateSearchEntries();
    }
}