using System.IO;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using YMCL.Public.Classes;

namespace YMCL.Public.Module.Mc;

public class GameSetting
{
    public static void InitGameSetting(GameEntry entry)
    {
        File.WriteAllText(Path.Combine(entry.GameFolderPath, "versions", entry.Id, "YMCL.GameSetting.DaiYu"),
            JsonConvert.SerializeObject(new GameSettingEntry(), Formatting.Indented));
    }

    public static GameSettingEntry GetGameSetting(GameEntry entry)
    {
        if(!File.Exists(Path.Combine(entry.GameFolderPath, "versions", entry.Id, "YMCL.GameSetting.DaiYu")))
        {
            InitGameSetting(entry);
        }
        return JsonConvert.DeserializeObject<GameSettingEntry>(File.ReadAllText(Path.Combine(
            entry.GameFolderPath, "versions",
            entry.Id, "YMCL.GameSetting.DaiYu")));
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

        if (entry.Java.JavaVersion == "Global")
        {
            entry.Java = Data.Setting.Java;
        }
    }
}