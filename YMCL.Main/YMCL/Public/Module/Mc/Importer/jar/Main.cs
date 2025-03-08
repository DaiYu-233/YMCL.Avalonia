using System.IO;
using System.IO.Compression;
using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc.Importer.zip;

namespace YMCL.Public.Module.Mc.Importer.jar;

public class Main
{
    public static async Task Import(string path)
    {
        if (Data.SettingEntry.SelectedMinecraftId == "bedrock") return;

        if (!Data.UiProperty.IsAllImport)
        {
            var cr = await ShowDialogAsync(
                $"{MainLang.Import} → {Data.SettingEntry.SelectedMinecraftId}",
                $"{MainLang.SureToImportTheFile}: {Path.GetFileName(path)}", b_primary: MainLang.Ok,
                b_cancel: MainLang.Cancel, b_secondary: MainLang.AllImport);

            if (cr == ContentDialogResult.None) return;
            if (cr == ContentDialogResult.Secondary) Data.UiProperty.IsAllImport = true;
        }

        await IO.Disk.Setter.CopyFileWithDialog(path,
            Path.Combine(Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                GameSpecialFolder.ModsFolder), Path.GetFileName(path)));
    }
}