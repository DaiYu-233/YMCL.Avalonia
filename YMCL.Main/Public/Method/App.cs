using System.Net;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace YMCL.Main.Public;

public partial class Method
{
    public static class App
    {
        public static async Task AppLoaded()
        {
            try
            {
                (Ui.FindControlByName(Const.Window.main, "ContentGridBorder") as Border).Background = null;
                (Ui.FindControlByName(Const.Window.main, "ContentGridBorder") as Border).BorderThickness = new Thickness(0);
                (Ui.FindControlByName(Const.Window.main, "PART_PaneRoot") as Panel).Background =
                    Application.Current.ActualThemeVariant == ThemeVariant.Dark
                        ? SolidColorBrush.Parse("#2c2c2c")
                        : SolidColorBrush.Parse("#FFE9F6FF");
            }
            catch
            {
            }
            Ui.CheckLauncher();
            await Task.Delay(200);
            _ = Const.Window.main.settingPage.launcherSettingPage.AutoUpdate();

            if (Const.Data.Setting.DownloadSource != DownloadSource.Mojang)
            {
                MinecraftLaunch.MirrorDownloadManager.IsUseMirrorDownloadSource = true;
            }
            Ui.SetWindowBackGroundImg();
        }

        public static async Task MainWindowLoading()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;
            
            _ = Const.Window.main.LoadCustomHomePage();
            _ = Const.Window.main.morePage._gameUpdateLog.LoadNews();

            _ = Const.Window.main.downloadPage.curseForgeFetcherPage.InitModFromCurseForge();
        }
    }
}