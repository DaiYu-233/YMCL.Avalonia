using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls.Notifications;
using MinecraftLaunch.Classes.Models.Game;
using YMCL.Main.Public.Classes;
using YMCL.Main.Views.DeskLyric;
using YMCL.Main.Views.Initialize;
using YMCL.Main.Views.Main;

namespace YMCL.Main.Public;

public abstract class Const
{
    public abstract class Data
    {
        public static Setting Setting { get; set; } = null;
        public static JavaEntry AutoJava { get; set; } = new() { JavaPath = "Auto", JavaVersion = "All" };
        public static List<UrlImageDataListEntry> UrlImageDataList { get; set; } = new();
        public static Platform Platform { get; set; }
    }

    public abstract class String
    {
        public static string AzureClientId { get; } = "c06d4d68-7751-4a8a-a2ff-d1b46688f428";
        public static string AppTitle { get; } = "Yu Minecraft Launcher";

        public static string UserDataRootPath { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DaiYu.Platform.YMCL");

        public static string SettingDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Setting.DaiYu");

        public static string MinecraftFolderDataPath { get; } =
            Path.Combine(UserDataRootPath, "YMCL.MinecraftFolder.DaiYu");

        public static string JavaDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Java.DaiYu");
        public static string JavaNewsDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.JavaNews.DaiYu");
        public static string AppPathDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.AppPath.DaiYu");
        public static string PlayerDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Player.DaiYu");

        public static string CustomHomePageXamlDataPath { get; } =
            Path.Combine(UserDataRootPath, "YMCL.CustomHomePageXaml.DaiYu");

        public static string AccountDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Account.DaiYu");
        public static string PluginDataPath { get; } = Path.Combine(UserDataRootPath, "YMCL.Plugin.DaiYu");
        public static string PluginFolderPath { get; } = Path.Combine(UserDataRootPath, "Plugin");
        public static string TempFolderPath { get; } = Path.Combine(UserDataRootPath, "Temp");
        public static string UpdateFolderPath { get; } = Path.Combine(UserDataRootPath, "Update");
        public static string VersionSettingFileName { get; } = "YMCLSetting.DaiYu";

        public static string GithubUpdateApiUrl { get; } =
            "https://api.github.com/repos/DaiYu-233/YMCL.Avalonia/releases?per_page=1";

        public static string MusicApiUrl { get; set; } = "http://music.api.daiyu.fun/";
        public static string CurseForgeApiKey { get; } = "$2a$10$ndSPnOpYqH3DRmLTWJTf5Ofm7lz9uYoTGvhSj0OjJWJ8WdO4ZTsr.";
    }

    public abstract class Window
    {
        public static InitializeWindow initialize = new();
        public static MainWindow main;
        public static DeskLyric deskLyric = new();
    }

    public abstract class Notification
    {
        public static WindowNotificationManager main { get; set; }
        public static WindowNotificationManager initialize { get; set; }
    }
}