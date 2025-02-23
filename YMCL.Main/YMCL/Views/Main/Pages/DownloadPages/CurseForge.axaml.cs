using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages.DownloadPages;

public sealed partial class CurseForge : UserControl, INotifyPropertyChanged
{
    private SearchTabViewItemEntry _selectedItem;

    public ObservableCollection<SearchTabViewItemEntry> Items { get; set; } =
    [
        new()
        {
            Content = new CurseForgeFetcher(),
            Title = $"{MainLang.Search}: {MainLang.Home}",
            Tag = "Home",
            CanClose = false
        }
    ];

    public SearchTabViewItemEntry SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public void CreateNewPage(SearchTabViewItemEntry entry)
    {
        Items.Add(entry);
        SelectedItem = entry;
    }

    public CurseForge()
    {
        InitializeComponent();
        DataContext = this;
        SelectedItem = Items.FirstOrDefault();
    }
    
    public new event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}