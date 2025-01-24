using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Views.Main.Pages.LaunchPages.SubPages;

namespace YMCL.Public.Classes;

public class LocalSaveEntry : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public Bitmap Icon { get; set; }
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public Save.SaveInfo SaveInfo { get; set; }
    [Reactive] public Action Callback { get; set; }

    public async Task Delete()
    {
        var text = $"• {System.IO.Path.GetFileName(Name)}";

        var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
            ? MainLang.MoveToRecycleBin
            : MainLang.DeleteSelect;
        var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok);
        if (dialog != ContentDialogResult.Primary) return;

        if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
        {
            FileSystem.DeleteDirectory(Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
        }
        else
        {
            Directory.Delete(Path);
        }

        Callback?.Invoke();
    }

    public async Task ShowInfo()
    {
        var text =
            $"{MainLang.Name}: {SaveInfo.FolderName}\n{MainLang.CreateTime}: {SaveInfo.CreationTime}\n" +
            $"{MainLang.LastModifiedTime}: {SaveInfo.LastWriteTime}\n{MainLang.PlayerCount}: {SaveInfo.DatFileCount}\n" +
            $"{MainLang.DataPackCount}: {SaveInfo.ZipFileCount}";
        await ShowDialogAsync(MainLang.SaveInfo, text, b_primary: MainLang.Ok);
        Callback?.Invoke();
    }
    public void OpenFolder()
    {
        var path = SaveInfo.FolderPath;
        YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
        var launcher = TopLevel.GetTopLevel(App.UiRoot).Launcher;
        launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
    }
}