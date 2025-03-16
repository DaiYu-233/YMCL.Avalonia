using System.IO;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ursa.Controls;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Operate;
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

            TextBox world = null;
            if (!string.IsNullOrWhiteSpace(_model.MinecraftEntry.ClientJsonPath) &&
                File.Exists(_model.MinecraftEntry.ClientJsonPath))
            {
                try
                {
                    var obj = JObject.Parse(await File.ReadAllTextAsync(_model.MinecraftEntry.ClientJsonPath));
                    var time = DateTime.Parse(obj["releaseTime"].ToString());
                    if (time > new DateTime(2023, 4, 4))
                    {
                        world = new TextBox
                        {
                            FontFamily = (FontFamily)Application.Current.Resources["Font"], TextWrapping = TextWrapping.Wrap,
                            Text = string.Empty, HorizontalAlignment = HorizontalAlignment.Stretch, Watermark = MainLang.SavesName
                        };
                    }
                }
                catch
                {
                }
            }

            if (world != null)
            {
                server.TextChanged += (_, _) => { world.IsEnabled = string.IsNullOrWhiteSpace(server.Text); };
                world.TextChanged += (_, _) => { server.IsEnabled = string.IsNullOrWhiteSpace(world.Text); };
            }
            
            FormItem.SetLabel(icon, MainLang.Icon);
            var form = new Form()
            {
                Items =
                {
                    name, server
                },
                MinWidth = 300,
                FontFamily = (FontFamily)Application.Current.Resources["Font"],
            };
            if (world != null)
            {
                FormItem.SetLabel(world, MainLang.AutoJoinWorld);
                form.Items.Add(world);
            }
            form.Items.Add(icon);

            var cr = await ShowDialogAsync(MainLang.FastLaunch, p_content: form, b_cancel: MainLang.Cancel,
                b_primary: MainLang.Ok);
            if (cr != ContentDialogResult.Primary) return;
            Data.FavouriteMinecraft.Add(new FavouriteMinecraftEntry
            {
                MinecraftPath = _model.MinecraftEntry.MinecraftFolderPath, MinecraftId = _model.MinecraftEntry.Id,
                DisplayName = name.Text ?? _model.MinecraftEntry.Id, ServerUrl = server.Text ?? string.Empty,
                IconBase64 = base64, WorldName = world?.Text ?? string.Empty, IsSupportJoinWorld = world != null
            });
            await File.WriteAllTextAsync(ConfigPath.FavouriteMinecraftDataPath,
                JsonConvert.SerializeObject(Public.Const.Data.FavouriteMinecraft, Formatting.Indented));
        };
    }

    public Setting()
    {
    }
}