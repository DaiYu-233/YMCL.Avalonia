using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.IO.Disk;

public class Getter
{
    public static double GetTotalMemory(DesktopRunnerType platform)
    {
        if (platform == DesktopRunnerType.Windows)
            try
            {
                long totalMemory = 0;
                using (var searcher =
                       new ManagementObjectSearcher("select TotalVisibleMemorySize from Win32_OperatingSystem"))
                {
                    foreach (var o in searcher.Get())
                    {
                        var share = (ManagementObject)o;
                        totalMemory = Convert.ToInt64(share["TotalVisibleMemorySize"]);
                    }
                }

                Console.WriteLine("MaxMem: " + totalMemory);

                return totalMemory;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return 0;
            }

        if (platform == DesktopRunnerType.Linux)
            // 尝试读取 /proc/meminfo 文件  
            try
            {
                var meminfo = File.ReadAllText("/proc/meminfo");

                // 使用 LINQ 查询来找到 "MemTotal" 行  
                var memTotalLine = meminfo.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .FirstOrDefault(line => line.StartsWith("MemTotal:"));

                // 如果找到 MemTotal 行，解析其值  
                if (memTotalLine == null) return 0;
                // 提取 MemTotal 后面的数字，并转换为长整型  
                var memTotalValueStr = memTotalLine.Split(':')[1].Trim().Split(' ')[0];
                var memTotalValue = long.Parse(memTotalValueStr);

                return memTotalValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading /proc/meminfo: " + ex.Message);
                return 0;
            }

        return 0;
    }

    public static string GetMacAddress()
    {
        var macAddress = string.Empty;
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            macAddress = ni.GetPhysicalAddress().ToString();
            break;
        }

        return macAddress.Replace("-", "").ToLower(); // 移除MAC地址中的"-"并转为小写
    }

    public static double GetDirectoryLength(string dirPath)
    {
        //判断给定的路径是否存在,如果不存在则退出
        if (!Directory.Exists(dirPath))
            return 0;

        //定义一个DirectoryInfo对象
        var di = new DirectoryInfo(dirPath);

        //通过GetFiles方法,获取di目录中的所有文件的大小
        var len = di.GetFiles().Aggregate<FileInfo, double>(0, (current, fi) => current + fi.Length);

        //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
        var dis = di.GetDirectories();
        if (dis.Length <= 0) return len;
        len += dis.Sum(t => GetDirectoryLength(t.FullName));
        return len;
    }

    public static Bitmap LoadBitmapFromAppFile(string uri)
    {
        var memoryStream = new MemoryStream();
        var stream = AssetLoader.Open(new Uri("resm:" + uri));
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return new Bitmap(memoryStream);
    }

    public static string GetCurrentPlatformAndArchitecture()
    {
        // 检测操作系统  
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 检测架构  
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return "win-x64";
            if (RuntimeInformation.ProcessArchitecture == Architecture.X86) return "win-x86";
            // 其他的 Windows 架构可能也需要处理（比如 ARM）  
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
                return "osx";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                return "linux-x64";
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
                return "linux-arm"; // 注意: 这里应该是 linux-arm 而不是 liux-arm  
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) return "linux-arm64";
        }
        // 其他操作系统可能需要额外处理  

        // 如果没有匹配项，返回未知或默认字符串  
        return "unknown";
    }

    public static List<string> GetAllFilesByExtension(string folderPath, string fileExtension)
    {
        List<string> files = [];

        // 检查路径是否存在
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("指定的文件夹路径不存在！");
            return files;
        }

        // 使用递归方法获取所有文件
        var dirInfo = new DirectoryInfo(folderPath);
        files.AddRange(GetFilesRecursive(dirInfo, fileExtension));

        return files;

        static List<string> GetFilesRecursive(DirectoryInfo dirInfo, string fileExtension)
        {
            List<string> files = [];

            // 获取当前目录中的所有指定后缀的文件
            var fileInfos = dirInfo.GetFiles(fileExtension, SearchOption.TopDirectoryOnly);
            files.AddRange(fileInfos.Select(fileInfo => fileInfo.FullName));

            // 递归获取子目录中的文件
            var subDirs = dirInfo.GetDirectories();
            foreach (var subDir in subDirs)
            {
                files.AddRange(GetFilesRecursive(subDir, fileExtension));
            }

            return files;
        }
    }
}