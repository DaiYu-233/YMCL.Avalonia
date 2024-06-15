using MinecraftLaunch.Classes.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618
namespace YMCL.Main.Public.Classes
{
    public class Setting
    {
        public string Language { get; set; } = "zh-CN";
        public string MinecraftFolder { get; set; }
        public OpenFileWay OpenFileWay { get; set; }
        public JavaEntry Java { get; set; } = Const.AutoJava;
        public int AccountSelectionIndex { get; set; }
    }
}
