﻿using System.IO;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc.Importer.zip;

namespace YMCL.Public.Module.Ui.Special;

public class DropHandler
{
    public static async Task Handle(string path)
    {
        var type = Path.GetExtension(path);
        switch (type)
        {
            case ".zip":
                await Main.Import(path);
                return;
            default:
                Toast($"{MainLang.UnsupportedFileType}: {Path.GetFileName(path)}");
                return;
        }
    }
}