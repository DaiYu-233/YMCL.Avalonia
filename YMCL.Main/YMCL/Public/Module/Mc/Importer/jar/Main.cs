using System.IO;
using System.IO.Compression;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc.Importer.zip;

namespace YMCL.Public.Module.Mc.Importer.jar;

public class Main
{
    public static async Task Import(string path)
    {
        if (Data.Setting.SelectedMinecraftId == "bedrock") return;

        var cr = await ShowDialogAsync(
            $"{MainLang.Import} → {Data.Setting.SelectedMinecraftId}",
            $"{MainLang.SureToImportTheFile}: {Path.GetFileName(path)}", b_primary: MainLang.Ok,
            b_cancel: MainLang.Cancel);

        if (cr != ContentDialogResult.Primary) return;

        await IO.Disk.Setter.CopyFileWithDialog(path,
            Path.Combine(Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                GameSpecialFolder.ModsFolder), Path.GetFileName(path)));
    }
}