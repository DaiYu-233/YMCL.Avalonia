using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaDialogs.Views;
using DialogHostAvalonia;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls;

namespace YMCL.Main.Views.Main.Pages.More.Pages.GameUpdateLog;

public partial class GameUpdateLog : UserControl
{
    List<TextBlock> _translateList = new();
    string _nowJsonPath = "";

    public GameUpdateLog()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    public async Task LoadNews()
    {
        try
        {
            LoadErrorInfoBar.IsVisible = false;
            Loading.IsVisible = true;
            using var client = new HttpClient();
            var json = await client.GetStringAsync("https://launchercontent.mojang.com/v2/javaPatchNotes.json");
            var news = JsonConvert.DeserializeObject<MojangJavaNews.Root>(json);
            // var news = JsonConvert.DeserializeObject<MojangJavaNews.Root>(File.ReadAllText("E:\\Untitled-3.json"));
            foreach (MojangJavaNews.EntriesItem item in news.entries)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant, out var c);
                    var root = new Border()
                    {
                        Width = 200, Height = 235, Tag = item.contentPath,
                        Background = c as SolidColorBrush, Margin = new Thickness(0, 0, 10, 10),
                        CornerRadius = new CornerRadius(8), ClipToBounds = true,
                        Opacity = (double)Application.Current.Resources["Opacity"]!
                    };
                    root.PointerPressed += NewsEntryClick;
                    var panel = new StackPanel();
                    root.Child = panel;
                    panel.Children.Add(new Border()
                    {
                        CornerRadius = new CornerRadius(8), ClipToBounds = true, Width = 160, Height = 160,
                        Margin = new Thickness(20, 20, 20, 10),
                        Child = new UrlImage()
                        {
                            Url = "https://launchercontent.mojang.com" + item.image.url,
                            Width = 160, Height = 160
                        }
                    });
                    panel.Children.Add(AddToTranslate(new TextBlock()
                    {
                        Text = item.title, FontSize = 14, Margin = new Thickness(5, 10, 5, 5),
                        HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 170,
                        TextTrimming = TextTrimming.LeadingCharacterEllipsis
                    }));
                    Container.Children.Add(root);
                });
            }

            Loading.IsVisible = false;
        }
        catch (Exception ex)
        {
            LoadErrorInfoBar.IsVisible = true;
        }
    }

    private TextBlock AddToTranslate(TextBlock textBlock)
    {
        _translateList.Add(textBlock);
        return textBlock;
    }

    private async void StartTranslate()
    {
        async Task Translate(TextBlock textBlock)
        {
            // textBlock.Text = "aaa";
        }

        _translateList.ForEach(x => { _ = Translate(x); });
    }


    private async void NewsEntryClick(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            DetailLoading.IsVisible = true;
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (10, 0, 10, 10), TimeSpan.FromSeconds(0.30), Detail, true);
            Detail.IsVisible = true;
            Detail.Opacity = (double)Application.Current.Resources["Opacity"]!;
            Container.Opacity = 0;
            _translateList.Clear();
            DataPanel.Children.Clear();

            _nowJsonPath = ((Border)sender).Tag.ToString();
            var path = _nowJsonPath;
            using var client = new HttpClient();
            var json =
                Const.Data.NewsDataList.Find(a => a.Url == path)?.Data;
            if (json == null)
            {
                json = await client.GetStringAsync("https://launchercontent.mojang.com/v2/" + path);
                Const.Data.NewsDataList.Add(new NewsDataListEntry() { Url = path, Data = json });
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
                            { Text = node.InnerText,TextWrapping = TextWrapping.Wrap ,Margin = new Thickness(0, 5) }));
                        break;
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        // 解析H1-H6标签，并根据H的级别调整文字大小
                        var header = AddToTranslate(new TextBlock
                            { Text = node.InnerText, Margin = new Thickness(0, 5),TextWrapping = TextWrapping.Wrap });
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
                            var listParagraph = AddToTranslate(new TextBlock() { Margin = new Thickness(0, 5) ,TextWrapping = TextWrapping.Wrap});
                            if (node.Name == "ul")
                            {
                                // 列表前面添加圆点
                                listParagraph.Inlines.Add(new Run("• ") { FontSize = 12 });
                            }

                            listParagraph.Inlines.Add(new Run(listItem.InnerText));
                            dataContainer.Children.Add(listParagraph);
                        }

                        break;
                    case "a":
                        // 解析A标签
                        var hyperlink = new HyperlinkButton()
                        {
                            NavigateUri = new Uri(node.GetAttributeValue("href", "#")),
                            Content = AddToTranslate(new TextBlock
                            {
                                Text = node.InnerText,TextWrapping = TextWrapping.Wrap, TextDecorations = null,
                                Foreground =
                                    new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColor"]!)
                            })
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
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
        CloseButton.Click += async (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Container, true);
            Detail.Opacity = 0;
            await Task.Delay(TimeSpan.FromSeconds(0.30));
            Detail.IsVisible = false;
        };
    }

    private void ControlProperty()
    {
    }
}