using FluentAvalonia.UI.Controls;
using YMCL.Public.Module.App;
using YMCL.ViewModels;

namespace YMCL.Views.Main;

public partial class MainView : UserControl
{
    public readonly MainViewModel ViewModel = new();

    public MainView()
    {
        InitializeComponent();
        DataContext = ViewModel;
        BindingEvent();
    }

    private void BindingEvent()
    {
        Loaded += (_, _) => { InitDispatcher.OnMainViewLoaded(); };
        Nav.SelectionChanged += (_, e) =>
        {
            ViewModel.TogglePage(((e.SelectedItem as NavigationViewItem).Tag as string)!);
        };
    }
}