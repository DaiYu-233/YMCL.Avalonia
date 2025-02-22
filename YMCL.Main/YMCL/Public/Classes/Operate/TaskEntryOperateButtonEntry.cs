using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace YMCL.Public.Classes.Operate;

public class TaskEntryOperateButtonEntry : ReactiveObject
{
    [Reactive] public Action Action { get; set; }

    public void ActionInvoke()
    {
        Action.Invoke();
    }

    [Reactive] public object Content { get; set; }

    public TaskEntryOperateButtonEntry(object content, Action action)
    {
        Content = content;
        Action = action;
    }

    public void UpdateAction(Action action)
    {
        Action = action;
    }

    public void UpdateContent(object context)
    {
        Content = context;
    }
}