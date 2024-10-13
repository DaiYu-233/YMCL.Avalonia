using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using HtmlAgilityPack;

namespace YMCL.Main.Public.Controls;

public partial class JavaNewsEntry : UserControl
{
    public JavaNewsEntry(string img, string text)
    {
        InitializeComponent();
        ImageC.Source = "https://launchercontent.mojang.com" + img;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(text);

        var wrapPanel = WrapPanelC;

        // 解析并添加段落
        var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
        if (paragraphs != null)
        {
            foreach (var paragraph in paragraphs)
            {
                var textBlock = new TextBlock
                {
                    Text = paragraph.InnerText, FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
                    TextWrapping = TextWrapping.Wrap
                };
                wrapPanel.Children.Add(textBlock);
            }
        }

        // 解析并添加标题
        var h1 = htmlDoc.DocumentNode.SelectSingleNode("//h1");
        if (h1 != null)
        {
            var textBlock = new TextBlock
            {
                Text = h1.InnerText, FontWeight = FontWeight.Bold,
                FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
                TextWrapping = TextWrapping.Wrap
            };
            wrapPanel.Children.Add(textBlock);
        }

        // 解析并添加实验特性
        var h2 = htmlDoc.DocumentNode.SelectSingleNode("//h2");
        if (h2 != null)
        {
            var textBlock = new TextBlock
            {
                Text = h2.InnerText, FontWeight = FontWeight.Bold,
                FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
                TextWrapping = TextWrapping.Wrap
            };
            wrapPanel.Children.Add(textBlock);
        }

        // 解析并添加列表项
        var listItems = htmlDoc.DocumentNode.SelectNodes("//ul/li");
        if (listItems != null)
        {
            foreach (var item in listItems)
            {
                var textBlock = new TextBlock
                    { Text = item.InnerText, FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
                        TextWrapping = TextWrapping.Wrap };
                wrapPanel.Children.Add(textBlock);
                wrapPanel.Children.Add(new TextBlock { Text = "\n" }); // 添加换行
            }
        }

        // 解析并添加超链接
        var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        if (links != null)
        {
            foreach (var link in links)
            {
                var hyperlink = new HyperlinkButton()
                {
                    Content = link.InnerText, FontFamily = (FontFamily)Application.Current.Resources["Font"]!,
                    NavigateUri = new Uri(link.Attributes["href"].Value)
                };
                wrapPanel.Children.Add(hyperlink);
            }
        }
    }
}