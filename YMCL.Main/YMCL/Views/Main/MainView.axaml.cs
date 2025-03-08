using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using YMCL.Public.Controls.Drawers;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.App.Init;
using YMCL.Public.Module.Ui.Special;
using YMCL.ViewModels;
using YMCL.Views.Main.Pages;

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
                await Public.Module.Ui.Special.DropHandler.HandleFiles(item.Path.LocalPath);
            }

            if (Data.UiProperty.IsAllImport)
                Notice(MainLang.ImportFinish, NotificationType.Success);
            Data.UiProperty.IsAllImport = false;
        }
        if (e.Data.Contains(DataFormats.Text))
        {
            var text = e.Data.GetText(); 
            Public.Module.Ui.Special.DropHandler.HandleText(text!);
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
}