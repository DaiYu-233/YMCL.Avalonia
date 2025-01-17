using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace YMCL.Main.Public;

public abstract partial class Method
{
    public static class App
    {
        public static async Task AppLoaded()
        {
            try
            {
                (Ui.FindControlByName(Const.Window.main, "ContentGridBorder") as Border).Background = null;
                (Ui.FindControlByName(Const.Window.main, "ContentGridBorder") as Border).BorderThickness =
                    new Thickness(0);
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
            _=Const.Window.main.downloadPage.autoInstallPage.LoadVanlliaVersion();
            _=Const.Window.main.downloadPage.javaDownloaderPage.LoadJava();

            if (Const.Data.Setting.DownloadSource != DownloadSource.Mojang)
            {
                MinecraftLaunch.MirrorDownloadManager.IsUseMirrorDownloadSource = true;
            }

            Ui.SetWindowBackGroundImg();

            async void RefreshToken()
            {
                await Task.Delay(200);
                while (true)
                {
                    try
                    {
                        var handler = new HttpClientHandler()
                        {
                            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                        };
                        using var client = new HttpClient(handler);
                        client.DefaultRequestHeaders.Add("User-Agent", "Apifox/1.0.0 (https://apifox.com)");
                        client.DefaultRequestHeaders.Add("Accept", "*/*");
                        client.DefaultRequestHeaders.Add("Host", "edge.microsoft.com");
                        client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                        HttpResponseMessage response =
                            await client.GetAsync("https://edge.microsoft.com/translate/auth");
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Const.Data.TranslateToken = responseBody;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
            
            RefreshToken();
        }


        public static Task MainWindowLoading()
        {
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;

            Const.Window.main.LoadCustomHomePage();
            _ = Const.Window.main.morePage._gameUpdateLog.LoadNews();

            _ = Const.Window.main.downloadPage.curseForgeFetcherPage.InitModFromCurseForge();
            return Task.CompletedTask;
        }
    }
}