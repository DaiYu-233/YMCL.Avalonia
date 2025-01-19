namespace YMCL.Views.Main;

public partial class MainWindow : Window
{
    public MainWindow(out MainView view)
    {
        InitializeComponent();
        view = View;
        Public.Module.Ui.Setter.UpdateWindowStyle(this);
    }
}