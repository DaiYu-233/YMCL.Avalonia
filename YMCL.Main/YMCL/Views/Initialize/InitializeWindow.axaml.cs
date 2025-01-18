using Avalonia.Interactivity;

namespace YMCL.Views.Initialize;

public partial class InitializeWindow : Window
{
    public InitializeWindow(int page)
    {
        InitializeComponent();
        InitializeView.UpdatePageAnimation(page);
        Public.Module.Ui.Setter.UpdateWindowStyle(this);
    }
}