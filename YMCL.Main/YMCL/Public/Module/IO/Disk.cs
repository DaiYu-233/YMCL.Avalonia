using System.IO;
using System.Linq;
using YMCL.Public.Enum;
using System.Management;

namespace YMCL.Public.Module.IO;

public class Disk
{
    public static void TryCreateFolder(string path)
    {
        if (Directory.Exists(path)) return;
        var directoryInfo = new DirectoryInfo(path);
        directoryInfo.Create();
    }
    
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

                Console.WriteLine("系统最大内存: " + totalMemory);

                return totalMemory;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取系统内存信息时发生错误: " + ex.Message);
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
}