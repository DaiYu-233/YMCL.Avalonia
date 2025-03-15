using System.IO;
using System.Text.RegularExpressions;
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

    public static string StandardizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        // 检查字符串是否以协议开始，如果没有，则添加http协议
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "http://" + url; // 默认添加http协议
        }

        try
        {
            var uriResult = new Uri(url);
            return uriResult.AbsoluteUri; // 返回绝对URI，这将标准化URL
        }
        catch (UriFormatException e)
        {
            Console.WriteLine("The URL is still not valid: " + e.Message);
            throw new FormatException("The URL is still not valid: " + e.Message);
        }
    }

    public static Bitmap? Base64ToBitmap(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return null;
        }

        var imageBytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(imageBytes);
        var bitmap = new Bitmap(ms);
        return bitmap;
    }

    public static (string host, int port) UrlToHostAndPort(string input)
    {
        var host = "";
        var port = 25565; // 默认端口

        if (Regex.IsMatch(input, @"^(\d{1,3}\.){3}\d{1,3}:(\d+)$"))
        {
            var parts = input.Split(':');
            host = parts[0];
            port = int.Parse(parts[1]);
        }
        else if (Regex.IsMatch(input, @"^(.*):(\d+)$"))
        {
            var parts = input.Split(':');
            host = parts[0];
            port = int.Parse(parts[1]);
        }
        else
        {
            host = input;
        }
        return (host, port);
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

    public static string BitmapToBase64(Bitmap bitmap)
    {
        if (bitmap == null)
            return string.Empty;
        using var ms = new MemoryStream();
        bitmap.Save(ms);
        var imageBytes = ms.ToArray();
        return Convert.ToBase64String(imageBytes);
    }
}