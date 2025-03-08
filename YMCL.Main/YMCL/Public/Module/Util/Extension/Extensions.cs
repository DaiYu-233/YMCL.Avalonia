using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Modrinth.Extensions;
using Modrinth.Models;
using YMCL.Public.Classes.Data.ResourceFetcher;
using String = System.String;

namespace YMCL.Public.Module.Util.Extension;

public static class Extensions
{
    public static string GetDirectUrl(this ModrinthResourceEntry searchResult)
    {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
        interpolatedStringHandler.AppendFormatted("https://modrinth.com");
        interpolatedStringHandler.AppendLiteral("/");
        interpolatedStringHandler.AppendFormatted(searchResult.ProjectType.ToModrinthString());
        interpolatedStringHandler.AppendLiteral("/");
        interpolatedStringHandler.AppendFormatted(searchResult.ProjectId);
        return interpolatedStringHandler.ToStringAndClear();
    }
    
    public static string ToByteUnit(this int bytes)
    {
        if (bytes == 0)
        {
            return "0 Bytes";
        }

        string[] units = ["Bytes", "KiB", "MiB", "GiB"];
        double size = bytes;
        var unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }
        return $"{size:0.##} {units[unitIndex]}";
    }
}
