using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using MinecraftLaunch.Base.Models.Game;
using Ursa.Controls;
using YMCL.Public.Classes.Data;
using YMCL.Public.Langs;
using YMCL.Public.Module.Value;
using YMCL.ViewModels;
using String = System.String;

namespace YMCL.Views.Main.Pages.LaunchPages.SubPages;

public partial class Setting : UserControl
{
    private readonly GameSettingModel _model;

    public Setting(GameSettingModel model)
    {
        _model = model;
        InitializeComponent();
        DataContext = _model;
        BindingEvent();
    }

    private void BindingEvent()
    {
        FastLaunchButton.Click += async (_, _) =>
        {
            var base64 = Converter.BitmapToBase64(_model.Icon);
            var name = new TextBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                Text = _model.MinecraftEntry.Id, HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            FormItem.SetLabel(name, MainLang.DisplayName);
            FormItem.SetIsRequired(name, true);
            var server = new TextBox
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                Text = string.Empty, HorizontalAlignment = HorizontalAlignment.Stretch, Watermark = "example.com:25565"
            };
            FormItem.SetLabel(server, MainLang.AutoJoinServer);
            var icon = new Button
            {
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
                Content = MainLang.SelectImgFile, HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            icon.Click += async (_, _) =>
            {
                var list = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
                    new FilePickerOpenOptions
                        { AllowMultiple = false, FileTypeFilter = [FilePickerFileTypes.ImageAll] });
                if (list.Count == 0) return;
                try
                {
                    var bitmap = new Bitmap(list[0].Path.LocalPath);
                    base64 = Converter.BitmapToBase64(bitmap);
                    Notice(MainLang.OperateSuccess);
                }
                catch
                {
                    Notice(MainLang.OperateFailed);
                }
            };
            FormItem.SetLabel(icon, MainLang.Icon);
            var form = new Form()
            {
                Items =
                {
                    name, server, icon
                },
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
            };

            var cr = await ShowDialogAsync(MainLang.FastLaunch, p_content: form, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (cr != ContentDialogResult.Primary) return;
            Data.FavouriteMinecraft.Add(new FavouriteMinecraftEntry
            {
                MinecraftPath = _model.MinecraftEntry.MinecraftFolderPath, MinecraftId = _model.MinecraftEntry.Id,
                DisplayName = name.Text ?? _model.MinecraftEntry.Id, ServerUrl = server.Text ?? string.Empty, IconBase64 = base64
            });
        };
    }

    public Setting()
    {
    }
}