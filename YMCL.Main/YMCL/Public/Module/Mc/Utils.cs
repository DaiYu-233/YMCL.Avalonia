using System.IO;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Parser;
using YMCL.Public.Enum;

namespace YMCL.Public.Module.Mc;

public class Utils
{
    public static string GetMinecraftSpecialFolder(MinecraftEntry entry, GameSpecialFolder folder,
        bool isForceEnableIndependencyCore = false)
    {
        var setting = MinecraftSetting.GetGameSetting(entry);
        MinecraftSetting.HandleGameSetting(setting);
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

    public static MinecraftEntry? GetCurrentMinecraft()
    {
        if (Data.Setting.SelectedMinecraftId == "bedrock") return null;
        var parser = new MinecraftParser(Data.Setting.MinecraftFolder.Path);
        return parser.GetMinecraft(Data.Setting.SelectedMinecraftId);
    }
}