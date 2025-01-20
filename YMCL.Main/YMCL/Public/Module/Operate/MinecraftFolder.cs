using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Public.Langs;

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
        Data.MinecraftFolders.Add(new Public.Classes.MinecraftFolder
            { Name = textbox.Text, Path = list[0].Path.LocalPath });
        await File.WriteAllTextAsync(ConfigPath.MinecraftFolderDataPath,
            JsonConvert.SerializeObject(Data.MinecraftFolders, Formatting.Indented));
    }

    public static void RemoveSelected()
    {
        var item = Data.Setting.MinecraftFolder;
        if (item == null) return;
        Data.MinecraftFolders.Remove(item);
        File.WriteAllText(ConfigPath.MinecraftFolderDataPath,
            JsonConvert.SerializeObject(Data.MinecraftFolders, Formatting.Indented));
    }
}