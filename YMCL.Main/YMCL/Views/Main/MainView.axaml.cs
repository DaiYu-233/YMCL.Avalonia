using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using YMCL.Public.Enum;
using YMCL.Public.Module.Init;
using YMCL.ViewModels;
using YMCL.Views.Main.Drawers.MsgHistory;

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
    }

    private async void FocusButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var options = new DrawerOptions()
        {
            Position = Position.Right,
            Buttons = DialogButton.None,
            CanLightDismiss = true,
            IsCloseButtonVisible = true,
            Title = "消息历史",
            CanResize = true,
        };
        await Drawer.ShowModal<MsgHistory, MsgHistoryViewModel>(Data.UiProperty.MsgHistoryViewModel, null, options);
    }
}