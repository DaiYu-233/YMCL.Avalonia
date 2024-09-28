using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using YMCL.Main.Public;
using YMCL.Main.Public.Controls.TaskManage;

namespace YMCL.Main.Views.Main.Pages.TaskCenter;

public partial class TaskCenterPage : UserControl
{
    public TaskCenterPage()
    {
        InitializeComponent();
        ControlProperty();
        BindingEvent();
        UpdateTaskNumber();
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

    public async void UpdateTaskNumber()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(100);
                Dispatcher.UIThread.Invoke(() =>
                {
                    var tasks = TaskContainer.Children;
                    var index = 1;
                    foreach (var item in tasks)
                    {
                        var task = item as PageTaskEntry;
                        task.TaskNumber.Text = $"#{index}";
                        index++;
                    }

                    if (tasks.Count >= 1)
                    {
                        Const.Window.main.TaskInfoBadge.IsVisible = true;
                        Const.Window.main.TaskInfoBadge.Value = tasks.Count;
                        NoTaskTip.IsVisible = false;
                    }
                    else
                    {
                        Const.Window.main.TaskInfoBadge.IsVisible = false;
                        NoTaskTip.IsVisible = true;
                    }
                });
            }
        });
    }
}