using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Views.Main.Pages;

public partial class Search : UserControl
{
    public Search()
    {
        InitializeComponent();
        AggregateSearchListBox.SelectionChanged += async (_, e) =>
        {
            if (e.AddedItems.Count <= 0) return;
            if (e.AddedItems[0] is AggregateSearchEntry entry)
                YMCL.Public.Module.Ui.Special.AggregateSearchUi.HandleSelectedEntry(entry);
            await System.Threading.Tasks.Task.Delay(300);
            AggregateSearchListBox.SelectedIndex = -1;
        };
    }
}