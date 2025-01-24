using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using YMCL.Public.Enum;
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
            "mods" => Public.Module.Mc.GameSetting.GetGameSpecialFolder(_model.GameEntry, GameSpecialFolder.ModsFolder),
            "saves" => Public.Module.Mc.GameSetting.GetGameSpecialFolder(_model.GameEntry,
                GameSpecialFolder.SavesFolder),
            "resourcepacks" => Public.Module.Mc.GameSetting.GetGameSpecialFolder(_model.GameEntry,
                GameSpecialFolder.ResourcePacksFolder),
            "shaderpacks" => Public.Module.Mc.GameSetting.GetGameSpecialFolder(_model.GameEntry,
                GameSpecialFolder.ShaderPacksFolder),
            "screenshots" => Public.Module.Mc.GameSetting.GetGameSpecialFolder(_model.GameEntry,
                GameSpecialFolder.ScreenshotsFolder),
            _ => Public.Module.Mc.GameSetting.GetGameSpecialFolder(_model.GameEntry, GameSpecialFolder.GameFolder)
        };
        YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(path);
        var launcher = TopLevel.GetTopLevel(this).Launcher;
        launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(path));
    }
}