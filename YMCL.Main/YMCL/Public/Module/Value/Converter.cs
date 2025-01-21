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
        var second = 0;
        second = (int)(ms / 1000);

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