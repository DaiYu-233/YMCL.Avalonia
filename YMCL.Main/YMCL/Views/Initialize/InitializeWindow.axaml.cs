using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace YMCL.Views.Initialize;

public partial class InitializeWindow : Window
{
    public InitializeWindow(int page)
    {
        InitializeComponent();
        InitializeView.UpdatePage(page);
    }
}