using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace YMCL.Main.Public;

public partial class Method
{
    public static partial class Value
    {
        public static Color ColorVariant(Color color, float percent)
        {
            // 确保百分比在-1到1之间  
            percent = Math.Max(-1f, Math.Min(1f, percent));

            // 计算调整后的RGB值  
            var adjust = 1f + percent; // 亮化是1+percent，暗化是1+(negative percent)，即小于1  
            var r = (int)Math.Round(color.R * adjust);
            var g = (int)Math.Round(color.G * adjust);
            var b = (int)Math.Round(color.B * adjust);

            // 确保RGB值在有效范围内  
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            // 创建一个新的颜色（保持Alpha通道不变）  
            return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }

        public static string GetTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString(); //精确到秒
        }

        public static Bitmap Base64ToBitmap(string base64)
        {
            var imageBytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(imageBytes))
            {
                var bitmap = new Bitmap(ms);
                return bitmap;
            }
        }

        public static string BytesToBase64(byte[] imageBytes)
        {
            var base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }

        public static string MsToTime(double ms) //转换为分秒格式
        {
            var minute = 0;
            var second = 0;
            second = (int)(ms / 1000);

            var secondStr = string.Empty;
            var minuteStr = string.Empty;

            if (second > 60)
            {
                minute = second / 60;
                second = second % 60;
            }

            secondStr = second < 10 ? $"0{second}" : $"{second}";
            minuteStr = minute < 10 ? $"0{minute}" : $"{minute}";

            return $"{minuteStr}:{secondStr}";
        }

        public static double GetTotalMemory(Platform platform)
        {
            if (platform == Platform.Windows)
                try
                {
                    long totalMemory = 0;
                    using (var searcher =
                           new ManagementObjectSearcher("select TotalVisibleMemorySize from Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject share in searcher.Get())
                            totalMemory = Convert.ToInt64(share["TotalVisibleMemorySize"]);
                    }

                    Console.WriteLine("系统最大内存: " + totalMemory);

                    return totalMemory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取系统内存信息时发生错误: " + ex.Message);
                    return 0;
                }

            if (platform == Platform.Linux)
                // 尝试读取 /proc/meminfo 文件  
                try
                {
                    var meminfo = File.ReadAllText("/proc/meminfo");

                    // 使用 LINQ 查询来找到 "MemTotal" 行  
                    var memTotalLine = meminfo.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .FirstOrDefault(line => line.StartsWith("MemTotal:"));

                    // 如果找到 MemTotal 行，解析其值  
                    if (memTotalLine != null)
                    {
                        // 提取 MemTotal 后面的数字，并转换为长整型  
                        var memTotalValueStr = memTotalLine.Split(':')[1].Trim().Split(' ')[0];
                        var memTotalValue = long.Parse(memTotalValueStr);

                        return memTotalValue;
                    }

                    return 0;
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
            double len = 0;

            //定义一个DirectoryInfo对象
            var di = new DirectoryInfo(dirPath);

            //通过GetFiles方法,获取di目录中的所有文件的大小
            foreach (var fi in di.GetFiles()) len += fi.Length;

            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            var dis = di.GetDirectories();
            if (dis.Length > 0)
                for (var i = 0; i < dis.Length; i++)
                    len += GetDirectoryLength(dis[i].FullName);
            return len;
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
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    return "osx-x64";
                // 注意: OSX 上的 ARM 架构可能需要特定检测，因为目前可能是 Apple Silicon (M1/M2)  
                if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) return "osx-arm64";
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

        public static async Task<Bitmap> LoadImageFromUrlAsync(string url)
        {
            if (url is "Url" or "null") return null;
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var imageData = await response.Content.ReadAsByteArrayAsync();

                        using (var stream = new MemoryStream(imageData))
                        {
                            var bitmap = new Bitmap(stream);
                            return bitmap;
                        }
                    }

                    Console.WriteLine($"Failed to load image. Status code: {response.StatusCode}");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return null;
        }

        public static string ConvertToWanOrYi(double number)
        {
            if (number < 7500)
                return number.ToString();
            if (number < 100000000)
                return $"{Math.Round(number / 10000, 1)}万";
            return $"{Math.Round(number / 100000000, 1)}亿";
        }
    }
}