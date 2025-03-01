using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Ursa.Controls;
using YMCL.Public.Classes;
using YMCL.Public.Classes.Data;
using YMCL.Public.Module.Ui.Special;

namespace YMCL.Views.Main.Pages;

public partial class Search : UserControl
{
    public Search(bool isPage)
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

    public Search()
    {
        InitializeComponent();
        AggregateSearchListBox.SelectionChanged += async (_, e) =>
        {
            if (e.AddedItems.Count <= 0) return;
            if (e.AddedItems[0] is AggregateSearchEntry entry)
                YMCL.Public.Module.Ui.Special.AggregateSearchUi.HandleSelectedEntry(entry);
            AggregateSearchListBox.SelectedIndex = -1;
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        };
        Width = 870;
        Height = 550;
        PointerMoved += (_, _) =>
        {
            AggregateSearchBox.Focus();
        };
        KeyDown += (_, e) =>
        {
            if (e.Key is not Key.Escape) return;
            var host = TopLevel.GetTopLevel(this);
            if (host is DialogWindow window)
            {
                window.Close();
            }
        };
        Loaded += (_, _) =>
        {
            AggregateSearchBox.Focus();
        };
    }
}