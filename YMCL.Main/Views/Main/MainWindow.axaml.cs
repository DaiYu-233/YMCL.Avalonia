using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Linq;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;

namespace YMCL.Main.Views.Main;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        EventBinding();
    }
    private void EventBinding()
    {
        Loaded += (s, e) =>
        {
            if (Const.Platform != Platform.Windows)
            {
                TitleBar.IsVisible = false;
                TitleText.IsVisible = false;
                Root.CornerRadius = new CornerRadius(0, 0, 8, 8);
            }
            else
            {
                WindowState = WindowState.Maximized;
                WindowState = WindowState.Normal;
            }
        };
        TitleText.PointerPressed += (s, e) =>
        {
            BeginMoveDrag(e);
        };
        PropertyChanged += (s, e) =>
        {
            if (Const.Platform == Platform.Windows && e.Property.Name == nameof(WindowState))
            {
                switch (WindowState)
                {
                    case WindowState.Normal:
                        Root.Margin = new Thickness(0);
                        break;
                    case WindowState.Maximized:
                        Root.Margin = new Thickness(20);
                        break;
                }
            }
        };
    }
}