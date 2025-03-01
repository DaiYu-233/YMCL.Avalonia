using System.Linq;
using Avalonia.Controls.Notifications;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages.DownloadPages.ModrinthPages;

public partial class SearchResult : UserControl
{
    private readonly string _keyword;
    private readonly string _mcVersion;
    private readonly int _type;
    private int _page = 1;

    public SearchResult()
    {
        InitializeComponent();
    }

    public SearchResult(string keyword, string mcVersion, int type)
    {
        InitializeComponent();
        _keyword = keyword;
        _mcVersion = mcVersion;
        _type = type;
        _page = 1;
        ListBox.Items.Clear();
        _ = Search();
        LoadMoreBtn.Click += (_, _) =>
        {
            _page++;
            _ = Search();
        };
        Loaded += (_, _) => { _ = Animator.PageLoading.LevelTwoPage(this); };
        ListBox.SelectionChanged += (_, _) =>
        {
            if (ListBox.SelectedItem == null) return;
            var item = ListBox.SelectedItem as ModrinthResourceEntry;
            App.UiRoot.ViewModel.Download._modrinth.CreateNewPage(new SearchTabViewItemEntry()
            {
                CanClose = true, Host = nameof(DownloadPages.Modrinth),
                Content = new ModFileResult(item!, type),
                Title = $"{item.DisplayType}: {item.Name}"
            });
            ListBox.SelectedItem = null;
        };
    }

    private async System.Threading.Tasks.Task Search()
    {
        Ring.IsVisible = true;
        LoadMoreBtn.IsVisible = false;
        var data = await Public.Module.IO.Network.Modrinch.Search(_keyword, _page, _mcVersion,_type);
        Ring.IsVisible = false;
        if (!data.success)
        {
            Notice(MainLang.LoadFail, NotificationType.Error);
            return;
        }

        if (data.data.Count == 0)
        {
            Notice(MainLang.SearchNoResult, NotificationType.Error);
            return;
        }

        LoadMoreBtn.IsVisible = true;
        data.data.ForEach(x => 
            ListBox.Items.Add(x));
    }
}