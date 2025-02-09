using System.IO;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Microsoft.VisualBasic.FileIO;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YMCL.Public.Enum;
using YMCL.Public.Langs;

namespace YMCL.Public.Classes;

public class LocalModEntry : ReactiveObject
{
    [Reactive] public string FileName { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public bool IsEnable { get; set; }
    [Reactive] public Action Callback { get; set; }

    public TextDecorationCollection? Decoration => !IsEnable
        ? TextDecorations.Strikethrough
        : null;

    [Reactive] public string DisplayText { get; set; }
    [Reactive] public string Description { get; set; }
    [Reactive] public bool ShouldTranslateDescription { get; set; } = false;
    [Reactive] public bool ShouldTranslateInfoName { get; set; } = false;
    [Reactive] public string ModInfoName { get; set; }

    public void EnableOrDisable()
    {
        if (string.IsNullOrWhiteSpace(System.IO.Path.GetDirectoryName(Path))) return;
        if (System.IO.Path.GetExtension(Path) == ".disabled")
            File.Move(Path, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, $"{FileName}.jar"));
        if (System.IO.Path.GetExtension(Path) == ".jar")
            File.Move(Path, Path + ".disabled");
        Callback?.Invoke();
    }

    public async Task Delete()
    {
        var text = $"• {System.IO.Path.GetFileName(FileName)}";

        var title = YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows
            ? MainLang.MoveToRecycleBin
            : MainLang.DeleteSelect;
        var dialog = await ShowDialogAsync(title, text, b_cancel: MainLang.Cancel,
            b_primary: MainLang.Ok);
        if (dialog != ContentDialogResult.Primary) return;

        if (YMCL.Public.Const.Data.DesktopType == DesktopRunnerType.Windows)
        {
            FileSystem.DeleteFile(Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
        }
        else
        {
            File.Delete(Path);
        }

        Callback?.Invoke();
    }
}