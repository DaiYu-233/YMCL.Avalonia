using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.IO.Disk;

public class Setter
{
    public static void TryCreateFolder(string path)
    {
        if (Directory.Exists(path)) return;
        var directoryInfo = new DirectoryInfo(path);
        directoryInfo.Create();
    }

    public static void ClearFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine(MainLang.FolderNotExist + folderPath);
            return;
        }

        foreach (var file in Directory.GetFiles(folderPath))
        {
            File.Delete(file);
        }

        foreach (var dir in Directory.GetDirectories(folderPath))
        {
            ClearFolder(dir);
            Directory.Delete(dir);
        }
    }

    public static async Task<bool> CopyFileWithDialog(string source, string target)
    {
        var path = target;
        if (File.Exists(target))
        {
            var cr = await ShowDialogAsync(MainLang.Conflict, MainLang.FileConflictTip, b_primary: MainLang.Cover,
                b_secondary: MainLang.Rename, b_cancel: MainLang.Cancel);
            if (cr == ContentDialogResult.Primary)
            {
                if (source == path) return false;
                File.Copy(source, target, true);
            }else if (cr == ContentDialogResult.None)
            {
                return false;
            }
            else
            {
                var textBox = new TextBox
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                    Text = Path.GetFileName(target), HorizontalAlignment = HorizontalAlignment.Stretch, Width = 500
                };
                var cr1 = await ShowDialogAsync(MainLang.Rename, p_content: textBox, b_cancel: MainLang.Cancel,
                    b_primary: MainLang.Ok);
                if (cr1 != ContentDialogResult.Primary) return false;
                path = Path.Combine(Path.GetDirectoryName(target)!, textBox.Text);
                return await CopyFileWithDialog(source, path);
            }
        }
        else
        {
            if (source == path) return false;
            File.Copy(source, path, true);
        }

        Notice(MainLang.ImportFinish, NotificationType.Success);
        return true;
    }
}