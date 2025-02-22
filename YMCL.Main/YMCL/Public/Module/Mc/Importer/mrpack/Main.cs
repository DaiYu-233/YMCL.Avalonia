using System.IO;
using System.IO.Compression;
using Avalonia.Controls.Notifications;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.Mc.Importer.mrpack;

public class Main
{
    public static async Task Import(string path)
    {
        using var archive = ZipFile.OpenRead(path);
        var modPackEntry = archive.GetEntry("modrinth.index.json");
        if (modPackEntry is not null)
        {
            await ModPack.Import(path);
            return;
        }

        Notice($"{MainLang.Unrecognized}: {Path.GetFileName(path)}", NotificationType.Warning);
    }
}