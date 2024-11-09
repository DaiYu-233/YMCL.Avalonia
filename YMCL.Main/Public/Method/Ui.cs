using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Main.Public.Controls.TaskManage;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Public;

public partial class Method
{
    public static partial class Ui
    {
        public static async void PageLoadAnimation((double, double, double, double) original,
            (double, double, double, double) target, TimeSpan time, Control control, bool visibility = false)
        {
            var (ol, ot, or, ob) = original;
            var (tl, tt, tr, tb) = target;

            var transitions = control.Transitions;

            if (control != null && control.Transitions != null)
            {
                control.Transitions.Clear();
                control.Margin = new Thickness(ol, ot, or, ob);
                control.Opacity = 0;
                control.Transitions.Add(new ThicknessTransition
                {
                    Duration = time,
                    Easing = new SineEaseInOut(),
                    Property = Layoutable.MarginProperty
                });
                control.Transitions.Add(new DoubleTransition
                {
                    Duration = time,
                    Easing = new SineEaseInOut(),
                    Property = Visual.OpacityProperty
                });
                if (visibility) control.IsVisible = true;
                control.Margin = new Thickness(tl, tt, tr, tb);
                control.Opacity = 1;
                await Task.Delay(time);
                control.Transitions = transitions;
            }
        }

        public static Control FindControlByName(Visual parent, string name)
        {
            var visuals = new Queue<Visual>();
            visuals.Enqueue(parent);

            while (visuals.Count > 0)
            {
                var current = visuals.Dequeue();
                var control = current as Control;

                if (control != null)
                {
                    foreach (var child in control.GetVisualChildren())
                    {
                        visuals.Enqueue((Visual)child);
                    }

                    var result = control;
                    if (result != null && result.Name == name)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public static void Toast(string msg, WindowNotificationManager p_notification = null,
            NotificationType type = NotificationType.Information, bool time = true,
            string title = "Yu Minecraft Launcher")
        {
            var notification = p_notification == null ? Const.Notification.main : p_notification;
            var showTitle = Const.String.AppTitle;
            if (!string.IsNullOrWhiteSpace(title)) showTitle = title;
            if (time) showTitle += $" - {DateTime.Now.ToString("HH:mm:ss")}";
            notification.Show(new Notification(showTitle, msg, type));
        }

        public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = "Content",
            Control p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null,
            Window p_window = null)
        {
            var content = p_content == null
                ? new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Text = msg
                }
                : p_content;
            var window = p_window == null ? Const.Window.main : p_window;
            var dialog = new ContentDialog
            {
                PrimaryButtonText = b_primary,
                Content = content,
                DefaultButton = ContentDialogButton.Primary,
                CloseButtonText = b_cancel,
                SecondaryButtonText = b_secondary,
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Title = title
            };
            var result = await dialog.ShowAsync(window);
            return result;
        }

        public static async void ShowLongException(string msg, Exception ex)
        {
            var textBox = new TextBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                TextWrapping = TextWrapping.Wrap,
                Text = $"{msg} - {ex.Message}\n\n{ex}",
                HorizontalAlignment = HorizontalAlignment.Center,
                IsReadOnly = true
            };
            await ShowDialogAsync(MainLang.GetException, p_content: textBox, b_primary: MainLang.Ok);
        }

        public static void SetAccentColor(Color color)
        {
            Application.Current.Resources["SystemAccentColor"] = color;
            Application.Current.Resources["ButtonDefaultPrimaryForeground"] = color;
            Application.Current.Resources["TextBoxFocusBorderBrush"] = color;
            Application.Current.Resources["ComboBoxSelectorPressedBorderBrush"] = color;
            Application.Current.Resources["ComboBoxSelectorFocusBorderBrush"] = color;
            Application.Current.Resources["TextBoxSelectionBackground"] = color;
            Application.Current.Resources["ProgressBarPrimaryForeground"] = color;
            Application.Current.Resources["ProgressBarIndicatorBrush"] = color;
            Application.Current.Resources["SliderThumbBorderBrush"] = color;
            Application.Current.Resources["SliderTrackForeground"] = color;
            Application.Current.Resources["HyperlinkButtonOverForeground"] = color;
            Application.Current.Resources["SliderThumbPressedBorderBrush"] = color;
            Application.Current.Resources["SliderThumbPointeroverBorderBrush"] = color;
            Application.Current.Resources["SystemAccentColorLight1"] = Value.ColorVariant(color, 0.15f);
            Application.Current.Resources["SystemAccentColorLight2"] = Value.ColorVariant(color, 0.30f);
            Application.Current.Resources["SystemAccentColorLight3"] = Value.ColorVariant(color, 0.45f);
            Application.Current.Resources["SystemAccentColorDark1"] = Value.ColorVariant(color, -0.15f);
            Application.Current.Resources["SystemAccentColorDark2"] = Value.ColorVariant(color, -0.30f);
            Application.Current.Resources["SystemAccentColorDark3"] = Value.ColorVariant(color, -0.45f);
        }

