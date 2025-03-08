using System.IO;
using System.IO.Compression;
using Avalonia.Controls.Notifications;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Importer.zip;

public class Main
{
    public static async Task Import(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var modPackEntry = archive.GetEntry("manifest.json");
        if (modPackEntry is not null)
        {
            await ModPack.Import(path);
            return;
        }

        if (Data.SettingEntry.SelectedMinecraftId == "bedrock") return;

        var selection = await ShowDialogWithComboBox([MainLang.AsResourcePackImport, MainLang.AsShaderPackImport],
            $"{MainLang.Import} → {Data.SettingEntry.SelectedMinecraftId}",
            $"{MainLang.HopeHowToHandleTheFile}: {Path.GetFileName(path)}");

        if (selection == 0)
        {
            await IO.Disk.Setter.CopyFileWithDialog(path,
                Path.Combine(Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.ResourcePacksFolder), Path.GetFileName(path)));
        }

        if (selection == 1)
        {
            await IO.Disk.Setter.CopyFileWithDialog(path,
                Path.Combine(Mc.Utils.GetMinecraftSpecialFolder(Mc.Utils.GetCurrentMinecraft()!,
                    GameSpecialFolder.ShaderPacksFolder), Path.GetFileName(path)));
        }
    }
}