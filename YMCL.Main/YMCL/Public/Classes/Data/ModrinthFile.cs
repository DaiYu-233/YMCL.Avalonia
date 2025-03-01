using YMCL.Public.Module.Util.Extension;

namespace YMCL.Public.Classes.Data;

public class ModrinthFile
{
    public int Size { get; set; }
    public string DisplaySize => Size.ToByteUnit();
    public string FileName { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public int Downloads { get; set; }
    public string DisplayDownloads => Convert.ToDouble(Downloads).ToUnit();
    public DateTime UpdateTime { get; set; }
}