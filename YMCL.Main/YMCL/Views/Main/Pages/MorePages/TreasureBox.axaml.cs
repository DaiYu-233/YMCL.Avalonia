using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using YMCL.Public.Langs;
using YMCL.Public.Module;

namespace YMCL.Views.Main.Pages.MorePages;

public partial class TreasureBox : UserControl
{
    public TreasureBox()
    {
        InitializeComponent();
        BindingEvent(); 
        _ = LoadHitokoto();
        _ = LoadCodeLife();
    }

    private void BindingEvent()
    {
        NeverClickButton.Click += async (_, _) =>
        {
            await ShowDialogAsync(MainLang.Tip, MainLang.NeverClickTip, b_primary: MainLang.Ok,
                b_cancel: MainLang.Ok,
                b_secondary: MainLang.Ok);
            Public.Module.Ui.Setter.AppStrangeEffect();
        };
        TodayLuckyValueButton.Click += async (_, _) =>
        {
            var info = Public.Module.IO.Disk.Getter.GetMacAddress() + DateTime.Now.ToString("yyyyMMdd");
            var seed = Encoding.UTF8.GetBytes(info).Aggregate(0, (current, b) => current + b);

            var random = new Random(seed);
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
                _ => string.Empty
            };
            await ShowDialogAsync($"{MainLang.TodayLuckyValue} - {DateTime.Now:yyyy/MM/dd}",
                MainLang.TodayLuckyValueTip.Replace("{value}", lucky.ToString()) + "\n" + tip,
                b_primary: MainLang.IKnow);
        };
        RefreshHitokoto.PointerPressed += async (_, _) => await LoadHitokoto();
        RefreshCodeLife.PointerPressed += async (_, _) => await LoadCodeLife();
    }

    private async System.Threading.Tasks.Task LoadHitokoto()
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

    private async System.Threading.Tasks.Task LoadCodeLife()
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