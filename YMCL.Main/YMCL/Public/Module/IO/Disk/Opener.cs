using System.Diagnostics;
using System.IO;
using Avalonia.Platform.Storage;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.IO.Disk;

public class Opener
{
    public static async Task OpenFolder(string path)
    {
        if (Data.DesktopType == DesktopRunnerType.MacOs)
        {
            var process = new Process();
            process.StartInfo.FileName = "open";
            process.StartInfo.Arguments = $"\"{path}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }
        else
        {
            var launcher = TopLevel.GetTopLevel(YMCL.App.UiRoot).Launcher;
            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
        }
    }
}