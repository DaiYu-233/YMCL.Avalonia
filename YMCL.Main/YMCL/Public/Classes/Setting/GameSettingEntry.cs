using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Classes.Data;

namespace YMCL.Public.Classes.Setting;

public sealed class GameSettingEntry : ReactiveObject
{
    [Reactive]
    [JsonProperty]
    public Enum.Setting.VersionSettingEnableIndependencyCore EnableIndependencyCore { get; set; } =
        Enum.Setting.VersionSettingEnableIndependencyCore.Global;

    [Reactive] [JsonProperty] public JavaEntry Java { get; set; } = new() { JavaVersion = "Global" };
    [Reactive] [JsonProperty] public double MaxMem { get; set; } = -2;
    [Reactive] [JsonProperty] public string AutoJoinServerIp { get; set; } = string.Empty;
    [Reactive] [JsonProperty] public Enum.Setting.MaxMemWay MaxMemWay { get; set; } = Enum.Setting.MaxMemWay.Global;
    [Reactive] [JsonIgnore] public bool IsEnableIndependencyCore { get; set; }
    [Reactive] [JsonIgnore] public MinecraftEntry MinecraftEntry { get; set; }

}