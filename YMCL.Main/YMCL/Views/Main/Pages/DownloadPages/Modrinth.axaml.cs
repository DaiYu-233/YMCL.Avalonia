using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes.Data;
using YMCL.Public.Langs;

namespace YMCL.Views.Main.Pages.DownloadPages;

public partial class Modrinth : UserControl, INotifyPropertyChanged
{
    private SearchTabViewItemEntry _selectedItem;

    public ObservableCollection<SearchTabViewItemEntry> Items { get; set; } =
    [
        new()
        {
            Content = new ModrinthPages.ModrinthFetcher(),
            Title = $"{MainLang.Search}: {MainLang.Home}",
            Tag = "Home",
            CanClose = false,
            Host = nameof(Modrinth)
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

    public Modrinth()
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