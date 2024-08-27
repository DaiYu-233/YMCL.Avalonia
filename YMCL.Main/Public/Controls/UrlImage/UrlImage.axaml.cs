using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using YMCL.Main.Public.Classes;

namespace YMCL.Main.Public.Controls;

public partial class UrlImage : UserControl
{
    public static readonly StyledProperty<string> UrlProperty =
        AvaloniaProperty.Register<TitleBar, string>(nameof(Url),
            "null");

    public UrlImage()
    {
        InitializeComponent();
        _ = LoadImgAsync();
    }

    public string Url
    {
        get => GetValue(UrlProperty);
        set => SetValue(UrlProperty, value);
    }

    public async Task LoadImgAsync()
    {
        while (Url == "null") await Task.Delay(100);

        var entry =
            Const.Data.UrlImageDataList.Find(UrlImageDataListEntry => UrlImageDataListEntry.Url == Url);
        if (entry == null)
        {
            var bitmap = await Method.Value.LoadImageFromUrlAsync(Url);
            if (bitmap != null) Img.Source = bitmap;

            Const.Data.UrlImageDataList.Add(new UrlImageDataListEntry { Url = Url, Bitmap = bitmap });
        }
        else
        {
            Img.Source = entry.Bitmap;
        }
    }
}