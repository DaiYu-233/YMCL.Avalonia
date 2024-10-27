using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Newtonsoft.Json;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Controls;

namespace YMCL.Main.Views.Main.Pages.More.Pages.GameUpdateLog;

public partial class GameUpdateLog : UserControl
{
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
            await Task.Run(async () =>
            {
                // using var client = new HttpClient();
                // var json = await client.GetStringAsync("https://launchercontent.mojang.com/javaPatchNotes.json");
                var news = JsonConvert.DeserializeObject<MojangJavaNews.Root>(File.ReadAllText("E:\\Untitled-3.json"));
                foreach (MojangJavaNews.EntriesItem item in news.entries)
                {
                    Dispatcher.UIThread.InvokeAsync((() =>
                    {
                        Application.Current.TryGetResource("1x", Application.Current.ActualThemeVariant, out var c);
                        var root = new Border()
                        {
                            Background = c as SolidColorBrush, Margin = new Thickness(0,0,10,10),
                            CornerRadius = new CornerRadius(8), ClipToBounds = true
                        };
                        var panel = new StackPanel();
                        root.Child = panel;
                        panel.Children.Add(new Border()
                        {
                            CornerRadius = new CornerRadius(8), ClipToBounds = true, Width = 120, Height = 120,
                            Margin = new Thickness(10),
                            Child = new UrlImage()
                            {
                                Url = "https://launchercontent.mojang.com" + item.image.url,
                                Width = 120, Height = 120
                            }
                        });
                        panel.Children.Add(new TextBlock()
                            { Text = item.title, FontSize = 14, Margin = new Thickness(5, 10, 5, 5) });
                        Container.Children.Add(root);
                    }), DispatcherPriority.ApplicationIdle);
                }
            });
            Loading.IsVisible = false;
        }
        catch (Exception ex)
        {
            LoadErrorInfoBar.IsVisible = true;
        }
    }

    private void BindingEvent()
    {
        Loaded += (_, _) =>
        {
            Method.Ui.PageLoadAnimation((0, 50, 0, -50), (0, 0, 0, 0), TimeSpan.FromSeconds(0.30), Root, true);
        };
    }

    private void ControlProperty()
    {
    }
}