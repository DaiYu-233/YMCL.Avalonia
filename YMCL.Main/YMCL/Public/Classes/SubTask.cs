using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Semi.Avalonia;
using YMCL.Public.Enum;

namespace YMCL.Public.Classes;

public sealed class SubTask : ReactiveObject
{
    public SubTask(string name, int totalTask = 1, int finishedTask = 0)
    {
        Name = name;
        TotalTask = totalTask;
        FinishedTask = finishedTask;
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(State))
            {
                GetIcon();
            }
        };
    }

    [Reactive] public string Name { get; set; }
    [Reactive] public int TotalTask { get; set; }
    [Reactive] public int FinishedTask { get; set; }
    [Reactive] public int FailedTask { get; set; }
    [Reactive] public Control Icon { get; set; }
    [Reactive] public TaskState State { get; set; }

    private void GetIcon()
    {
        Icon = State switch
        {
            TaskState.Finished => new PathIcon
            {
                Width = 14, Height = 16,
                Data = Geometry.Parse
                    ("F1 M 0 9.375 C 0 9.205729 0.061849 9.059245 0.185547 8.935547 C 0.309245 8.81185 0.455729 8.75 0.625 8.75 C 0.794271 8.75 0.940755 8.81185 1.064453 8.935547 L 6.875 14.736328 L 18.935547 2.685547 C 19.059244 2.56185 19.205729 2.5 19.375 2.5 C 19.54427 2.5 19.690754 2.56185 19.814453 2.685547 C 19.93815 2.809246 20 2.95573 20 3.125 C 20 3.294271 19.93815 3.440756 19.814453 3.564453 L 7.314453 16.064453 C 7.190755 16.188152 7.044271 16.25 6.875 16.25 C 6.705729 16.25 6.559244 16.188152 6.435547 16.064453 L 0.185547 9.814453 C 0.061849 9.690756 0 9.544271 0 9.375 Z ")
            },
            TaskState.Waiting => new PathIcon
            {
                Width = 12, Height = 16,
                Data = Geometry.Parse
                    ("F1 M 3.125 10.625 C 2.955729 10.625 2.809245 10.563151 2.685547 10.439453 C 2.561849 10.315756 2.5 10.169271 2.5 10 C 2.5 9.830729 2.561849 9.684245 2.685547 9.560547 C 2.809245 9.43685 2.955729 9.375 3.125 9.375 L 16.875 9.375 C 17.04427 9.375 17.190754 9.43685 17.314453 9.560547 C 17.43815 9.684245 17.5 9.830729 17.5 10 C 17.5 10.169271 17.43815 10.315756 17.314453 10.439453 C 17.190754 10.563151 17.04427 10.625 16.875 10.625 Z ")
            },
            TaskState.Running => new ProgressRing { Width = 16, Height = 16 },
            _ => null
        };
    }

    public void Finish()
    {
        State = TaskState.Finished;
        FinishedTask = TotalTask;
    }
}