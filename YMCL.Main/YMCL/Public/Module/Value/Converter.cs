using System.IO;
using Avalonia.Media.Imaging;

namespace YMCL.Public.Module.Value;

public class Converter
{
    public static string BytesToBase64(byte[] imageBytes)
    {
        var base64String = Convert.ToBase64String(imageBytes);
        return base64String;
    }
    
    public static byte[] Base64ToBytes(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        return bytes;
    }
    
    public static string StandardizeUrl(string url)
    {
        // 检查字符串是否以协议开始，如果没有，则添加http协议
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "http://" + url; // 默认添加http协议
        }

        try
        {
            Uri uriResult = new Uri(url);
            return uriResult.AbsoluteUri; // 返回绝对URI，这将标准化URL
        }
        catch (UriFormatException e)
        {
            Console.WriteLine("The URL is still not valid: " + e.Message);
            throw new FormatException("The URL is still not valid: " + e.Message);
        }
    }
    
    public static Bitmap Base64ToBitmap(string base64)
    {
        var imageBytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(imageBytes);
        var bitmap = new Bitmap(ms);
        return bitmap;
    }
    
    public static string MsToTime(double ms)
    {
        var minute = 0;
        var second = (int)(ms / 1000);

        if (second > 60)
        {
            minute = second / 60;
            second = second % 60;
        }

        var secondStr = second < 10 ? $"0{second}" : $"{second}";
        var minuteStr = minute < 10 ? $"0{minute}" : $"{minute}";

        return $"{minuteStr}:{secondStr}";
    }
}