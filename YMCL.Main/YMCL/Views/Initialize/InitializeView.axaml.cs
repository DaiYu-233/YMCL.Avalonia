using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Views.Initialize.Pages;

namespace YMCL.Views.Initialize;

public partial class InitializeView : UserControl
{
    private readonly ViewModels.InitViewModel ViewModel = new();
    private readonly Language _language = new();
    private readonly TitleBarStyle _titleBarStyle = new();

    public InitializeView(int page)
    {
        InitializeComponent();
        DataContext = ViewModel;
        UpdatePage(page);
    }

    public InitializeView()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public void UpdatePage(int index)
    {
        Frame.Content = index switch
        {
            1 => _language,
            2 => _titleBarStyle,
            _ => Frame.Content
        };
    }
}