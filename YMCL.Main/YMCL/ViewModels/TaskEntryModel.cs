using System.Collections.ObjectModel;
using System.Timers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.ViewModels;

public class TaskEntryModel : ReactiveObject
{
    private readonly Timer _timer = new();
    private double _totalTime;
    [Reactive] public ObservableCollection<SubTask> SubTasks { get; set; }
    [Reactive] public ObservableCollection<TaskEntryOperateButtonEntry> OperateButtons { get; set; } = [];
    [Reactive] public double Time { get; set; }
    [Reactive] public TaskState State { get; set; }
    [Reactive] public bool NumberValue { get; set; }
    [Reactive] public bool CanRemove { get; set; }
    [Reactive] public bool ButtonIsEnable { get; set; } = true;
    [Reactive] public double Value { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public string TopRightInfo { get; set; }
    [Reactive] public string BottomLeftInfo { get; set; }
    [Reactive] public double DisplayProgress { get; set; }
    [Reactive] public bool DisplayIsIndeterminate { get; set; }
    [Reactive] public string ButtonDisplay { get; set; }
    [Reactive] public Action ButtonAction { get; set; }
    [Reactive] public Action DestoryAction { get; set; }
    [Reactive] public TaskEntry Instance { get; set; }


    public TaskEntryModel(TaskEntry entry, TaskState state, ObservableCollection<SubTask>? subTasks)
    {
        _timer.Interval = 500;
        _timer.Elapsed += OnTimerElapsed!;
        Instance = entry;
        State = state;
        SubTasks = subTasks ?? [];
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(State))
            {
                Refresh();
            }
            if (e.PropertyName == nameof(CanRemove))
            {
                UpdateButtonDisplay();
            }
        };
        Refresh();
    }

    private void Refresh()
    {
        HandleStateChange(State);
        GetDisplayProgress();
        UpdateButtonDisplay();
        GetIsIndeterminate();
    }

    private void HandleStateChange(TaskState newState)
    {
        switch (newState)
        {
            case TaskState.Running:
            case TaskState.Canceling:
            case TaskState.Waiting:
                _timer.Start();
                break;

            case TaskState.Paused:
            case TaskState.Error:
            case TaskState.Finished:
            case TaskState.Canceled:
                _timer.Stop();
                break;
        }
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        _totalTime += _timer.Interval;
        Time = _totalTime;
    }

    private void UpdateButtonDisplay()
    {
        ButtonDisplay = CanRemove ? MainLang.Remove : MainLang.Cancel;
        if (CanRemove)
        {
            ButtonAction = () =>
            {
                Instance.Destory();
                DestoryAction?.Invoke();
            };
        }
    }

    public void GetDisplayProgress()
    {
        if (!NumberValue)
        {
            DisplayProgress = State switch
            {
                TaskState.Finished => 100,
                TaskState.Error => 50,
                TaskState.Canceled => 50,
                TaskState.Paused => 50,
                _ => Value
            };
        }
        else
        {
            DisplayProgress = Math.Round(Value, 1);
        }
    }

    public void GetIsIndeterminate()
    {
        if (!NumberValue)
        {
            DisplayIsIndeterminate = State switch
            {
                TaskState.Finished => false,
                TaskState.Error => false,
                TaskState.Canceled => false,
                TaskState.Paused => false,
                _ => true
            };
        }
        else
        {
            DisplayIsIndeterminate = State switch
            {
                TaskState.Canceling => true,
                _ => false
            };
        }
    }

    private TaskEntryModel GetEntry()
    {
        return this;
    }

    public void ButtonActionCommand()
    {
        ButtonAction?.Invoke();
    }
}