using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Controls;

public partial class ScreenshotEntry : UserControl
{
    private readonly string _path;
    private readonly Action _refreshAction;

    public ScreenshotEntry(string name, string path, Action refreshAction, Action selectedAction)
    {
        _path = path;
        _refreshAction = refreshAction;
        InitializeComponent();
        FileNameTextBlock.Text = name;
        Image.Source = new Bitmap(path);
        Image.PointerPressed += (_, _) => { selectedAction.Invoke(); };
    }

    public ScreenshotEntry()
    {
    }

    private void OpenFileInFileExplore(object? sender, RoutedEventArgs e)
    {
        var launcher = TopLevel.GetTopLevel(this).Launcher;
        launcher.LaunchFileInfoAsync(new FileInfo(_path));
    }

    private async void DelFile(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_path)) return;

        var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
            ? MainLang.MoveToRecycleBin
            : MainLang.DeleteSelect;
        var dialog = await ShowDialogAsync(title, $"• {Path.GetFileName(_path)}\n", b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok);
        if (dialog != ContentDialogResult.Primary) return;

        if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
        {
            FileSystem.DeleteFile(_path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
        }
        else
        {
            File.Delete(_path);
        }

        _refreshAction.Invoke();
    }
}