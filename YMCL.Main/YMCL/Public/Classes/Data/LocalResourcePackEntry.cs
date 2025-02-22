using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace YMCL.Public.Classes.Data;

public class LocalResourcePackEntry : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public Bitmap Icon { get; set; }
    [Reactive] public string Description { get; set; } = string.Empty;
}