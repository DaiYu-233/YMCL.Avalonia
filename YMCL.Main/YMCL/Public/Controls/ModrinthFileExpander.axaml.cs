using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DynamicData;
using YMCL.Public.Classes.Data;
using YMCL.Public.Enum;

namespace YMCL.Public.Controls;

public partial class ModrinthFileExpander : UserControl
{
    public ObservableCollection<ModrinthFile> Files { get; } = [];

    public ModrinthFileExpander(string name, ResourceType type)
    {
        InitializeComponent();
        Expander.Header = name;
        DataContext = this;
        ListView.SelectionChanged += async (_, _) =>
        {
            if (ListView.SelectedItem == null) return;
            Public.Module.Op.DownloadResource.SaveModrinth(type, ListView.SelectedItem as ModrinthFile);
            await Task.Delay(300);
            ListView.SelectedItem = null;
        };
    }

    public ModrinthFileExpander()
    {
    }
}