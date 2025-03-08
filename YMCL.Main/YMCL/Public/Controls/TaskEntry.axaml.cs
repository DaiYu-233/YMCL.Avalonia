﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Splat.ModeDetection;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Operate;
using YMCL.Public.Enum;
using YMCL.ViewModels;

namespace YMCL.Public.Controls;

public partial class TaskEntry : UserControl
{
    public TaskEntryModel Model { get; }

    public TaskEntry(string name, ObservableCollection<SubTask> subTasks = null, TaskState state = TaskState.Waiting)
    {
        InitializeComponent();
        Model = new TaskEntryModel(this, state, subTasks);
        HeaderContent.DataContext = Model;
        SubTaskContainer.DataContext = Model;
        OperateButtonContainer.DataContext = Model;
        DataContext = Data.Instance;
        Model.Name = name;
        Data.TaskEntries.Add(this);
        YMCL.App.UiRoot.ViewModel.Task.UpdateTasksTip();
    }

    public TaskEntry()
    {
    }

    public void UpdateAction(Action action)
    {
        Model.ButtonAction = action;
    }

    public void UpdateDestoryAction(Action action)
    {
        Model.DestoryAction = action;
    }
    
    public void Rename(string name)
    {
        Model.Name = name;
    }

    public void AddOperateButton(TaskEntryOperateButtonEntry entry)
    {
        Model.OperateButtons.Add(entry);
    }

    public void UpdateButtonText(string text)
    {
        Model.ButtonDisplay = text;
    }

    public void UpdateValue(double value)
    {
        Model.NumberValue = true;
        Model.Value = value;
        Model.GetIsIndeterminate();
        Model.GetDisplayProgress();
    }

    public void UpdateSubFinishTask(SubTask task, int count)
    {
        task.FailedTask = count;
    }

    public void UpdateSubTotalTask(SubTask task, int count)
    {
        task.TotalTask = count;
    }

    public void AddSubTask(SubTask task)
    {
        Model.SubTasks.Add(task);
    }

    public void AddSubTaskRange(SubTask[] task)
    {
        foreach (var subTask in task)
        {
            Model.SubTasks.Add(subTask);
        }
    }

    public void RemoveSubTask(SubTask task)
    {
        Model.SubTasks.Remove(task);
    }

    public void Destroy()
    {
        Data.TaskEntries.Remove(this);
        YMCL.App.UiRoot.ViewModel.Task.UpdateTasksTip();
    }

    public void FinishWithSuccess()
    {
        Model.CanRemove = true;
        Model.ButtonIsEnable = true;
        Model.State = TaskState.Finished;
        foreach (var task in Model.SubTasks)
        {
            task.Finish();
        }
    }

    public void FinishWithError()
    {
        Model.CanRemove = true;
        Model.State = TaskState.Error;
    }

    private int index;

    public void AdvanceSubTask()
    {
        Model.SubTasks[index].Finish();
        if (index <= Model.SubTasks.Count - 2)
        {
            Model.SubTasks[index + 1].State = TaskState.Running;
        }

        index++;
    }

    public void Cancel()
    {
        Model.State = TaskState.Canceled;
        Model.CanRemove = true;
    }

    public void CancelWaitFinish()
    {
        Model.State = TaskState.Canceling;
        Model.ButtonIsEnable = false;
    }

    public void CancelWithSuccess()
    {
        Model.State = TaskState.Canceling;
        Model.ButtonIsEnable = false;
        FinishWithSuccess();
    }

    public void CancelFinish()
    {
        Model.State = TaskState.Canceled;
        Model.ButtonIsEnable = true;
        Model.CanRemove = true;
    }
}