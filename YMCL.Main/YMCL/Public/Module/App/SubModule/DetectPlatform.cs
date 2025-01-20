using System.Runtime.InteropServices;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.App;

public class DetectPlatform
{
    public static void Main()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Running on Windows");
            Const.Data.Instance.DesktopType = DesktopRunnerType.Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("Running on Linux");
            Const.Data.Instance.DesktopType = DesktopRunnerType.Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("Running on macOS");
            Const.Data.Instance.DesktopType = DesktopRunnerType.MacOs;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Console.WriteLine("Running on FreeBSD");
            Const.Data.Instance.DesktopType = DesktopRunnerType.FreeBSD;
        }
        else
        {
            Console.WriteLine("Running on an unknown platform");
            Const.Data.Instance.DesktopType = DesktopRunnerType.Unknown;
        }
    }
}