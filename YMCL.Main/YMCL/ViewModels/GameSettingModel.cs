using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using DynamicData;
using MinecraftLaunch.Base.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.Public.Module;

namespace YMCL.ViewModels;

public class GameSettingModel : ReactiveObject
{
    private static GameSettingModel? _instance;
    public ObservableCollection<JavaEntry> JavaRuntimes { get; set; } = [];
    [Reactive] public GameSettingEntry GameSetting { get; set; }
    [Reactive] public MinecraftEntry MinecraftEntry { get; set; }
    [Reactive] public string LoaderType { get; set; }
    public Bitmap Icon => Public.Module.Mc.Icon.GetMinecraftIcon(new MinecraftDataEntry(MinecraftEntry));
    public static double SystemMaxMem => UiProperty.Instance.SystemMaxMem;

    public static GameSettingModel Instance => _instance;

    public GameSettingModel(MinecraftEntry entry)
    {
        MinecraftEntry = entry;
        LoaderType = MinecraftEntry.IsVanilla
            ? "Vanilla"
            : string.Join(" , ", (MinecraftEntry as ModifiedMinecraftEntry)?.ModLoaders.Select(a => $"{a.Type} {a.Version}")!);
        GameSetting = YMCL.Public.Module.Mc.GameSetting.GetGameSetting(entry);
        JavaRuntimes.Add(new JavaEntry { JavaStringVersion = "Global", JavaPath = MainLang.UseGlobalSetting });
        foreach (var javaRuntime in Data.JavaRuntimes)
        {
            JavaRuntimes.Add(javaRuntime);
        }

        _instance = this;
        var debouncer = new Debouncer(() =>
        {
            File.WriteAllText(Path.Combine(entry.MinecraftFolderPath, "versions",
                entry.Id, "YMCL.GameSetting.DaiYu"), JsonConvert.SerializeObject(GameSetting, Formatting.Indented));
        }, 100);
        GameSetting.PropertyChanged += (_, _) => { debouncer.Trigger(); };
    }
}