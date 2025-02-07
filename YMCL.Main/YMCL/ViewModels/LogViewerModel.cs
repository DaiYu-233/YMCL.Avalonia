using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using String = System.String;

namespace YMCL.ViewModels;

public class LogViewerModel : ReactiveObject
{
    public ObservableCollection<LogItemEntry> LogItems { get; } = [];
    public ObservableCollection<LogItemEntry> DisplayLogItems { get; } = [];
    [Reactive] public bool AutoScrollToEnd { get; set; } = true;
    [Reactive] public bool Error { get; set; } = true;
    [Reactive] public bool Info { get; set; } = true;
    [Reactive] public bool Debug { get; set; } = true;
    [Reactive] public bool Fatal { get; set; } = true;
    [Reactive] public bool Warning { get; set; } = true;
    [Reactive] public bool Exception { get; set; } = true;
    [Reactive] public bool StackTrace { get; set; } = true;
    [Reactive] public bool Unknown { get; set; } = true;

    public LogViewerModel(Action scrollToEndAction)
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is not (nameof(Error) or nameof(Info) or nameof(Debug) or nameof(Fatal)
                or nameof(Warning) or nameof(Exception) or nameof(StackTrace) or nameof(Unknown))) return;
            Filter();
            if (AutoScrollToEnd)
                scrollToEndAction?.Invoke();
        };
        LogItems.CollectionChanged += (_, e) =>
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                // 增量更新
                var newItems = e.NewItems.Cast<LogItemEntry>()
                    .Where(IsItemVisible)
                    .ToList();

                if (newItems.Count <= 0) return;
                foreach (var item in newItems)
                {
                    DisplayLogItems.Add(item);
                }
            }
            else
            {
                // 全量更新
                Filter();
            }

            if (AutoScrollToEnd)
                scrollToEndAction?.Invoke();
        };
    }


    public void Filter()
    {
        DisplayLogItems.Clear();
        LogItems.Where(a => a.Type switch
            {
                LogType.Error => Error,
                LogType.Info => Info,
                LogType.Debug => Debug,
                LogType.Fatal => Fatal,
                LogType.Warning => Warning,
                LogType.Exception => Exception,
                LogType.StackTrace => StackTrace,
                LogType.Unknown => Unknown,
                _ => true
            })
            .ToList().ForEach(a => { DisplayLogItems.Add(a); });
    }

    private bool IsItemVisible(LogItemEntry item)
    {
        return item.Type switch
        {
            LogType.Error => Error,
            LogType.Info => Info,
            LogType.Debug => Debug,
            LogType.Fatal => Fatal,
            LogType.Warning => Warning,
            LogType.Exception => Exception,
            LogType.StackTrace => StackTrace,
            LogType.Unknown => Unknown,
            _ => true
        };
    }
}