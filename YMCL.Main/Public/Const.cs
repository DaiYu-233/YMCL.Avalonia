using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YMCL.Main.Public
{
    internal class Const
    {
        public static Platform Platform { get; set; } = Platform.Unknown;
        public static string UserDataRootPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DaiYu.Platform.YMCL");
        public static string SettingDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Setting.DaiYu");
        public static string MinecraftFolderDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.MinecraftFolder.DaiYu");
    }
}
