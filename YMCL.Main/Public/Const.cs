using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Main;

namespace YMCL.Main.Public
{
    internal class Const
    {
        public class Window
        {
            public static MainWindow main = new MainWindow();
        }
        public class Notification
        {
            public static WindowNotificationManager main { get; set; }
        }
        public static JavaEntry AutoJava = new JavaEntry() { JavaPath = MainLang.LetYMCLChooseJava, JavaVersion = "All" };
        public static Platform Platform { get; set; }
        public static string AzureClientId { get; } = "c06d4d68-7751-4a8a-a2ff-d1b46688f428";
        public static string AppTitle { get; } = "Yu Minecraft Launcher";
        public static string UserDataRootPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DaiYu.Platform.YMCL");
        public static string SettingDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Setting.DaiYu");
        public static string MinecraftFolderDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.MinecraftFolder.DaiYu");
        public static string JavaDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Java.DaiYu");
        public static string AppPathDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.AppPath.DaiYu");
        public static string PlayerDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Player.DaiYu");
        public static string CustomHomePageXamlDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.CustomHomePageXaml.DaiYu");
        public static string AccountDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Account.DaiYu");
        public static string VersionSettingFileName { get; } = "YMCLSetting.DaiYu";
        public static string GithubUpdateApiUrl { get; } = "https://api.github.com/repos/DaiYu-233/YMCL.Avalonia/releases?per_page=1";
        public static string MusicApiUrl { get; } = "https://music.api.daiyu.fun/";
        public static string CurseForgeApiKey { get; } = "$2a$10$ndSPnOpYqH3DRmLTWJTf5Ofm7lz9uYoTGvhSj0OjJWJ8WdO4ZTsr.";
    }
}
