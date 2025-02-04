using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Public.Classes;
using YMCL.Public.Module;

namespace YMCL.Views.Main.Pages.MorePages;

public partial class GameUpdateLog : UserControl
{
    List<TextBlock> _translateList = [];
    List<TextBlock> _tbList = [];
    List<Border> _bList = [];
    string _nowJsonPath = "";

    public GameUpdateLog()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
        _ = LoadNews();
    }

    public async System.Threading.Tasks.Task LoadNews()
    {
        try
        {
            _tbList.Clear();
            _bList.Clear();
            LoadErrorInfoBar.IsVisible = false;
            Loading.IsVisible = true;
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://launchercontent.mojang.com/v2/javaPatchNotes.json");
            var news = JsonConvert.DeserializeObject<MojangJavaNews.Root>(json);
            // var news = JsonConvert.DeserializeObject<MojangJavaNews.Root>(File.ReadAllText("E:\\Untitled-3.json"));
            foreach (var item in news.entries)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant, out var c);
                    var root = new Border()
                    {
                        Width = 200, Height = 235, Tag = item.contentPath,
                        Background = c as SolidColorBrush, Margin = new Thickness(0, 0, 10, 10),
                        CornerRadius = new CornerRadius(8), ClipToBounds = true,
                        Opacity = (double)Application.Current.Resources["MainOpacity"]!
                    };
                    _bList.Add(root);
                    root.PointerPressed += NewsEntryClick;
                    var panel = new StackPanel();
                    root.Child = panel;
                    panel.Children.Add(new Border()
                    {
                        CornerRadius = new CornerRadius(8), ClipToBounds = true, Width = 160, Height = 160,
                        Margin = new Thickness(20, 20, 20, 10),
                        Child = new AsyncImageLoader.AdvancedImage(new Uri("https://launchercontent.mojang.com" +
                                                                           item.image.url))
                        {
                            Width = 160, Height = 160,
                            Source = "https://launchercontent.mojang.com" + item.image.url
                        }
                    });
                    var tb = new TextBlock()
                    {
                        Text = item.title, FontSize = 14, Margin = new Thickness(5, 10, 5, 5),
                        HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 170,
                        TextTrimming = TextTrimming.LeadingCharacterEllipsis
                    };
                    _tbList.Add(tb);
                    panel.Children.Add(tb);
                    Container.Children.Add(root);
                });
            }

            Loading.IsVisible = false;
        }
        catch
        {
            LoadErrorInfoBar.IsVisible = true;
            Loading.IsVisible = false;
        }
    }

    private TextBlock AddToTranslate(TextBlock textBlock)
    {
        _translateList.Add(textBlock);
        return textBlock;
    }

    private void StartTranslate()
    {
        if (Data.Setting.Language.Code == "en-US") return;

        async System.Threading.Tasks.Task Translate(TextBlock textBlock)
        {
            if (string.IsNullOrWhiteSpace(Data.TranslateToken)) return;
            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    (_, _, _, _) => true;
                using var client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("Authorization", Data.TranslateToken);
                var response =
                    await client.PostAsync(
                        $"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to={Data.Setting.Language.Code}&textType=plain",
                        new StringContent($"[{{\"Text\": \"{textBlock.Text}\"}}]", Encoding.UTF8, "application/json"));
                var responseContent = await response.Content.ReadAsStringAsync();
                string translatedText =
                    ((JObject)JArray.Parse(responseContent)[0]["translations"][0])["text"].ToString();
                if (!string.IsNullOrWhiteSpace(translatedText))
                {
                    textBlock.Text = translatedText;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        System.Threading.Tasks.Task.Run(() =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var task = new List<System.Threading.Tasks.Task>();
                _translateList.ForEach(x => { task.Add(Translate(x)); });
                System.Threading.Tasks.Task.WhenAll(task.ToArray());
            }, DispatcherPriority.ApplicationIdle);
        });
    }


    private async void NewsEntryClick(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            DetailLoading.IsVisible = true;
            _ = Public.Module.Ui.Animator.PageLoading.LevelTwoPage(Detail);
            Detail.IsVisible = true;
            Detail.Opacity = (double)Application.Current.Resources["MainOpacity"]!;
            Container.Opacity = 0;
            _translateList.Clear();
            DataPanel.Children.Clear();

            _nowJsonPath = ((Border)sender).Tag.ToString();
            var path = _nowJsonPath;
            using var client = new HttpClient();
            var json =
                Data.NewsDataList.Find(a => a.Url == path)?.Data;
            if (json == null)
            {
                json = await client.GetStringAsync("https://launchercontent.mojang.com/v2/" + path);
                Data.NewsDataList.Add(new NewsDataListEntry() { Url = path, Data = json });
            }

            if (path != _nowJsonPath) return;
            JObject obj = JObject.Parse(json);

            // 创建一个新的HtmlDocument对象
            var htmlDoc = new HtmlDocument();
            // 加载HTML内容
            htmlDoc.LoadHtml(obj["body"].ToString());
            // 获取根节点
            var rootNode = htmlDoc.DocumentNode;

            // 创建StackPanel
            var dataContainer = DataPanel!;

            // 遍历所有节点
            foreach (var node in rootNode.Descendants())
            {
                switch (node.Name)
                {
                    case "p":
                        // 解析P标签
                        dataContainer.Children.Add(AddToTranslate(new TextBlock
                            { Text = node.InnerText, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5) }));
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        // 解析H1-H6标签，并根据H的级别调整文字大小
                        var header = AddToTranslate(new TextBlock
                            { Text = node.InnerText, Margin = new Thickness(0, 5), TextWrapping = TextWrapping.Wrap });
                        switch (node.Name)
                        {
                            case "h1":
                                header.FontSize = 24;
                                break;
                            case "h2":
                                header.FontSize = 22;
                                break;
                            case "h3":
                                header.FontSize = 20;
                                break;
                            case "h4":
                                header.FontSize = 18;
                                break;
                            case "h5":
                                header.FontSize = 16;
                                break;
                            case "h6":
                                header.FontSize = 14;
                                break;
                        }

                        dataContainer.Children.Add(header);
                        break;
                    case "ul":
                    case "ol":
                        // 解析UL和OL标签
                        foreach (var listItem in node.Descendants("li"))
                        {
                            var listParagraph = AddToTranslate(new TextBlock()
                            {
                                Margin = new Thickness(0, 5), Text = "• " + listItem.InnerText,
                                TextWrapping = TextWrapping.Wrap
                            });
                            dataContainer.Children.Add(listParagraph);
                        }

                        break;
                    case "a":
                        // 解析A标签
                        var hyperlink = new HyperlinkButton()
                        {
                            NavigateUri = new Uri(node.GetAttributeValue("href", "#")),
                            Content = new TextBlock
                            {
                                Text = node.InnerText, TextWrapping = TextWrapping.Wrap, TextDecorations = null,
                                Foreground =
                                    new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]!)
                            }
                        };
                        dataContainer.Children.Add(hyperlink);
                        break;
                }
            }

            DetailLoading.IsVisible = false;
            StartTranslate();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    private void BindingEvent()
    {
        Application.Current.ActualThemeVariantChanged += (_, _) =>
        {
            Application.Current.TryGetResource("TextColor", Application.Current.ActualThemeVariant,
                out var c);
            Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant,
                out var c1);
            var color = (SolidColorBrush)c;
            var color1 = (SolidColorBrush)c1;
            _tbList.ForEach(a => { a.Foreground = color!; });
            _bList.ForEach(a => { a.Background = color1!; });
            _translateList.ForEach(a => { a.Foreground = color!; });
        };
        CloseButton.Click += async (_, _) =>
        {
            await Public.Module.Ui.Animator.PageLoading.ReversalLevelTwoPage(Detail);
            await Public.Module.Ui.Animator.PageLoading.LevelTwoPage(Container);
            Detail.IsVisible = false;
        };
        ReloadBtn.Click += (_, _) => { _ = LoadNews(); };
    }

    private void ControlProperty()
    {
    }
}