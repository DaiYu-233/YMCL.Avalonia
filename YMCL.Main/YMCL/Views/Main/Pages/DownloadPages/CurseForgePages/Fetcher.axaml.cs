using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DynamicData;
using MinecraftLaunch.Components.Provider;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;
using YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;

namespace YMCL.Views.Main.Pages.DownloadPages;

public partial class CurseForgeFetcher : UserControl
{
    public CurseForgeFetcher()
    {
        InitializeComponent();
        DataContext = this;
        BindingEvent();
    }

    private void BindingEvent()
    {
        SearchButton.Click += (_, _) => SearchAction();
        SearchKey.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            SearchAction();
            e.Handled = true;
        };
        SearchMcVersion.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter) return;
            SearchAction();
            e.Handled = true;
        };
        Loaded += (_, _) =>
        {
            _ = Animator.PageLoading.LevelTwoPage(this);
        };
    }

    private void SearchAction()
    {
        App.UiRoot.ViewModel.Download._curseForge.CreateNewPage(new SearchTabViewItemEntry()
        {
            CanClose = true,
            Content = new SearchResult(string.IsNullOrWhiteSpace(SearchKey.Text) ? string.Empty : SearchKey.Text,
                string.IsNullOrWhiteSpace(SearchMcVersion.Text) ? string.Empty : SearchMcVersion.Text,
                SearchType.SelectedIndex, SearchLoaderType.SelectedIndex),
            Title = string.IsNullOrWhiteSpace(SearchKey.Text) ?  $"{MainLang.Search}: {MainLang.HotResource}" : $"{MainLang.Search}: {SearchKey.Text}"
        });
    }
}