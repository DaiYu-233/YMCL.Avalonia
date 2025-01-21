using Avalonia.Media;
using MinecraftLaunch.Classes.Models.Game;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.App;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Public.Classes;

public class UiProperty : ReactiveObject
{
    [Reactive] public bool LaunchBtnIsEnable { get; set; }
    [Reactive] public double TaskEntryHeaderWidth { get; set; }
}