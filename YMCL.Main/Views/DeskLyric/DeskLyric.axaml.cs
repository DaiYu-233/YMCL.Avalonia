using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Newtonsoft.Json;
using YMCL.Main.Public;

namespace YMCL.Main.Views.DeskLyric;

public partial class DeskLyric : Window
{
    public bool _isOpen = false;

    public DeskLyric()
    {
        InitializeComponent();
        Hide();
        LyricText.PointerPressed += (s, e) =>
        {
            if (e.Pointer.Type == PointerType.Mouse)
            {
                BeginMoveDrag(e);
                e.Handled = true;
            }
        };
    }

    public async void Toggle()
    {
        LyricText.FontFamily = (FontFamily)Application.Current.Resources["Font"]!;
        if (_isOpen)
        {
            Hide();
            _isOpen = false;
        }
        else
        {
            var setting = Const.Data.Setting;
            LyricText.Foreground = new SolidColorBrush(setting.DeskLyricColor);
            LyricText.TextAlignment = setting.DeskLyricAlignment;
            _isOpen = true;
            LyricText.Transitions = null;
            Const.Window.deskLyric.Height = 92;
            LyricText.FontSize = 72;
            var tr = new Transitions
            {
                new DoubleTransition()
                {
                    Property = FontSizeProperty,
                    Duration = TimeSpan.FromMilliseconds(300)
                }
            };
            Show();
            LyricText.Transitions = tr;
            LyricText.FontSize = setting.DeskLyricSize;
            WindowState = WindowState.Normal;
            Activate();
            Topmost = false;
            Topmost = true;
            await Task.Delay(300);
            Const.Window.deskLyric.Height = setting.DeskLyricSize + 20;
        }
    }
}