using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace YMCL.Plugin.Dependence;

public partial class ExamplePage : UserControl
{
    private int _num = 0;
    public ExamplePage()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        _num++;
        TextBox.Text = $"{_num}";
    }
}