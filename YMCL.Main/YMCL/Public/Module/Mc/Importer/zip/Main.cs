using System.IO;
using System.IO.Compression;
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

        Toast($"{MainLang.Unrecognized}: {Path.GetFileName(path)}");
    }
}