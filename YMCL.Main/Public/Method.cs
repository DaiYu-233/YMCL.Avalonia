using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using FileInfo = YMCL.Main.Public.Classes.FileInfo;
using Path = System.IO.Path;

namespace YMCL.Main.Public
{
    public class Method
    {
        public static async void PageLoadAnimation((double, double, double, double) original, (double, double, double, double) target, TimeSpan time, Control control, bool visibility = false)
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
                    Property = Avalonia.Layout.Layoutable.MarginProperty
                });
                control.Transitions.Add(new DoubleTransition
                {
                    Duration = time,
                    Easing = new SineEaseInOut(),
                    Property = Visual.OpacityProperty
                });
                if (visibility)
                {
                    control.IsVisible = true;
                }
                control.Margin = new Thickness(tl, tt, tr, tb);
                control.Opacity = 1;
                await Task.Delay(time);
                control.Transitions = transitions;
            }
        }
        public static void Toast(string msg, WindowNotificationManager notification, NotificationType type = NotificationType.Information, bool time = true, string title = "Yu Minecraft Launcher")
        {
            var showTitle = Const.AppTitle;
            if (!string.IsNullOrEmpty(title))
            {
                showTitle = title;
            }
            if (time)
            {
                showTitle += $" - {DateTime.Now.ToString("HH:mm:ss")}";
            }
            notification.Show(new Notification(showTitle, msg, type));
        }
        public static async Task<List<FolderInfo>> OpenFolderPicker(TopLevel topLevel = null, FolderPickerOpenOptions options = null)
        {
            var isPrimaryButtonClick = false;
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
            {
                if (options != null && topLevel != null)
                {
                    var storageProvider = topLevel!.StorageProvider;
                    var result = await storageProvider.OpenFolderPickerAsync(options);
                    var list = new List<FolderInfo>();
                    result.ToList().ForEach(item =>
                    {
                        list.Add(new FolderInfo() { Name = item.Name, Path = item.Path.LocalPath });
                    });
                    return list;
                }
                else
                {
                    new Exception("ParameterIsNull");
                    return null;
                }
            }
            else
            {
                var textBox = new TextBox() { FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap };
                ContentDialog dialog = new()
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = MainLang.InputFolderPath,
                    PrimaryButtonText = MainLang.Ok,
                    CloseButtonText = MainLang.Cancel,
                    DefaultButton = ContentDialogButton.Primary,
                    Content = textBox
                };
                dialog.PrimaryButtonClick += (_, _) =>
                {
                    isPrimaryButtonClick = true;
                };
                var result = await dialog.ShowAsync();
                var path = textBox.Text;
                if (!Directory.Exists(path) && isPrimaryButtonClick)
                {
                    Toast(MainLang.FolderNotExist, Const.Notification.main, NotificationType.Error);
                    return null;
                }
                var folder = Path.GetFileName(path);
                var list = new List<FolderInfo>() { new FolderInfo() { Name = folder, Path = path } };
                return list;
            }
        }
        public static async Task<List<FileInfo>> OpenFilePicker(TopLevel topLevel = null, FilePickerOpenOptions options = null)
        {
            var isPrimaryButtonClick = false;
            var setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(Const.SettingDataPath));
            if (setting.OpenFileWay == OpenFileWay.FileSelectWindow)
            {
                if (options != null && topLevel != null)
                {
                    var storageProvider = topLevel!.StorageProvider;
                    var result = await storageProvider.OpenFilePickerAsync(options);
                    var list = new List<FileInfo>();
                    result.ToList().ForEach(item =>
                    {
                        var path = item.Path.LocalPath;
                        var fullName = Path.GetFileName(path);
                        var name = Path.GetFileNameWithoutExtension(fullName);
                        var extension = Path.GetExtension(fullName);
                        list.Add(new FileInfo() { Name = name, Path = path, FullName = fullName, Extension = extension });
                    });
                    return list;
                }
                else
                {
                    new Exception("ParameterIsNull");
                    return null;
                }
            }
            else
            {
                var textBox = new TextBox() { FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap };
                ContentDialog dialog = new()
                {
                    FontFamily = (FontFamily)Application.Current.Resources["Font"],
                    Title = MainLang.InputFilePath,
                    PrimaryButtonText = MainLang.Ok,
                    CloseButtonText = MainLang.Cancel,
                    DefaultButton = ContentDialogButton.Primary,
                    Content = textBox
                };
                dialog.PrimaryButtonClick += (_, _) =>
                {
                    isPrimaryButtonClick = true;
                };
                var result = await dialog.ShowAsync();
                var path = textBox.Text;
                if (!File.Exists(path) && isPrimaryButtonClick)
                {
                    Toast(MainLang.FileNotExist, Const.Notification.main, NotificationType.Error);
                    return null;
                }
                var fullName = Path.GetFileName(path);
                var name = Path.GetFileNameWithoutExtension(fullName);
                var extension = Path.GetExtension(fullName);
                var list = new List<FileInfo>() { new FileInfo() { Name = name, Path = path, FullName = fullName, Extension = extension } };
                return list;
            }
        }
        public static void SetAccentColor(Color color)
        {
            Application.Current.Resources["SystemAccentColor"] = color;
            Application.Current.Resources["SystemAccentColorLight1"] = ColorVariant(color, 0.15f);
            Application.Current.Resources["SystemAccentColorLight2"] = ColorVariant(color, 0.30f);
            Application.Current.Resources["SystemAccentColorLight3"] = ColorVariant(color, 0.45f);
            Application.Current.Resources["SystemAccentColorDark1"] = ColorVariant(color, -0.15f);
            Application.Current.Resources["SystemAccentColorDark2"] = ColorVariant(color, -0.30f);
            Application.Current.Resources["SystemAccentColorDark3"] = ColorVariant(color, -0.45f);
        }
        public static Color ColorVariant(Color color, float percent)
        {
            // 确保百分比在-1到1之间  
            percent = Math.Max(-1f, Math.Min(1f, percent));

            // 计算调整后的RGB值  
            float adjust = 1f + percent; // 亮化是1+percent，暗化是1+(negative percent)，即小于1  
            int r = (int)Math.Round(color.R * adjust);
            int g = (int)Math.Round(color.G * adjust);
            int b = (int)Math.Round(color.B * adjust);

            // 确保RGB值在有效范围内  
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            // 创建一个新的颜色（保持Alpha通道不变）  
            return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
        }
        public static void ToggleTheme(Theme theme)
        {
            if (theme == Theme.Light)
            {
                var rd = (AvaloniaXamlLoader.Load(new Uri("avares://YMCL.Main/Public/Styles/LightTheme.axaml")) as ResourceDictionary)!;
                Application.Current!.Resources.MergedDictionaries.Add(rd);
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
            }
            else if (theme == Theme.Dark)
            {
                var rd = (AvaloniaXamlLoader.Load(new Uri("avares://YMCL.Main/Public/Styles/DarkTheme.axaml")) as ResourceDictionary)!;
                Application.Current!.Resources.MergedDictionaries.Add(rd);
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
            }
        }
        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                directoryInfo.Create();
            }
        }
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();//精确到秒
        }
        public static Bitmap Base64ToBitmap(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(imageBytes))
            {
                var bitmap = new Bitmap(ms);
                return bitmap;
            }
        }
        public static string BytesToBase64(byte[] imageBytes)
        {
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }
        public static object RunCodeByString(string code, object[] args = null, string[] dlls = null)
        {
            //Nuget Microsoft.CodeAnalysis.CSharp
            //Type type = null;
            //SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            //CSharpCompilation cSharpCompilation = CSharpCompilation.Create("CustomAssembly")
            //    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            //    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            //    .AddSyntaxTrees(syntaxTree);
            //if (dlls != null)
            //{
            //    foreach (string dll in dlls)
            //    {
            //        if (!string.IsNullOrEmpty(dll))
            //        {
            //            cSharpCompilation.AddReferences(MetadataReference.CreateFromFile(dll));
            //        }
            //    }
            //}
            //MemoryStream memoryStream = new MemoryStream();
            //EmitResult emitResult = cSharpCompilation.Emit(memoryStream);
            //if (emitResult.Success)
            //{
            //    memoryStream.Seek(0, SeekOrigin.Begin);
            //    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(memoryStream);
            //    type = assembly.GetType("YMCLRunner");
            //}
            //else
            //{
            //    var str = string.Empty;
            //    foreach (var item in emitResult.Diagnostics)
            //    {
            //        str += $"----> {item}\n";
            //    }
            //    MessageBoxX.Show($"\n{LangHelper.Current.GetText("ComPileCSharpError")}\n\n{str}", "Yu Minecraft Launcher");
            //    type = null;
            //}
            //if (type != null)
            //{
            //    object? obj = Activator.CreateInstance(type);
            //    MethodInfo? methodInfo = type.GetMethod("Main");
            //    object? result = methodInfo.Invoke(obj, args);
            //    //MessageBoxX.Show($"Result: {result}");
            //    return result;
            //}
            return null;
        }
        public static string MsToTime(double ms)//转换为分秒格式
        {
            int minute = 0;
            int second = 0;
            second = (int)(ms / 1000);

            string secondStr = string.Empty;
            string minuteStr = string.Empty;

            if (second > 60)
            {
                minute = second / 60;
                second = second % 60;
            }

            secondStr = second < 10 ? $"0{second}" : $"{second}";
            minuteStr = minute < 10 ? $"0{minute}" : $"{minute}";

            return $"{minuteStr}:{secondStr}";
        }
        public static string FormatNumberWithWanYi(string numberStr)
        {
            // 先转换为decimal，确保精度  
            decimal number = decimal.Parse(numberStr, CultureInfo.InvariantCulture);

            if (number < 10000) // 小于万位，直接返回  
            {
                return number.ToString("N0", CultureInfo.InvariantCulture);
            }
            else if (number < 100000000) // 小于亿位，转换为万位  
            {
                return (number / 10000).ToString("N2") + "万";
            }
            else // 大于等于亿位，转换为亿位  
            {
                return (number / 100000000).ToString("N2") + "亿";
            }
        }
        public static void ShowShortException(string msg, Exception ex)
        {
            Toast($"{msg}\n{ex.Message}", Const.Notification.main, NotificationType.Error);
        }
        public static void RestartApp()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName!
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
        public static double GetTotalMemory(Platform platform)
        {
            if (platform == Platform.Windows)
            {
                try
                {
                    long totalMemory = 0;
                    using (var searcher = new ManagementObjectSearcher("select TotalVisibleMemorySize from Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject share in searcher.Get())
                        {
                            totalMemory = Convert.ToInt64(share["TotalVisibleMemorySize"]);
                        }
                    }

                    Console.WriteLine("系统最大内存: " + totalMemory);

                    return totalMemory;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取系统内存信息时发生错误: " + ex.Message);
                    return 0;
                }
            }
            else if (platform == Platform.Linux)
            {
                // 尝试读取 /proc/meminfo 文件  
                try
                {
                    string meminfo = File.ReadAllText("/proc/meminfo");

                    // 使用 LINQ 查询来找到 "MemTotal" 行  
                    var memTotalLine = meminfo.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                        .FirstOrDefault(line => line.StartsWith("MemTotal:"));

                    // 如果找到 MemTotal 行，解析其值  
                    if (memTotalLine != null)
                    {
                        // 提取 MemTotal 后面的数字，并转换为长整型  
                        string memTotalValueStr = memTotalLine.Split(':')[1].Trim().Split(' ')[0];
                        long memTotalValue = long.Parse(memTotalValueStr);

                        return memTotalValue;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading /proc/meminfo: " + ex.Message);
                    return 0;
                }
            }
            return 0;
        }
        public static double GetDirectoryLength(string dirPath)
        {
            //判断给定的路径是否存在,如果不存在则退出
            if (!Directory.Exists(dirPath))
                return 0;
            double len = 0;

            //定义一个DirectoryInfo对象
            DirectoryInfo di = new DirectoryInfo(dirPath);

            //通过GetFiles方法,获取di目录中的所有文件的大小
            foreach (var fi in di.GetFiles())
            {
                len += fi.Length;
            }

            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }
        public static async Task<ContentDialogResult> ShowDialogAsync(string title = "Title", string msg = "Content", Control p_content = null, string b_primary = null, string b_cancel = null, string b_secondary = null)
        {
            var content = p_content == null ? new TextBlock() { FontFamily = (FontFamily)Application.Current.Resources["Font"], Text = msg } : p_content;
            var dialog = new ContentDialog()
            {
                PrimaryButtonText = b_primary,
                Content = content,
                DefaultButton = ContentDialogButton.Primary,
                CloseButtonText = b_cancel,
                SecondaryButtonText = b_secondary,
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Title = title
            };
            var result = await dialog.ShowAsync();
            return result;
        }
    }
}
