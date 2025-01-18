using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YMCL.Views.Initialize.Pages;

public partial class Language : UserControl
{
    public Language()
    {
        InitializeComponent();
        EventBinding();
    }

    private void EventBinding()
    {
        LanguageListBox.SelectionChanged += (_, _) =>
        {
            
        };
    }
}