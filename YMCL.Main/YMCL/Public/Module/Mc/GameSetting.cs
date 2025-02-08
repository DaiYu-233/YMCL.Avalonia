using System.IO;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Classes;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.Mc;

public class GameSetting
{
    public static void InitGameSetting(MinecraftEntry entry)
    {
        File.WriteAllText(Path.Combine(entry.MinecraftFolderPath, "versions", entry.Id, "YMCL.GameSetting.DaiYu"),
            JsonConvert.SerializeObject(new GameSettingEntry(), Formatting.Indented));
    }

    public static GameSettingEntry GetGameSetting(MinecraftEntry entry)
    {
        if (!File.Exists(Path.Combine(entry.MinecraftFolderPath, "versions", entry.Id, "YMCL.GameSetting.DaiYu")))
        {
            InitGameSetting(entry);
        }

        try
        {
            return JsonConvert.DeserializeObject<GameSettingEntry>(File.ReadAllText(Path.Combine(
                entry.MinecraftFolderPath, "versions",
                entry.Id, "YMCL.GameSetting.DaiYu")));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            File.Delete(Path.Combine(
                entry.MinecraftFolderPath, "versions",
                entry.Id, "YMCL.GameSetting.DaiYu"));
            return GetGameSetting(entry);
        }
    }

    public static void HandleGameSetting(GameSettingEntry entry)
    {
        if (entry.EnableIndependencyCore == Enum.Setting.VersionSettingEnableIndependencyCore.Global)
        {
            entry.IsEnableIndependencyCore = Data.Setting.EnableIndependencyCore;
        }
        else
        {
            entry.IsEnableIndependencyCore =
                entry.EnableIndependencyCore == Enum.Setting.VersionSettingEnableIndependencyCore.On;
        }

        if (entry.MaxMem < 0)
        {
            entry.MaxMem = Data.Setting.MaxMem;
        }

        if (entry.Java.JavaStringVersion == "Global")
        {
            entry.Java = Data.Setting.Java;
        }
    }

    public static string GetGameSpecialFolder(MinecraftEntry entry, GameSpecialFolder folder,
        bool isForceEnableIndependencyCore = false)
    {
        var setting = GetGameSetting(entry);
        HandleGameSetting(setting);
        var isEnableIndependencyCore = isForceEnableIndependencyCore || setting.IsEnableIndependencyCore;
        var basePath = isEnableIndependencyCore
            ? Path.Combine(entry.MinecraftFolderPath, "versions", entry.Id)
            : entry.MinecraftFolderPath;
        var path = folder switch
        {
            GameSpecialFolder.GameFolder => basePath,
            GameSpecialFolder.ModsFolder => Path.Combine(basePath, "mods"),
            GameSpecialFolder.ResourcePacksFolder => Path.Combine(basePath, "resourcepacks"),
            GameSpecialFolder.SavesFolder => Path.Combine(basePath, "saves"),
            GameSpecialFolder.ScreenshotsFolder => Path.Combine(basePath, "screenshots"),
            GameSpecialFolder.ShaderPacksFolder => Path.Combine(basePath, "shaderpacks"),
            _ => basePath
        };
        IO.Disk.Setter.TryCreateFolder(path);
        return path;
    }
}