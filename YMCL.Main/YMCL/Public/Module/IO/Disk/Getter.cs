using System.IO;
using System.Linq;
using System.Management;
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
}