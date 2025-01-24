using System.IO;
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
}