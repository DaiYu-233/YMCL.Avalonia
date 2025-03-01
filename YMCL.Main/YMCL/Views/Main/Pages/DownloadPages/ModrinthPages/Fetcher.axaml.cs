using Avalonia.Input;
using YMCL.Public.Classes.Data;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages.DownloadPages.ModrinthPages;

public partial class ModrinthFetcher : UserControl
{
    public ModrinthFetcher()
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
        Loaded += (_, _) => { _ = Animator.PageLoading.LevelTwoPage(this); };
    }

    private void SearchAction()
    {
        App.UiRoot.ViewModel.Download._modrinth.CreateNewPage(new SearchTabViewItemEntry()
        {
            CanClose = true,Host = nameof(Modrinth),
            Content = new SearchResult(string.IsNullOrWhiteSpace(SearchKey.Text) ? string.Empty : SearchKey.Text,
                string.IsNullOrWhiteSpace(SearchMcVersion.Text) ? string.Empty : SearchMcVersion.Text,
                SearchType.SelectedIndex),
            Title = string.IsNullOrWhiteSpace(SearchKey.Text)
                ? $"{MainLang.Search}({(SearchType.SelectedItem as ComboBoxItem).Content}): {MainLang.HotResource}"
                : $"{MainLang.Search}({(SearchType.SelectedItem as ComboBoxItem).Content}): {SearchKey.Text}"
        });
    }
}