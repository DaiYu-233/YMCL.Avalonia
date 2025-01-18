using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Module.IO;

namespace YMCL.Public.Module.App;

public static class InitConfig
{
    public static void Dispatch()
    {
        CreateFolder();
        CreateFile();
    }
    public static void CreateFolder()
    {
        Disk.TryCreateFolder(Const.ConfigPath.UserDataRootPath);
        Disk.TryCreateFolder(Const.ConfigPath.PluginFolderPath);
        Disk.TryCreateFolder(Const.ConfigPath.TempFolderPath);
        Disk.TryCreateFolder(Const.ConfigPath.UpdateFolderPath);
    }

    public static void CreateFile()
    {
        if (!File.Exists(Const.ConfigPath.SettingDataPath))
            File.WriteAllText(Const.ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(new Setting(), Formatting.Indented));
        if (!File.Exists(Const.ConfigPath.MinecraftFolderDataPath) || JsonConvert
                .DeserializeObject<List<string>>(File.ReadAllText(Const.ConfigPath.MinecraftFolderDataPath)).Count == 0)
        {
            var path = Path.Combine(Const.ConfigPath.UserDataRootPath, ".minecraft");
            Disk.TryCreateFolder(path);
            File.WriteAllText(Const.ConfigPath.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(new List<string> { path }, Formatting.Indented));
            var setting1 = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.ConfigPath.SettingDataPath));
            setting1.MinecraftFolder = path;
            File.WriteAllText(Const.ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(setting1, Formatting.Indented));
        }
        if (!File.Exists(Const.ConfigPath.JavaDataPath))
            File.WriteAllText(Const.ConfigPath.JavaDataPath,
                JsonConvert.SerializeObject(new List<JavaEntry>(), Formatting.Indented));
        if (!File.Exists(Const.ConfigPath.PluginDataPath))
            File.WriteAllText(Const.ConfigPath.PluginDataPath,
                JsonConvert.SerializeObject(new List<string>(), Formatting.Indented));
        if (!File.Exists(Const.ConfigPath.PlayerDataPath))
            File.WriteAllText(Const.ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(new List<Player.PlaySongListViewItemEntry>(), Formatting.Indented));
        if (!File.Exists(Const.ConfigPath.AccountDataPath))
            File.WriteAllText(Const.ConfigPath.AccountDataPath,
                JsonConvert.SerializeObject(
                    new List<AccountInfo>
                    {
                        new()
                        {
                            Name = "Steve", AccountType = Enum.Setting.AccountType.Offline,
                            AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                        }
                    }, Formatting.Indented));
        if (File.Exists(Const.ConfigPath.CustomHomePageXamlDataPath)) return;
        const string resourceName = "YMCL.Public.Texts.CustomHomePageDefault.axaml";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        var result = reader.ReadToEnd();
        File.WriteAllText(Const.ConfigPath.CustomHomePageXamlDataPath, result);
    }
}