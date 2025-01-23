using System.IO;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace YMCL.Public.Classes;

public sealed class GameSettingEntry : ReactiveObject
{
    [Reactive]
    [JsonProperty]
    public Enum.Setting.VersionSettingEnableIndependencyCore EnableIndependencyCore { get; set; } =
        Enum.Setting.VersionSettingEnableIndependencyCore.Global;

    [Reactive] [JsonProperty] public JavaEntry Java { get; set; } = new() { JavaVersion = "Global" };
    [Reactive] [JsonProperty] public double MaxMem { get; set; } = -2;
    [Reactive] [JsonProperty] public string AutoJoinServerIp { get; set; } = string.Empty;
    [Reactive] [JsonIgnore] public bool IsEnableIndependencyCore { get; set; }
    [Reactive] [JsonIgnore] public GameEntry GameEntry { get; set; }
}