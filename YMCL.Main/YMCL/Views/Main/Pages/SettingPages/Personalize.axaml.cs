using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using YMCL.Public.Langs;

namespace YMCL.Views.Main.Pages.SettingPages;

public partial class Personalize : UserControl
{
    private ComboBox[] List;

    public Personalize()
    {
        InitializeComponent();
        List =
        [
            ThemeComboBox,
            LauncherVisibilityComboBox,
            LyricAlignComboBox,
            CustomHomePageComboBox,
            CustomBackGroundImgComboBox
        ];
        EditCustomBackGroundImgBtn.Click += async (_, _) =>
        {
            var list = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { AllowMultiple = false, Title = MainLang.SelectJava });
            if (list.Count == 0) return;
            Data.Setting.WindowBackGroundImgData =
                Public.Module.Value.Converter.BytesToBase64(File.ReadAllBytes(list[0].Path.LocalPath));
            Public.Module.Ui.Setter.SetBackGround();
        };
        EditCustomHomePageBtn.Click += (_, _) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            launcher.LaunchFileInfoAsync(new FileInfo(ConfigPath.CustomHomePageXamlDataPath));
        };
        LanguageComboBox.SelectionChanged += (_, _) =>
        {
            Data.JavaRuntimes.FirstOrDefault(java => java.JavaVersion == "Auto").JavaPath = MainLang.LetYMCLChooseJava;
        };
    }
}