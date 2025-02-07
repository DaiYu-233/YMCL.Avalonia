using YMCL.Public.Enum;

namespace YMCL.Public.Classes;

public class LogItemEntry
{
    public string Message { get; set; } = string.Empty;
    public string Original { get; set; }
    public string Time { get; set; }
    public LogType Type { get; set; }

    public void SetOriginal()
    {
        Original = $"[{Time}] [{Type}] {Message}";
    }
}