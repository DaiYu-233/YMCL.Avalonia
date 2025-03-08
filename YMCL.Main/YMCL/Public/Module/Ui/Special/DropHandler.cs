using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using Avalonia.Controls.Notifications;
using YMCL.Public.Langs;
using YMCL.Public.Module.Mc.Importer;

namespace YMCL.Public.Module.Ui.Special;

public class DropHandler
{
    public static async Task HandleFiles(string path)
    {
        var type = Path.GetExtension(path);
        switch (type)
        {
            case ".mrpack":
                await Mc.Importer.mrpack.Main.Import(path);
                return;
            case ".jar":
                await Mc.Importer.jar.Main.Import(path);
                return;
            case ".zip":
                await Mc.Importer.zip.Main.Import(path);
                return;
            default:
                Notice($"{MainLang.UnsupportedFileType}: {Path.GetFileName(path)}", NotificationType.Warning);
                return;
        }
    }

    public static void HandleText(string text)
    {
        if (text.Trim().StartsWith("authlib-injector:"))
        {
            var match = Regex.Match(HttpUtility.UrlDecode(text.Trim()), @"https?://[^\s:]+");
            if (!match.Success) return;
            var url = match.Value;
            Op.Account.YggdrasilLogin(YMCL.App.UiRoot!, server1: url);
        }
    }
}