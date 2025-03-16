using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using YMCL.Public.Enum;
using YMCL.Public.Module.IO.Disk;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public partial class OverView : UserControl
{
    private GameSettingModel _model;

    public OverView(GameSettingModel model)
    {
        InitializeComponent();
        _model = model;
        DataContext = model;
    }

    private void OpenVersionFolder(object? sender, RoutedEventArgs e)
    {
        var tag = ((Button)sender).Tag.ToString();
        var path = tag switch
        {
            "mods" => Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_model.MinecraftEntry,
                GameSpecialFolder.ModsFolder),
            "saves" => Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_model.MinecraftEntry,
                GameSpecialFolder.SavesFolder),
            "resourcepacks" => Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_model.MinecraftEntry,
                GameSpecialFolder.ResourcePacksFolder),
            "shaderpacks" => Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_model.MinecraftEntry,
                GameSpecialFolder.ShaderPacksFolder),
            "screenshots" => Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_model.MinecraftEntry,
                GameSpecialFolder.ScreenshotsFolder),
            _ => Public.Module.Mc.Utils.GetMinecraftSpecialFolder(_model.MinecraftEntry, GameSpecialFolder.GameFolder,
                true)
        };
        YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
        _ = Opener.OpenFolder(path);
    }

    public OverView()
    {
    }
}