using MinecraftLaunch.Classes.Models.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618
namespace YMCL.Main.Public.Classes
{
    public class Setting
    {
        public Avalonia.Media.Color AccentColor { get; set; } = Avalonia.Media.Color.FromRgb(22, 233, 184);
        public string Language { get; set; } = "Unset";
        public string MinecraftFolder { get; set; }
        public OpenFileWay OpenFileWay { get; set; }
        public JavaEntry Java { get; set; } = Const.AutoJava;
        public int AccountSelectionIndex { get; set; }
        public Theme Theme { get; set; } = Theme.Light;
        public double MaxMem { get; set; } = 1024;
        public string Version { get; set; } 
        public bool EnableIndependencyCore { get; set; } = true;
        public bool AlreadyWrittenIntoTheUrlScheme { get; set; } = false;
        public bool ShowGameOutput { get; set; } = true;
        public WindowTitleBarStyle WindowTitleBarStyle { get; set; } = WindowTitleBarStyle.Unset;
        public CustomHomePageWay CustomHomePage { get; set; } = CustomHomePageWay.None;
    }
    public class VersionSetting
    {
        public VersionSettingEnableIndependencyCore EnableIndependencyCore { get; set; } = VersionSettingEnableIndependencyCore.Global;
        public JavaEntry Java { get; set; } = new JavaEntry() { JavaPath = "Global" };
        public double MaxMem { get; set; } = -1; // -1 = Global
        public string AutoJoinServerIp { get; set; } = "";
    }
}
