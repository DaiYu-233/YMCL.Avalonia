using System.IO;

namespace YMCL.Public.Module.IO;

public class Disk
{
    public static void TryCreateFolder(string path)
    {
        if (Directory.Exists(path)) return;
        var directoryInfo = new DirectoryInfo(path);
        directoryInfo.Create();
    }
}