using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using YMCL.Public.Classes;
using YMCL.Public.Langs;
using YMCL.ViewModels;

namespace YMCL.Public.Controls;

public partial class LogViewer : UserControl
{
    public LogViewerModel Model { get; }

    public LogViewer()
    {
        InitializeComponent();
        Model = new LogViewerModel(() => { ScrollViewer.ScrollToEnd(); });
        DataContext = Model;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var time = DateTime.Now;
        var path = (await TopLevel.GetTopLevel(App.UiRoot).StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = MainLang.ExportLogFile,
                SuggestedFileName = $"{time:yyyy-MM-ddTHH-mm-sszz}.log",
                DefaultExtension = "log"
            }))?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path)) return;
        _ = File.WriteAllTextAsync(path,
            $"---- Exported By Yu Minecraft Launcher (http://ymcl.daiyu.fun) ----\n" +
            $"---- Exported Time : {time:yyyy-MM-ddTHH:mm:sszzz} ----\n\n\n" +
            $"{string.Join("\n", Model.LogItems.Select(a => a.Original))}");
    }
}