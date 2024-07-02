using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace YMCL.Main.Public.Controls
{
    public partial class UrlImage : UserControl
    {
        bool _firstLoad = true;
        public UrlImage()
        {
            InitializeComponent();
            Loaded += UrlImage_Loaded;
        }

        private async void UrlImage_Loaded(object? sender, RoutedEventArgs e)
        {
            if (_firstLoad)
            {
                _firstLoad = false;
                var bitmap = await Method.LoadImageFromUrlAsync(Url);
                if (bitmap != null)
                {
                    Img.Source = bitmap;
                }
            }
        }

        public static readonly StyledProperty<string> UrlProperty =
            AvaloniaProperty.Register<TitleBar, string>(nameof(Url), defaultValue: "https://ymcl.daiyu.fun/Assets/img/YMCL-Icon.png");

        public string Url
        {
            get => GetValue(UrlProperty);
            set => SetValue(UrlProperty, value);
        }
    }
}
