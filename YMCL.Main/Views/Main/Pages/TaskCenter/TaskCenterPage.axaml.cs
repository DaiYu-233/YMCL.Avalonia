using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Main.Public;

namespace YMCL.Main.Views.Main.Pages.TaskCenter;

public partial class TaskCenterPage : UserControl
{
    public TaskCenterPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
    }

    private void ControlProperty()
    {
    }

    private void BindingEvent()
    {
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
        };
    }
}