using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System;

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
        Loaded += (_, _) =>
        {
            TitleText.Text = Title;
            if (!IsCloseBtnShow)
            {
                CloseButton.IsVisible = false;
                MaximizeButton.Margin = new Thickness(0, 0, -5, 0);
                MinimizeButton.Margin = new Thickness(0, 0, 21, 0);
            };
        };
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TitleBar, string>(nameof(Title), defaultValue: "Default Title");
    public static readonly StyledProperty<bool> IsCloseBtnExitAppProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsCloseBtnExitApp), defaultValue: false);
    public static readonly StyledProperty<bool> IsCloseBtnShowProperty =
        AvaloniaProperty.Register<TitleBar, bool>(nameof(IsCloseBtnShow), defaultValue: true);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public bool IsCloseBtnExitApp
    {
        get => GetValue(IsCloseBtnExitAppProperty);
        set => SetValue(IsCloseBtnExitAppProperty, value);
    }
    public bool IsCloseBtnShow
    {
        get => GetValue(IsCloseBtnShowProperty);
        set => SetValue(IsCloseBtnShowProperty, value);
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
            if (IsCloseBtnExitApp)
            {
                Environment.Exit(0);
            }
            else
            {
                var window = button.GetVisualRoot() as Window;
                if (window != null)
                {
                    window.Close();
                }
            }
        }
    }
}