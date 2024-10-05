using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using FluentAvalonia.UI.Controls;
using YMCL.Main.Public;
using YMCL.Main.Public.Langs;

namespace YMCL.Main.Views.Main.Pages.More.Pages.TreasureBox;

public partial class TreasureBox : UserControl
{
    public TreasureBox()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
        _ = LoadHitokoto();
        _ = LoadCodeLife();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
        NeverClickButton.Click += async (_, _) =>
        {
            await Method.Ui.ShowDialogAsync(MainLang.Tip, MainLang.NeverClickTip, b_primary: MainLang.Ok,
                b_cancel: MainLang.Ok,
                b_secondary: MainLang.Ok);
            Method.Ui.AppStrangeEffect();
        };
        TodayLuckyValueButton.Click += async (_, _) =>
        {
            var info = Method.IO.GetMacAddress() + DateTime.Now.ToString("yyyyMMdd");
            var seed = 0;
            foreach (var b in Encoding.UTF8.GetBytes(info))
            {
                seed += b;
            }

            Random random = new Random(seed);
            var lucky = random.Next(0, 101);
            Console.WriteLine(lucky);
            var tips = MainLang.LuckyValueTip.Split(@"/n");
            var tip = lucky switch
            {
                0 => tips[0],
                < 20 => tips[1],
                >= 20 and < 40 => tips[2],
                >= 40 and < 60 => tips[3],
                >= 60 and < 80 => tips[4],
                >= 80 and < 100 => tips[5],
                100 => tips[6],
                _ => String.Empty
            };
            await Method.Ui.ShowDialogAsync($"{MainLang.TodayLuckyValue} - {DateTime.Now:yyyy/MM/dd}",
                MainLang.TodayLuckyValueTip.Replace("{value}", lucky.ToString()) + "\n" + tip,
                b_primary: MainLang.IKnow);
        };
        RefreshHitokoto.PointerPressed += async (_, _) => await LoadHitokoto();
        RefreshCodeLife.PointerPressed += async (_, _) => await LoadCodeLife();
        ActivateBtn.Click += async (sender, e) =>
        {
            if (Const.Data.Platform != Platform.Windows)
                Method.Ui.Toast(MainLang.ThisFeatureOnlySupportsWindowsPlatform, type: NotificationType.Error);
            else
                try
                {
                    if (Environment.OSVersion.Version.Major < 10)
                        Method.Ui.Toast(MainLang.ThisFeatureOnlySupportsWindows10AndAbove,
                            type: NotificationType.Warning);
                    if (await Method.Ui.UpgradeToAdministratorPrivilegesAsync())
                    {
                        var process = new Process();
                        process.StartInfo.FileName = @"powershell.exe";
                        process.StartInfo.Arguments = "irm https://get.activated.win | iex";
                        process.StartInfo.Verb = "runas";
                        process.Start();
                    }
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowLongException(MainLang.RunCommandFail, ex);
                }
        };
        CancelActivateWinBtn.Click += async (sender, e) =>
        {
            if (Const.Data.Platform != Platform.Windows)
                Method.Ui.Toast(MainLang.ThisFeatureOnlySupportsWindowsPlatform, type: NotificationType.Error);
            else
                try
                {
                    var tip = await Method.Ui.ShowDialogAsync(MainLang.CancelActivateWin, MainLang.CancelActivateWinTip,
                        b_primary: MainLang.Ok, b_secondary: MainLang.Cancel);
                    if (tip == ContentDialogResult.Primary && await Method.Ui.UpgradeToAdministratorPrivilegesAsync())
                    {
                        var str = "slmgr -upk";
                        Process p = new();
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.UseShellExecute = false; //是否使用操作系统shell启动
                        p.StartInfo.Verb = "runas";
                        p.StartInfo.RedirectStandardInput = true; //接受来自调用程序的输入信息
                        p.StartInfo.RedirectStandardOutput = true; //由调用程序获取输出信息
                        p.StartInfo.RedirectStandardError = true; //重定向标准错误输出
                        p.StartInfo.CreateNoWindow = true; //不显示程序窗口
                        p.Start(); //启动程序
                        p.StandardInput.WriteLine(str + "&exit");
                        p.StandardInput.AutoFlush = true;
                        var output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit();
                        p.Close();
                    }
                }
                catch (Exception ex)
                {
                    Method.Ui.ShowLongException(MainLang.RunCommandFail, ex);
                }
        };
    }

    private void ControlProperty()
    {
    }

    private async Task LoadHitokoto()
    {
        try
        {
            Hitokoto.Text = MainLang.Loading;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var hitokoto = await client.GetStringAsync("https://v1.hitokoto.cn/");
            var obj = JsonNode.Parse(hitokoto);
            Hitokoto.Text = $"{obj["hitokoto"]}  \u2014\u2014  {obj["from_who"] ?? obj["from"]}";
        }
        catch
        {
            Hitokoto.Text = MainLang.LoadFail;
        }
    }

    private async Task LoadCodeLife()
    {
        try
        {
            CodeLife.Text = MainLang.Loading;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var codelife = await client.GetStringAsync("https://api.codelife.cc/yiyan/random?lang=cn");
            var obj = JsonNode.Parse(codelife);
            CodeLife.Text = $"{obj["data"]["hitokoto"]}  \u2014\u2014  {obj["data"]["from"].ToString().Trim('-')}";
        }
        catch
        {
            CodeLife.Text = MainLang.LoadFail;
        }
    }
}