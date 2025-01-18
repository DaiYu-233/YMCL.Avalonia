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
}