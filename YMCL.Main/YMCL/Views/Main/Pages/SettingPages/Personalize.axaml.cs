using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.App.Init.SubModule;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Personalize : UserControl
{
    public static ObservableCollection<string> SelectedItems { get; } = [];

    public Personalize()
    {
        InitializeComponent();
        EditCustomBackGroundImgBtn.Click += async (_, _) =>
        {
            var list = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypes.ImageAll] });
            if (list.Count == 0) return;
            Data.SettingEntry.WindowBackGroundImgData =
                Public.Module.Value.Converter.BytesToBase64(File.ReadAllBytes(list[0].Path.LocalPath));
            Public.Module.Ui.Setter.SetBackGround();
        };
        EditCustomHomePageBtn.Click += (_, _) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchFileInfoAsync(new FileInfo(ConfigPath.CustomHomePageXamlDataPath));
        };
        MultiComboBox.ItemsSource = new List<string>
        {
            "ContentDialog",
            "NotificationCard",
            "NotificationBubble",
            "Popup"
        };
        if (!string.IsNullOrWhiteSpace(Data.SettingEntry.SpecialControlEnableTranslucent))
        {
            Data.SettingEntry.SpecialControlEnableTranslucent.Split(',').Select(x => x.Trim())
                .Distinct().ToList().ForEach(x => SelectedItems.Add(x));
        }

        var isDistinct = false;
        var debouncer = new Debouncer(() =>
        {
            isDistinct = true;
            Dispatcher.UIThread.Invoke(() =>
            {
                SelectedItems.Clear();
                if (!string.IsNullOrWhiteSpace(Data.SettingEntry.SpecialControlEnableTranslucent))
                {
                    Data.SettingEntry.SpecialControlEnableTranslucent.Split(',').Select(x => x.Trim())
                        .Distinct().ToList().ForEach(x => SelectedItems.Add(x));
                }
            });
            isDistinct = false;
        }, 10);
        SelectedItems.CollectionChanged += (_, _) =>
        {
            if (isDistinct) return;
            Data.SettingEntry.SpecialControlEnableTranslucent =
                SelectedItems.Count == 0 ? "" : string.Join(",", SelectedItems.Distinct());
            debouncer.Trigger();
        };
        RefreshCustomHomePageBtn.Click += (_, _) => { _ = InitUi.SetCustomHomePage(); };
    }
}