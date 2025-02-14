using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using YMCL.Public.Controls.Drawers;
using YMCL.Public.Enum;
using YMCL.Public.Module.Init;
using YMCL.Public.Module.Ui.Special;
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
        if (Data.DesktopType == DesktopRunnerType.Windows)
        {
            NavMusic.IsVisible = true;
        }

        
    }

    private void BindingEvent()
    {
        Loaded += (_, _) => { InitDispatcher.OnMainViewLoaded(); };
        Nav.SelectionChanged += (_, e) =>
        {
            ViewModel.TogglePage(((e.SelectedItem as NavigationViewItem).Tag as string)!);
        };
        SizeChanged += (_, e) =>
        {
            Data.UiProperty.TaskEntryHeaderWidth = e.NewSize.Width - 230;
        };
        AddHandler(DragDrop.DropEvent, DropHandler);
    }

    private async void DropHandler(object? sender, DragEventArgs e)
    {
        if(e is null) return;
        if (e.Data.Contains(DataFormats.Files))
        {
            foreach (var item in e.Data.GetFiles())
            {
                await Public.Module.Ui.Special.DropHandler.Handle(item.Path.LocalPath);
            }
        }
    }
}