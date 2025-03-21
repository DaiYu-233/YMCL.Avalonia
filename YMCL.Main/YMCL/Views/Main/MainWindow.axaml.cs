using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Ursa.Common;
using Ursa.Controls;
using Ursa.Controls.Options;
using YMCL.Public.Controls.Drawers;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Util.Platform;
using YMCL.Views.Main.Pages;

namespace YMCL.Views.Main;

public partial class MainWindow : UrsaWindow
{
    private DateTime _lastShiftPressTime;

    public MainWindow(out MainView view)
    {
        InitializeComponent();
        view = View;
        Public.Module.Ui.Setter.UpdateWindowStyle(this, action: () => { TitleRoot.IsVisible = true; });
        BindingEvent();
        AddButtonToTitleBar();
    }

    public MainWindow()
    {
    }

    private void AddButtonToTitleBar()
    {
        var msgHistory = new Button()
        {
            Classes = { "icon-custom-button" },
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Content = PathGeometry.Parse(
                "F1 M 0 19.375 C 0 19.205729 0.027669 19.029947 0.083008 18.847656 C 0.138346 18.665365 0.185547 18.492838 0.224609 18.330078 C 0.374349 17.711588 0.530599 17.099609 0.693359 16.494141 C 0.85612 15.888672 1.01237 15.276693 1.162109 14.658203 C 0.777995 13.942058 0.488281 13.193359 0.292969 12.412109 C 0.097656 11.630859 0 10.833334 0 10.019531 C 0 9.101562 0.118815 8.214519 0.356445 7.358398 C 0.594076 6.502279 0.929362 5.703125 1.362305 4.960938 C 1.795247 4.21875 2.316081 3.543295 2.924805 2.93457 C 3.533528 2.325848 4.208984 1.803387 4.951172 1.367188 C 5.693359 0.93099 6.490885 0.594076 7.34375 0.356445 C 8.196614 0.118816 9.082031 0 10 0 C 10.917969 0 11.801758 0.118816 12.651367 0.356445 C 13.500977 0.594076 14.296875 0.93099 15.039062 1.367188 C 15.78125 1.803387 16.456705 2.324219 17.06543 2.929688 C 17.674152 3.535156 18.196613 4.208984 18.632812 4.951172 C 19.06901 5.693359 19.405924 6.489258 19.643555 7.338867 C 19.881184 8.188477 20 9.072266 20 9.990234 C 20 10.901693 19.881184 11.782227 19.643555 12.631836 C 19.405924 13.481445 19.072266 14.277344 18.642578 15.019531 C 18.212891 15.761719 17.695312 16.438803 17.089844 17.050781 C 16.484375 17.66276 15.812174 18.18685 15.073242 18.623047 C 14.33431 19.059244 13.541666 19.397787 12.695312 19.638672 C 11.848958 19.879557 10.966797 20 10.048828 20 C 9.241536 20 8.447266 19.907227 7.666016 19.72168 C 6.884766 19.536133 6.136067 19.257812 5.419922 18.886719 L 0.771484 19.980469 C 0.70638 19.99349 0.657552 20 0.625 20 C 0.449219 20 0.301107 19.939779 0.180664 19.819336 C 0.060221 19.698893 0 19.550781 0 19.375 Z M 9.960938 18.75 C 10.768229 18.75 11.546224 18.645834 12.294922 18.4375 C 13.043619 18.229166 13.743488 17.936197 14.394531 17.558594 C 15.045572 17.18099 15.639648 16.726889 16.176758 16.196289 C 16.713867 15.66569 17.172852 15.07487 17.553711 14.423828 C 17.93457 13.772787 18.229166 13.072917 18.4375 12.324219 C 18.645832 11.575521 18.75 10.797526 18.75 9.990234 C 18.75 9.182943 18.645832 8.406576 18.4375 7.661133 C 18.229166 6.91569 17.93457 6.220703 17.553711 5.576172 C 17.172852 4.931641 16.715494 4.342448 16.181641 3.808594 C 15.647785 3.27474 15.055338 2.819012 14.404297 2.441406 C 13.753255 2.063803 13.055013 1.770834 12.30957 1.5625 C 11.564127 1.354168 10.791016 1.25 9.990234 1.25 C 9.189453 1.25 8.416341 1.354168 7.670898 1.5625 C 6.925456 1.770834 6.228841 2.063803 5.581055 2.441406 C 4.933268 2.819012 4.344075 3.273113 3.813477 3.803711 C 3.282877 4.334311 2.827148 4.921875 2.446289 5.566406 C 2.06543 6.210938 1.770833 6.905925 1.5625 7.651367 C 1.354167 8.396811 1.25 9.169922 1.25 9.970703 C 1.25 10.393881 1.276042 10.784506 1.328125 11.142578 C 1.380208 11.500651 1.455078 11.850586 1.552734 12.192383 C 1.650391 12.53418 1.767578 12.87435 1.904297 13.212891 C 2.041016 13.551433 2.197266 13.90625 2.373047 14.277344 C 2.42513 14.381511 2.451172 14.482422 2.451172 14.580078 C 2.451172 14.651693 2.431641 14.785156 2.392578 14.980469 C 2.353516 15.175781 2.301432 15.400391 2.236328 15.654297 C 2.171224 15.908203 2.101237 16.181641 2.026367 16.474609 C 1.951497 16.767578 1.878255 17.047525 1.806641 17.314453 C 1.735026 17.581381 1.669922 17.823893 1.611328 18.041992 C 1.552734 18.260092 1.510417 18.421225 1.484375 18.525391 C 1.927083 18.421225 2.364909 18.317057 2.797852 18.212891 C 3.230794 18.108725 3.66862 18.001303 4.111328 17.890625 C 4.332682 17.838541 4.563802 17.776693 4.804688 17.705078 C 5.045573 17.633463 5.279948 17.597656 5.507812 17.597656 C 5.559896 17.597656 5.608724 17.605795 5.654297 17.62207 C 5.69987 17.638346 5.748698 17.65625 5.800781 17.675781 C 6.152343 17.838541 6.492513 17.986654 6.821289 18.120117 C 7.150065 18.25358 7.480469 18.365885 7.8125 18.457031 C 8.144531 18.548178 8.486328 18.619791 8.837891 18.671875 C 9.189453 18.723959 9.563802 18.75 9.960938 18.75 Z M 6.875 8.75 C 6.705729 8.75 6.559244 8.688151 6.435547 8.564453 C 6.311849 8.440756 6.25 8.294271 6.25 8.125 C 6.25 7.95573 6.311849 7.809246 6.435547 7.685547 C 6.559244 7.56185 6.705729 7.5 6.875 7.5 L 13.125 7.5 C 13.294271 7.5 13.440755 7.56185 13.564453 7.685547 C 13.68815 7.809246 13.75 7.95573 13.75 8.125 C 13.75 8.294271 13.68815 8.440756 13.564453 8.564453 C 13.440755 8.688151 13.294271 8.75 13.125 8.75 Z M 6.875 12.5 C 6.705729 12.5 6.559244 12.438151 6.435547 12.314453 C 6.311849 12.190756 6.25 12.044271 6.25 11.875 C 6.25 11.705729 6.311849 11.559245 6.435547 11.435547 C 6.559244 11.31185 6.705729 11.25 6.875 11.25 L 10.625 11.25 C 10.794271 11.25 10.940755 11.31185 11.064453 11.435547 C 11.18815 11.559245 11.25 11.705729 11.25 11.875 C 11.25 12.044271 11.18815 12.190756 11.064453 12.314453 C 10.940755 12.438151 10.794271 12.5 10.625 12.5 Z ")
        };
        msgHistory.Click += async (_, _) =>
        {
            var options = new DrawerOptions()
            {
                Position = Ursa.Common.Position.Right,
                Buttons = DialogButton.None,
                CanLightDismiss = true,
                IsCloseButtonVisible = true,
                MaxWidth = 420,
                MinWidth = 420,
                Title = "消息历史",
                CanResize = true,
            };
            await Drawer.ShowModal<MsgHistory, Data>(Data.Instance, null, options);
        };
        TitleBar.AddButton(msgHistory);
    }

