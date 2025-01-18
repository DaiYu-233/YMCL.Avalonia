using System.IO;

namespace YMCL.Public.Const;

public class ConfigPath
{
    public static string UserDataRootPath { get; protected set; }
    public static string SettingDataPath { get; protected set; }
    public static string MinecraftFolderDataPath { get; protected set; }
    public static string JavaDataPath { get; protected set; }
    public static string AppPathDataPath { get; protected set; }
    public static string PlayerDataPath { get; protected set; }
    public static string CustomHomePageXamlDataPath { get; protected set; }
    public static string AccountDataPath { get; protected set; }
    public static string PluginDataPath { get; protected set; }
    public static string PluginFolderPath { get; protected set; }
    public static string TempFolderPath { get; protected set; }
    public static string UpdateFolderPath { get; protected set; }

    public static void InitPath()
    {
        UserDataRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DaiYu.Platform.YMCL");
        SettingDataPath = Path.Combine(UserDataRootPath, "YMCL.Setting.DaiYu");
        MinecraftFolderDataPath = Path.Combine(UserDataRootPath, "YMCL.MinecraftFolder.DaiYu");
        JavaDataPath = Path.Combine(UserDataRootPath, "YMCL.Java.DaiYu");
        AppPathDataPath = Path.Combine(UserDataRootPath, "YMCL.AppPath.DaiYu");
        PlayerDataPath = Path.Combine(UserDataRootPath, "YMCL.Player.DaiYu");
        CustomHomePageXamlDataPath = Path.Combine(UserDataRootPath, "YMCL.CustomHomePageXaml.DaiYu");
        AccountDataPath = Path.Combine(UserDataRootPath, "YMCL.Account.DaiYu");
        PluginDataPath = Path.Combine(UserDataRootPath, "YMCL.Plugin.DaiYu");
        PluginFolderPath = Path.Combine(UserDataRootPath, "Plugin");
        TempFolderPath = Path.Combine(UserDataRootPath, "Temp");
        UpdateFolderPath = Path.Combine(UserDataRootPath, "Update");
    }
}