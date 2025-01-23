using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using DynamicData;
using MinecraftLaunch.Classes.Enums;
using MinecraftLaunch.Classes.Models.Game;
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
    [Reactive] public GameEntry GameEntry { get; set; }
    public Bitmap Icon => GetGameIcon();
    public static double SystemMaxMem => UiProperty.Instance.SystemMaxMem;

    public static GameSettingModel Instance => _instance;

    public GameSettingModel(GameEntry entry)
    {
        GameEntry = entry;
        GameSetting = YMCL.Public.Module.Mc.GameSetting.GetGameSetting(entry);
        JavaRuntimes.Add(new JavaEntry { JavaVersion = "Global", JavaPath = MainLang.UseGlobalSetting });
        foreach (var javaRuntime in Data.JavaRuntimes)
        {
            JavaRuntimes.Add(javaRuntime);
        }
        _instance = this;
        var debouncer = new Debouncer(() =>
        {
            File.WriteAllText(Path.Combine(entry.GameFolderPath, "versions",
                entry.Id, "YMCL.GameSetting.DaiYu"), JsonConvert.SerializeObject(GameSetting, Formatting.Indented));
        }, 100);
        GameSetting.PropertyChanged += (_, _) => { debouncer.Trigger(); };
    }

    private Bitmap GetGameIcon()
    {
        var icon = GameEntry.MainLoaderType switch
        {
            LoaderType.Vanilla => GameEntry.Type switch
            {
                "release" => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.grass_block_side.png"),
                "snapshot" => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.crafting_table_front.png"),
                "bedrock" => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.dirt_path_side.png"),
                _ => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                    "YMCL.Public.Assets.McIcons.grass_block_side.png")
            },
            LoaderType.Forge =>
                Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile("YMCL.Public.Assets.McIcons.furnace_front.png"),
            LoaderType.OptiFine => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.OptiFineIcon.png"),
            LoaderType.Fabric => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.FabricIcon.png"),
            LoaderType.Quilt => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.QuiltIcon.png"),
            _ => Public.Module.IO.Disk.Getter.LoadBitmapFromAppFile(
                "YMCL.Public.Assets.McIcons.grass_block_side.png")
        };
        return icon;
    }
}