        public static void ToggleTheme(Theme theme)
        {
            if (theme == Theme.Light)
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            }
            else if (theme == Theme.Dark)
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else if (theme == Theme.System)
            {
                Application.Current.RequestedThemeVariant = ThemeVariant.Default;
            }
        }

        public static void UpdateTheme()
        {
            try
            {
                (FindControlByName(Const.Window.main, "PART_PaneRoot") as Panel).Background =
                    Application.Current.ActualThemeVariant == ThemeVariant.Dark
                        ? SolidColorBrush.Parse("#2c2c2c")
                        : SolidColorBrush.Parse("#FFE9F6FF");
                var visuals = new Queue<Visual>();
                visuals.Enqueue(Const.Window.main);

                while (visuals.Count > 0)
                {
                    var current = visuals.Dequeue();
                    var control = current as Control;

                    if (control != null)
                    {
                        foreach (var child in control.GetVisualChildren())
                        {
                            visuals.Enqueue((Visual)child);
                        }

                        var imageControl = control as Image;
                        if (imageControl != null && imageControl.Source is DrawingImage)
                        {
                            imageControl.InvalidateVisual();
                            Console.WriteLine(imageControl.GetVisualRoot());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void InitStyle()
        {
            Application.Current!.Resources.MergedDictionaries.Add(
                (AvaloniaXamlLoader.Load(
                        new Uri("avares://FluentAvalonia/Styling/ControlThemes/BasicControls/ToggleSwitchStyles.axaml"))
                    as
                    ResourceDictionary)!);
            Application.Current!.Resources.MergedDictionaries.Add(
                (AvaloniaXamlLoader.Load(new Uri(
                        "avares://FluentAvalonia/Styling/ControlThemes/BasicControls/HyperlinkButtonStyles.axaml")) as
                    ResourceDictionary)!);
            Application.Current!.Resources.MergedDictionaries.Add(
                (AvaloniaXamlLoader.Load(
                        new Uri("avares://FluentAvalonia/Styling/ControlThemes/BasicControls/ListBoxStyles.axaml")) as
                    ResourceDictionary)!);
            Application.Current!.Resources.MergedDictionaries.Add(
                (AvaloniaXamlLoader.Load(
                        new Uri("avares://FluentAvalonia/Styling/ControlThemes/BasicControls/ExpanderStyles.axaml")) as
                    ResourceDictionary)!);
        }

        public static void SetWindowBackGroundImg()
        {
            Const.Window.main.TransparencyLevelHint = new[] { WindowTransparencyLevel.Mica };
            Application.Current.Resources["Opacity"] = 1.0;
            Const.Window.main.BackGroundImg.Source = null;
            /*Const.Window.main.Root.Background = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#262626")
                : SolidColorBrush.Parse("#f6fafd");*/
            Const.Window.main.TitleBar.Root.Background = Application.Current.ActualThemeVariant == ThemeVariant.Dark
                ? SolidColorBrush.Parse("#2c2c2c")
                : SolidColorBrush.Parse("#FFE9F6FF");
            
            try
            {
                (FindControlByName(Const.Window.main, "PART_PaneRoot") as Panel).Opacity =
                    (double)Application.Current.Resources["Opacity"]!;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var setting = Const.Data.Setting;

            if (setting.CustomBackGround == CustomBackGroundWay.Default)
            {
                return;
            }

            if (setting.CustomBackGround == CustomBackGroundWay.Image &&
                !string.IsNullOrWhiteSpace(setting.WindowBackGroundImgData))
            {
                Application.Current.Resources["Opacity"] = 0.875;
                try
                {
                    var bitmap = Value.Base64ToBitmap(setting.WindowBackGroundImgData);
                    Const.Window.main.BackGroundImg.Source = bitmap;
                }
                catch
                {
                    Const.Data.Setting.WindowBackGroundImgData = null;
                    File.WriteAllText(Const.String.SettingDataPath,
                        JsonConvert.SerializeObject(Const.Data.Setting, Formatting.Indented));
                    Toast(MainLang.LoadBackGroudFromPicFailTip, type: NotificationType.Error);
                    Application.Current.Resources["Opacity"] = 1.0;
                    Const.Window.main.BackGroundImg.Source = null;
                }

                (Method.Ui.FindControlByName(Const.Window.main, "PART_PaneRoot") as Panel).Opacity =
                    (double)Application.Current.Resources["Opacity"]!;
                return;
            }

            Const.Window.main.Background = Brushes.Transparent;
            Application.Current.Resources["Opacity"] = 0.75;
            Const.Window.main.Root.Background = Brushes.Transparent;
            Const.Window.main.TitleBar.Root.Background =
                Application.Current.ActualThemeVariant == ThemeVariant.Dark
                    ? SolidColorBrush.Parse("#a8242424")
                    : SolidColorBrush.Parse("#a8e7f5ff");
            (Method.Ui.FindControlByName(Const.Window.main, "PART_PaneRoot") as Panel).Opacity =
                (double)Application.Current.Resources["Opacity"]!;

            if (setting.CustomBackGround == CustomBackGroundWay.AcrylicBlur)
            {
                Const.Window.main.TransparencyLevelHint = new[] { WindowTransparencyLevel.AcrylicBlur };
            }

            if (setting.CustomBackGround == CustomBackGroundWay.Transparent)
            {
                Const.Window.main.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            }
        }

        public static void CheckLauncher()
        {
            async void UnofficialToast()
            {
                while (true)
                {
                    await ShowDialogAsync("Error !", MainLang.UnofficialTip, b_primary: MainLang.Ok,
                        b_cancel: MainLang.Cancel);
                }
            }

            try
            {
                var name = Const.Window.main.settingPage.launcherSettingPage.FindControl<TextBlock>("Version").Text;
                var author = Const.Window.main.settingPage.launcherSettingPage.FindControl<Label>("AuthorLabel")
                    .Content;
                var title = Const.Window.main.FindControl<TextBlock>("TitleText").Text;
                Task.Run(async () =>
                {
                    string url = "https://player.daiyu.fun/a.json";
                    HttpClient client = new HttpClient();

                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var list = JsonConvert.DeserializeObject<List<string>>(responseBody);
                        if (list.Contains(name!) || list.Contains(author) || list.Contains(title!))
                        {
                            Dispatcher.UIThread.Invoke(
                                () => { UnofficialToast(); });
                        }
                    }
                    catch
                    {
                    }
                });
            }
            catch
            {
            }
        }

        public static void ShowShortException(string msg, Exception ex)
        {
            Toast($"{msg}\n{ex.Message}", Const.Notification.main, NotificationType.Error);
        }

        public static async Task<bool> UpgradeToAdministratorPrivilegesAsync(Window p_window = null)
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            var windwo = p_window == null ? Const.Window.main : p_window;
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var result = await ShowDialogAsync(MainLang.UpgradeToAdministratorPrivileges,
                    MainLang.UpgradeToAdministratorPrivilegesTip, b_primary: MainLang.Ok, b_secondary: MainLang.Cancel,
                    p_window: windwo);
                if (result == ContentDialogResult.Primary)
                {
                    var startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory,
                        FileName = Process.GetCurrentProcess().MainModule.FileName!,
                        Verb = "runas"
                    };
                    Process.Start(startInfo);
                    Environment.Exit(0);
                    return true;
                }

                return false;
            }

            return true;
        }

        public static void RestartApp()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName!
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        public static async Task<(bool, bool, string, string)> CheckUpdateAsync()
        {
            try
            {
                var resourceName = "YMCL.Main.Public.Texts.DateTime.txt";
                var _assembly = Assembly.GetExecutingAssembly();
                var stream = _assembly.GetManifestResourceStream(resourceName);
                var version = string.Empty;
                using (var reader = new StreamReader(stream!))
                {
                    var result = reader.ReadToEnd();
                    version = $"v{result.Trim()}";
                }

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
                var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl));
                var apiVersion = (string)githubApiJson[0]["name"]!;
                return (true, apiVersion != version, apiVersion, (string)githubApiJson[0]["html_url"]!);
            }
            catch
            {
                return (false, false, string.Empty, string.Empty);
            }
        }

