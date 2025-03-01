using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes;
using YMCL.Public.Controls;
using YMCL.Public.Enum;

namespace YMCL.Views.Main.Pages;

public partial class Task : UserControl
{
    public Task()
    {
        InitializeComponent();
    }

    public void UpdateTasksTip()
    {
        NoTasksTip.IsVisible = !Data.TaskEntries.Any(x => x.IsVisible);
    }
}