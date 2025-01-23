using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace YMCL.Public.Classes;

public class LocalModEntry : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public bool IsEnable { get; set; }

    public TextDecorationCollection? Decoration => !IsEnable
        ? TextDecorations.Strikethrough : null;
}