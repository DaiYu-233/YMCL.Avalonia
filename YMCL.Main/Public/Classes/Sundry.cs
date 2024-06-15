using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618
namespace YMCL.Main.Public.Classes
{
    public class FolderInfo()
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class FileInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
}
