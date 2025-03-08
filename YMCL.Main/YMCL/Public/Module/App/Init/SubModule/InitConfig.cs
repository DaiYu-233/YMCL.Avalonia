using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Classes.Setting;

namespace YMCL.Public.Module.App.Init.SubModule;

public static class InitConfig
{
    public static void Dispatch()
    {
        CreateFolder();
        CreateFile();
    }

    public static void CreateFolder()
    {
        IO.Disk.Setter.TryCreateFolder(ConfigPath.UserDataRootPath);
        IO.Disk.Setter.TryCreateFolder(ConfigPath.PluginFolderPath);
        IO.Disk.Setter.TryCreateFolder(ConfigPath.TempFolderPath);
        IO.Disk.Setter.TryCreateFolder(ConfigPath.UpdateFolderPath);
    }

    public static void CreateFile()
    {
        File.WriteAllText(ConfigPath.AppPathDataPath,
            System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        if (!File.Exists(ConfigPath.SettingDataPath))
            File.WriteAllText(ConfigPath.SettingDataPath,
                JsonConvert.SerializeObject(new SettingEntry(), Formatting.Indented));
        if (!File.Exists(ConfigPath.FavouriteResourceDataPath))
            File.WriteAllText(ConfigPath.FavouriteResourceDataPath,
                JsonConvert.SerializeObject(new List<FavouriteResource>(), Formatting.Indented));
        if (!File.Exists(ConfigPath.MinecraftFolderDataPath))
        {
            var path = Path.Combine(ConfigPath.UserDataRootPath, ".minecraft");
            IO.Disk.Setter.TryCreateFolder(path);
            File.WriteAllText(ConfigPath.MinecraftFolderDataPath,
                JsonConvert.SerializeObject(
                    new List<MinecraftFolder>([
                        new MinecraftFolder { Name = "Minecraft Folder", Path = path }
                    ]), Formatting.Indented));
            var setting1 = JsonConvert.DeserializeObject<SettingEntry>(File.ReadAllText(ConfigPath.SettingDataPath));
            setting1.MinecraftFolder = new MinecraftFolder { Name = "Minecraft Folder", Path = path };
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
                JsonConvert.SerializeObject(new List<RecordSongEntry>(), Formatting.Indented));
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