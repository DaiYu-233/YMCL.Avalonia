using YMCL.Public.Enum;

namespace YMCL.Views.Main;

public partial class MainWindow : Window
{
    public MainWindow(out MainView view)
    {
        InitializeComponent();
        view = View;
        Public.Module.Ui.Setter.UpdateWindowStyle(this, systemAction: () => { TitleRoot.IsVisible = false; },
            launcherAction: () => { TitleRoot.IsVisible = true; });
        BindingEvent();
    }

    private void BindingEvent()
    {
        PropertyChanged += (_, e) =>
        {
            if (Data.Setting.WindowTitleBarStyle != Setting.WindowTitleBarStyle.Ymcl ||
                e.Property.Name != nameof(WindowState)) return;
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
        Data.Setting.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(Setting.WindowTitleBarStyle))
            {
                Public.Module.Ui.Setter.UpdateWindowStyle(this, Data.Setting.WindowTitleBarStyle,
                    systemAction: () => { TitleRoot.IsVisible = false; },
                    launcherAction: () => { TitleRoot.IsVisible = true; });
            }
        };
    }
}