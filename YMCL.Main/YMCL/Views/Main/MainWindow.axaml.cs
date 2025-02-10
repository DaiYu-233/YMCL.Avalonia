using Ursa.Controls;
using YMCL.Public.Enum;

namespace YMCL.Views.Main;

public partial class MainWindow : UrsaWindow
{
    public MainWindow(out MainView view)
    {
        InitializeComponent();
        view = View;
        Public.Module.Ui.Setter.UpdateWindowStyle(this, action: () => { TitleRoot.IsVisible = true; });
        BindingEvent();
        Loaded += (_, _) =>
        {
            Public.Module.Ui.Setter.UpdateWindowStyle(this);
        };
    }

    private void BindingEvent()
    {
        PropertyChanged += (_, e) =>
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    Root.Margin = new Thickness(0);
                    Root.BorderThickness = new Thickness(0);
                    break;
                case WindowState.Maximized:
                    Root.Margin = new Thickness(20);
                    Root.BorderThickness = new Thickness(2);
                    break;
            }
        };
    }
}