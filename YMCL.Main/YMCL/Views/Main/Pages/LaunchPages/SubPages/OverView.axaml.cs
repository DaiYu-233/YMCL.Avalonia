using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
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
        YMCL.Public.Module.IO.Disk.Setter.TryCreateFolder(Path.Combine(_model.GameEntry.GameFolderPath, "versions", _model.GameEntry.Id, tag!));
        var launcher = TopLevel.GetTopLevel(this).Launcher;
        launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Path.Combine(_model.GameEntry.GameFolderPath, "versions", _model.GameEntry.Id, tag!)));
    }
}