        public static async Task<bool> UpdateAppAsync()
        {
            var task = new TaskManager.TaskEntry(MainLang.CheckUpdate);
            task.Show();
            try
            {
                var architecture = Value.GetCurrentPlatformAndArchitecture();
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
                var githubApiJson = JArray.Parse(await httpClient.GetStringAsync(Const.String.GithubUpdateApiUrl));
                task.UpdateTextProgress(MainLang.CheckUpdate);
                var assets = (JArray)githubApiJson[0]["assets"];
                var url = String.Empty;
                var fileName = string.Empty;
                foreach (var jToken in assets)
                {
                    var asset = (JObject)jToken;
                    var name = (string)asset["name"];
                    var browser_download_url = (string)asset["browser_download_url"];
                    switch (name)
                    {
                        case "YMCL.Main.linux.arm.AppImage" when architecture == "linux-arm":
                        case "YMCL.Main.linux.arm64.AppImage" when architecture == "linux-arm64":
                        case "YMCL.Main.linux.x64.AppImage" when architecture == "linux-x64":
                        case "YMCL.Main.osx.mac.x64.app.zip" when architecture == "osx-x64":
                        case "YMCL.Main.osx.mac.arm64.app.zip" when architecture == "osx-arm64":
                        case "YMCL.Main.win.x64.installer.exe"
                            when architecture == "win-x64" && Environment.OSVersion.Version.Major >= 10:
                        case "YMCL.Main.win.x86.installer.exe"
                            when architecture == "win-x86" && Environment.OSVersion.Version.Major >= 10:
                        case "YMCL.Main.win.arm64.installer.exe"
                            when architecture == "win-arm64" && Environment.OSVersion.Version.Major >= 10:
                        case "YMCL.Main.win7.x64.exe.zip"
                            when architecture == "win-x64" && Environment.OSVersion.Version.Major < 10:
                        case "YMCL.Main.win7.x86.exe.zip"
                            when architecture == "win-x86" && Environment.OSVersion.Version.Major < 10:
                        case "YMCL.Main.win7.arm64.exe.zip"
                            when architecture == "win-arm64" && Environment.OSVersion.Version.Major < 10:
                            url = browser_download_url;
                            fileName = name;
                            break;
                    }
                }

                if (url == null)
                {
                    task.Destory();
                    return false;
                }

                IO.ClearFolder(Const.String.UpdateFolderPath);

                var setting = Const.Data.Setting;
                var trueUrl = url;
                if (setting.EnableCustomUpdateUrl)
                {
                    trueUrl = setting.CustomUpdateUrl.Replace("{%url%}", url);
                }

                task.UpdateTextProgress($"{MainLang.GetUpdateUrl}: {trueUrl}");
                task.UpdateTextProgress(
                    $"{MainLang.BeginDownload}: {Path.Combine(Const.String.UpdateFolderPath, fileName)}");
                try
                {
                    var handler = new HttpClientHandler();
                    handler.ServerCertificateCustomValidationCallback =
                        (_, _, _, _) => true;
                    ServicePointManager.SecurityProtocol =
                        SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12;
                    using var client = new HttpClient(handler);
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36 Edg/121.0.0.0");
                    using (var response =
                           await client.GetAsync(trueUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        await using (var downloadStream = await response.Content.ReadAsStreamAsync())
                        {
                            await using (var fileStream = new FileStream(
                                             Path.Combine(Const.String.UpdateFolderPath, fileName), FileMode.Create,
                                             FileAccess.Write))
                            {
                                var buffer = new byte[8192];
                                int bytesRead;
                                long totalBytesRead = 0;
                                var totalBytes = response.Content.Headers.ContentLength.HasValue
                                    ? response.Content.Headers.ContentLength.Value
                                    : -1;

                                while ((bytesRead =
                                           await downloadStream.ReadAsync(buffer)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                                    totalBytesRead += bytesRead;

                                    if (totalBytes <= 0) continue;
                                    var progress = (double)totalBytesRead / totalBytes * 100;
                                    task.UpdateValueProgress(progress);
                                }
                            }
                        }
                    }

                    task.UpdateTextProgress($"{MainLang.DownloadFinish}");
                    if ((architecture == "win-x86" || architecture == "win-x64" || architecture == "win-arm64") &&
                        Environment.OSVersion.Version.Major >= 10)
                    {
                        var startInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            WorkingDirectory = Environment.CurrentDirectory,
                            FileName = Path.Combine(Const.String.UpdateFolderPath, fileName)
                        };
                        Process.Start(startInfo);
                        Environment.Exit(0);
                    }
                    else
                    {
                        var dialog = Ui.ShowDialogAsync(MainLang.DownloadFinish,
                            MainLang.CurrectSystemNoSupportAutoUpdateTip + "\n" +
                            Path.Combine(Const.String.UpdateFolderPath, fileName), b_primary: MainLang.OpenFolder,
                            b_cancel: MainLang.Cancel);
                        if (dialog.Result == ContentDialogResult.Primary)
                        {
                            var launcher = TopLevel.GetTopLevel(Const.Window.main).Launcher;
                            await launcher.LaunchDirectoryInfoAsync(
                                new DirectoryInfo(Const.String.TempFolderPath));
                            await Task.Delay(1000);
                            var clipboard = TopLevel.GetTopLevel(Const.Window.main)?.Clipboard;
                            await clipboard.SetTextAsync(Path.Combine(Const.String.UpdateFolderPath, fileName));
                            Toast(MainLang.AlreadyCopyToClipBoard +
                                  $" : {Path.Combine(Const.String.UpdateFolderPath, fileName)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowShortException(MainLang.UpdateFail, ex);
                }

                task.Destory();
                return false;
            }
            catch
            {
                task.Destory();
                return false;
            }
        }

        public static void AppStrangeEffect()
        {
            List<Action> methods =
            [
                //NeverGiveUp
                () =>
                {
                    var launcher = TopLevel.GetTopLevel(Const.Window.main).Launcher;
                    launcher.LaunchUriAsync(new Uri("https://www.bilibili.com/video/BV1GJ411x7h7/"));
                },
                //Transform180deg
                () =>
                {
                    var rotateTransform = new RotateTransform(180);
                    Const.Window.main.Root.RenderTransform = rotateTransform;
                },
                //WindowMove
                () =>
                {
                    double _velocityX = 20; // 水平速度
                    double _velocityY = 20; // 垂直速度

                    // 使用DispatcherTimer来周期性地更新窗口位置
                    DispatcherTimer timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(10) // 设置定时器的时间间隔
                    };
                    var _screenBounds = Const.Window.main.Screens.All.FirstOrDefault()?.WorkingArea ??
                                        new PixelRect(0, 0, 800, 600);
                    timer.Tick += Timer_Tick;
                    timer.Start();

                    void Timer_Tick(object sender, EventArgs e)
                    {
                        var newPosition = new PixelPoint((int)(Const.Window.main.Position.X + _velocityX),
                            (int)(Const.Window.main.Position.Y + _velocityY));

                        // 检查窗口是否即将超出屏幕边界
                        if (newPosition.X < _screenBounds.X || newPosition.X + Const.Window.main.Width >
                            _screenBounds.X + _screenBounds.Width)
                        {
                            _velocityX = -_velocityX; // 改变水平方向
                        }

                        if (newPosition.Y < _screenBounds.Y || newPosition.Y + Const.Window.main.Height >
                            _screenBounds.Y + _screenBounds.Height)
                        {
                            _velocityY = -_velocityY; // 改变垂直方向
                        }

                        // 设置新位置
                        Const.Window.main.Position = newPosition;
                    }
                },
                //KeepSpinning
                async () =>
                {
                    var deg = 0;
                    while (true)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            if (deg > 360)
                            {
                                deg = 0;
                            }

                            var rotateTransform = new RotateTransform(deg);
                            Const.Window.main.RenderTransform = rotateTransform;
                            deg += 2;
                        });
                        await Task.Delay(10);
                    }
                },
            ];

            Random random = new Random();
            //methods[^1]();
            methods[random.Next(methods.Count)]();
        }
    }
}