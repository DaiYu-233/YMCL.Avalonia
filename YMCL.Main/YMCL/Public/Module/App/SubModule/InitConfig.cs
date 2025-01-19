using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
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
        Disk.TryCreateFolder(ConfigPath.UserDataRootPath);
        Disk.TryCreateFolder(ConfigPath.PluginFolderPath);
        Disk.TryCreateFolder(ConfigPath.TempFolderPath);
        Disk.TryCreateFolder(ConfigPath.UpdateFolderPath);
    }

    public static void CreateFile()
    {
        if (!File.Exists(ConfigPath.SettingDataPath))
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(new Setting(), Formatting.Indented));
        if (!File.Exists(ConfigPath.MinecraftFolderDataPath) || JsonConvert
                .DeserializeObject<List<MinecraftFolder>>(
                    File.ReadAllText(ConfigPath.MinecraftFolderDataPath)).Count == 0)
        {
            var path = Path.Combine(ConfigPath.UserDataRootPath, ".minecraft");
            Disk.TryCreateFolder(path);
            File.WriteAllText(ConfigPath.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(
                    new List<MinecraftFolder>([
                        new MinecraftFolder { Name = "Minecraft Folder", Path = path }
                    ]), Formatting.Indented));
            var setting1 = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(ConfigPath.SettingDataPath));
            setting1.MinecraftFolder = new MinecraftFolder { Name = MainLang.MinecraftFolder, Path = path };
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(setting1, Formatting.Indented));
        }

        if (!File.Exists(ConfigPath.JavaDataPath))
            File.WriteAllText(ConfigPath.JavaDataPath,
                JsonConvert.SerializeObject(new List<JavaEntry>(), Formatting.Indented));
        if (!File.Exists(ConfigPath.PluginDataPath))
            File.WriteAllText(ConfigPath.PluginDataPath,
                JsonConvert.SerializeObject(new List<string>(), Formatting.Indented));
        if (!File.Exists(ConfigPath.PlayerDataPath))
            File.WriteAllText(ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(new List<Player.PlaySongListViewItemEntry>(), Formatting.Indented));
        if (!File.Exists(ConfigPath.AccountDataPath))
            File.WriteAllText(ConfigPath.AccountDataPath,
                JsonConvert.SerializeObject(
                    new List<AccountInfo>
                    {
                        new()
                        {
                            Name = "Steve", AccountType = Enum.Setting.AccountType.Offline,
                            AddTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                        }
                    }, Formatting.Indented));
        if (File.Exists(ConfigPath.CustomHomePageXamlDataPath)) return;
        const string resourceName = "YMCL.Public.Texts.CustomHomePageDefault.axaml";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        var result = reader.ReadToEnd();
        File.WriteAllText(ConfigPath.CustomHomePageXamlDataPath, result);
        
    }
}