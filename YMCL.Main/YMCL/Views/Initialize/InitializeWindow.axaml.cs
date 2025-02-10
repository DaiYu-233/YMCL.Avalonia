using Avalonia.Interactivity;
using Ursa.Controls;

namespace YMCL.Views.Initialize;

public partial class InitializeWindow : UrsaWindow
{
    public InitializeWindow(int page)
    {
        InitializeComponent();
        InitializeView.UpdatePageAnimation(page);
        Public.Module.Ui.Setter.UpdateWindowStyle(this);
        Loaded += (_, _) =>
        {
            Public.Module.Ui.Setter.UpdateWindowStyle(this);
        };
    }
}