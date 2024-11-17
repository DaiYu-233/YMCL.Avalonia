using System;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using Ursa.Controls;
using YMCL.Main.Public;
using YMCL.Main.Public.Controls.TaskManage;
using YMCL.Main.Public.Langs;
using YMCL.Main.Views.Main.Pages.Download.Pages.AutoInstall;
using YMCL.Main.Views.Main.Pages.Download.Pages.CurseForgeFetcher;
using YMCL.Main.Views.Main.Pages.Download.Pages.Java;
using YMCL.Main.Views.Main.Pages.Download.Pages.Modrinch;

namespace YMCL.Main.Views.Main.Pages.Download;

public partial class DownloadPage : UserControl
{
    public AutoInstallPage autoInstallPage = new();
    public CurseForgeFetcher curseForgeFetcherPage = new();
    public ModrinchFetcher modrinchFetcherPage = new();
    public JavaDownloader javaDownloaderPage = new();

    public DownloadPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void ControlProperty()
    {
        FrameView.Content = autoInstallPage;
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
        };
        Nav.SelectionChanged += async (s, e) =>
        {
            switch (((NavigationViewItem)((NavigationView)s!).SelectedItem!).Tag)
            {
                case "auto":
                    autoInstallPage.Root.IsVisible = false;
                    FrameView.Content = autoInstallPage;
                    break;
                case "curseforgefetcher":
                    curseForgeFetcherPage.Root.IsVisible = false;
                    FrameView.Content = curseForgeFetcherPage;
                    break;
                case "javadownloader":
                    javaDownloaderPage.Root.IsVisible = false;
                    FrameView.Content = javaDownloaderPage;
                    break;
                case "modrinchfetcher":
                    /*curseForgeFetcherPage.Root.IsVisible = false;
                    FrameView.Content = curseForgeFetcherPage;*/
                    break;
                case "customdownload":
                    Nav.SelectedItem = NavCf;
                    Nav.SelectedItem = NavAuto;
                    _ = Const.Window.main.FocusButton();
                    var root = new StackPanel() { Spacing = 10 };
                    var url = new TextBox
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"], 
                        TextWrapping = TextWrapping.Wrap, Watermark = MainLang.DownloadUrl
                    };
                    var btn = new Button
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        Margin = new Thickness(0, 0, 10, 0),
                        Content = MainLang.Locate
                    };
                    var path = new TextBox
                    {
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        TextWrapping = TextWrapping.Wrap, Watermark = MainLang.InputFolderPath
                    };
                    var pathRoot = new DockPanel()
                    {
                        Children = { btn, path, }
                    };
                    root.Children.Add(url);
                    root.Children.Add(pathRoot);
                    var dialog = new ContentDialog()
                    {
                        Title = MainLang.CustomDownload,
                        PrimaryButtonText = MainLang.BeginDownload,
                        Content = root,
                        DefaultButton = ContentDialogButton.Primary,
                        CloseButtonText = MainLang.Cancel,
                        FontFamily = (FontFamily)Application.Current.Resources["Font"],
                        IsPrimaryButtonEnabled = false
                    };
                    btn.Click += async (_, _) =>
                    {
                        var result = (await Method.IO.OpenFolderPicker(TopLevel.GetTopLevel(this)!,
                            new FolderPickerOpenOptions() { Title = MainLang.OpenFolder }));
                        if (result.Count > 0)
                        {
                            path.Text = result[0].Path;
                        }
                    };
                    path.TextChanged += (_, _) =>
                    {
                        dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(path.Text) &&
                                                        !string.IsNullOrWhiteSpace(url.Text);
                    };
                    url.TextChanged += (_, _) =>
                    {
                        dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(url.Text) &&
                                                        !string.IsNullOrWhiteSpace(path.Text);
                    };
                    var dr = await dialog.ShowAsync(Const.Window.main);
                    if (dr == ContentDialogResult.Primary)
                    {
                        StringBuilder result = new StringBuilder(6);
                        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                        var random = new Random();
                        for (int i = 0; i < 6; i++)
                        {
                            var index = random.Next(chars.Length);
                            result.Append(chars[index]);
                        }

                        var task = new TaskManager.TaskEntry($"{MainLang.Download} : {result}",
                            textProgress: false);
                        try
                        {
                            await Method.IO.DownloadFileWithoutFileNameAsync(url.Text!,
                                new DirectoryInfo(path.Text!),
                                task);
                            task.Destory();
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                            Const.Notification.main.Show($"{MainLang.DownloadFail} : {url.Text}",
                                type: NotificationType.Error);
                            task.Destory();
                        }
                    }

                    break;
            }

            _ = Const.Window.main.FocusButton();
        };
    }
}