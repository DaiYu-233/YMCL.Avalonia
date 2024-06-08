using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace YMCL.Main.Public.Controls;

public partial class TitleBar : UserControl
{
    public TitleBar()
    {
        InitializeComponent();
        CloseButton.Click += CloseButton_Click;
        MaximizeButton.Click += MaximizeButton_Click;
        MinimizeButton.Click += MinimizeButton_Click;
        MoveDragArea.PointerPressed += MoveDragArea_PointerPressed;
        Loaded += (_, _) => { TitleText.Text = Title; };
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TitleBar, string>(nameof(Title), defaultValue: "Default Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private void MoveDragArea_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            var control = sender as Grid;
            if (control != null)
            {
                var window = control.GetVisualRoot() as Window;
                window.BeginMoveDrag(e);
            }
        }
    }

    private void MinimizeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var window = button.GetVisualRoot() as Window;
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }

    private void MaximizeButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var window = button.GetVisualRoot() as Window;
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
        }
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button != null)
        {
            var window = button.GetVisualRoot() as Window;
            if (window != null)
            {
                window.Close();
            }
        }
    }
}