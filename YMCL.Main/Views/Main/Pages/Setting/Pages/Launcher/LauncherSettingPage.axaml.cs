using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.Setting.Pages.Launcher;

public partial class LauncherSettingPage : UserControl
{
    public LauncherSettingPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    public async Task AutoUpdate()
    {
        if (!Const.Data.Setting.EnableAutoCheckUpdate) return;
        var updateAvailable = await Method.Ui.CheckUpdateAsync();
        if (!updateAvailable.Item1) return;
        if (!updateAvailable.Item2) return;
        if (Const.Data.Setting.SkipUpdateVersion == updateAvailable.Item3) return;
        var dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion, updateAvailable.Item3
            , b_cancel: MainLang.Cancel, b_secondary: MainLang.SkipThisVersion,
            b_primary: MainLang.Ok);
        if (dialog == ContentDialogResult.Primary)
        {
            var updateAppAsync = await Method.Ui.UpdateAppAsync();
            if (!updateAppAsync) Method.Ui.Toast(MainLang.UpdateFail);
        }
        else if (dialog == ContentDialogResult.Secondary)
        {
            Const.Data.Setting.SkipUpdateVersion = updateAvailable.Item3;
            File.WriteAllText(Const.String.SettingDataPath,
                JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
        }
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
        OpenUserDataFolderBtn.Click += async (s, e) =>
        {
            var launcher = TopLevel.GetTopLevel(this).Launcher;
            await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(Const.String.UserDataRootPath));
        };
        CheckUpdateBtn.Click += async (_, _) =>
        {
            CheckUpdateBtn.IsEnabled = false;
            var ring = new ProgressRing();
            CheckUpdateBtn.Width = CheckUpdateBtn.Bounds.Width;
            CheckUpdateBtn.Content = ring;
            ring.Height = 17;
            ring.Width = 17;
            var (success, checkUpdateAsyncStatus, _, checkUpdateAsyncMsg) = await Method.Ui.CheckUpdateAsync();
            CheckUpdateBtn.IsEnabled = true;
            CheckUpdateBtn.Content = MainLang.CheckUpdate;
            if (!success)
            {
                Method.Ui.Toast(MainLang.CheckUpdateFail);
                return;
            }

            if (!checkUpdateAsyncStatus)
            {
                return;
            }

            var dialog = await Method.Ui.ShowDialogAsync(MainLang.FoundNewVersion, checkUpdateAsyncMsg
                , b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (dialog == ContentDialogResult.Primary)
            {
                var updateAppAsync = await Method.Ui.UpdateAppAsync();
                if (!updateAppAsync) Method.Ui.Toast(MainLang.UpdateFail);
            }
        };
    }

    private void ControlProperty()
    {
        UserDataFolderPath.Text = Const.String.UserDataRootPath;
        var lenth = Method.Value.GetDirectoryLength(Const.String.UserDataRootPath);
        var userDataSize = Math.Round(lenth / 1024, 2) >= 512
            ? $"{Math.Round(lenth / 1024 / 1024, 2)} Mib"
            : $"{Math.Round(lenth / 1024, 2)} Kib";
        UserDataSize.Text = userDataSize;
        var resourceName = "YMCL.Main.Public.Texts.DateTime.txt";
        var _assembly = Assembly.GetExecutingAssembly();
        var stream = _assembly.GetManifestResourceStream(resourceName);
        using (var reader = new StreamReader(stream!))
        {
            var result = reader.ReadToEnd();
            Version.Text = $"v{result.Trim()}";
        }
    }
}