    private void BindingEvent()
    {
        PropertyChanged += (_, e) =>
        {
            if (Data.DesktopType == DesktopRunnerType.MacOs)
            {
                var platform = TryGetPlatformHandle();
                if (platform is not null)
                {
                    var nsWindow = platform.Handle;
                    if (nsWindow != IntPtr.Zero)
                    {
                        try
                        {
                            WindowHandler.RefreshTitleBarButtonPosition(nsWindow);
                            WindowHandler.HideZoomButton(nsWindow);
                            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
                            TitleBar.MaximizeButton.IsVisible = false;
                            TitleBar.MinimizeButton.IsVisible = false;
                            TitleBar.CloseButton.IsVisible = false;
                            TitleText.HorizontalAlignment = HorizontalAlignment.Right;
                            TitleText.Margin = new Thickness(0, 0, 10, 0);
                            TitleBar.FunctionRoot.Margin = new Thickness(0, 0, 175, 0);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    }
                }
            }
            if (e.Property.Name != nameof(WindowState)) return;

            switch (WindowState)
            {
                case WindowState.Normal:
                    Root.CornerRadius = new CornerRadius(8);
                    if (Data.DesktopType == DesktopRunnerType.Windows && Environment.OSVersion.Version.Major < 10)
                    {
                        Root.CornerRadius = new CornerRadius(0);
                    }

                    break;
                case WindowState.Maximized:
                    if (Data.DesktopType != DesktopRunnerType.MacOs)
                    {
                        Root.CornerRadius = new CornerRadius(0);
                    }

                    break;
            }
        };
        Loaded += (_, _) => { Public.Module.Ui.Setter.UpdateWindowStyle(this); };
        KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.LeftShift or Key.RightShift)) return;
            if ((DateTime.Now - _lastShiftPressTime).TotalMilliseconds < 300)
            {
                var options = new DialogOptions()
                {
                    ShowInTaskBar = true,
                    IsCloseButtonVisible = true,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    CanDragMove = true
                };
                Dialog.ShowCustom<Search, Data>(Data.Instance, options: options);
            }

            _lastShiftPressTime = DateTime.Now;
        };
        AddHandler(DragDrop.DropEvent, DropHandler);
        if (Data.DesktopType == DesktopRunnerType.Windows && Environment.OSVersion.Version.Major < 10)
        {
            Root.CornerRadius = new CornerRadius(0);
        }
    }

    private static async void DropHandler(object? sender, DragEventArgs e)
    {
        if (e is null) return;
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
        else if (e.Data.Contains(DataFormats.Text))
        {
            var text = e.Data.GetText();
            Public.Module.Ui.Special.DropHandler.HandleText(text!);
        }
    }
}