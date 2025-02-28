using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using DynamicData;
using YMCL.Public.Classes.Data;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.Ui;

namespace YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;

public partial class SearchResult : UserControl
{
    private readonly string _keyword;
    private readonly string _mcVersion;
    private readonly int _type;
    private readonly int _loader;
    private int _page = 1;

    public SearchResult()
    {
        InitializeComponent();
    }

    public SearchResult(string keyword, string mcVersion, int type, int loader)
    {
        InitializeComponent();
        _keyword = keyword;
        _mcVersion = mcVersion;
        _type = type;
        _loader = loader;
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
            var item = ListBox.SelectedItem as CurseForgeResourceEntry;
            App.UiRoot.ViewModel.Download._curseForge.CreateNewPage(new SearchTabViewItemEntry()
            {
                CanClose = true,
                Content = new ModFileResult(item!),
                Title = $"{item.Type}: {item.Name}"
            });
            ListBox.SelectedItem = null;
        };
    }

    private async System.Threading.Tasks.Task Search()
    {
        Ring.IsVisible = true;
        LoadMoreBtn.IsVisible = false;
        var classId = _type switch
        {
            0 => 0, //Any
            1 => 6, //Mod
            2 => 12, //MaterialPack
            3 => 17, //Map
            4 => 6552, //ShaderPack
            5 => 6945, //DataPack
            6 => 4471, //ModPack
            _ => 0
        };
        var data = await Public.Module.IO.Network.CurseForge.Search(_keyword, classId, _page, _mcVersion,
            (ModLoaderType)_loader);
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
        data.data.ForEach(x => ListBox.Items.Add(x));
    }
}