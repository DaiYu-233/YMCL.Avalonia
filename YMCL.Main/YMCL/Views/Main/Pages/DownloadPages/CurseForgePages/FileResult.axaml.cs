using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using YMCL.Public.Classes.Data.ResourceFetcher;
using YMCL.Public.Controls;
using YMCL.Public.Module.Ui;
using YMCL.Public.Module.Util;

namespace YMCL.Views.Main.Pages.DownloadPages.CurseForgePages;

public partial class FileResult : UserControl
{
    private readonly CurseForgeResourceEntry _entry;

    public FileResult(CurseForgeResourceEntry entry)
    {
        _entry = entry;
        InitializeComponent();
        FileInfo.DataContext = entry;
        Loaded += (_, _) =>
        {
            _ = Animator.PageLoading.LevelTwoPage(this);
        };
        _ = GetFiles();
    }

    private async System.Threading.Tasks.Task GetFiles()
    {
        var page = 1;
        var files = new List<IResourceFileEntry>();
        while (true)
        {
            var a = await Public.Module.IO.Network.CurseForge.GetFiles(_entry.Id, page);
            if (!a.success) break;
            if (a.data.Count == 0) break;
            files.AddRange(a.data);
            page++;
        }

        List<string> mcVersions = [];
        files.ForEach(x=>x.McVersions.ForEach(y =>
        {
            if(!mcVersions.Contains(y))
                mcVersions.Add(y);
        }));
        mcVersions.Sort(new VersionComparer());
        mcVersions.Reverse();
        mcVersions.ForEach(x =>
        {
            var control = new ResourceFileExpander();
            control.Expander.Header = x;
            files.ForEach(y =>
            {
                if (y.McVersions.Contains(x))
                {
                    control.ListView.Items.Add(y);
                }
            });
            Container.Children.Add(control);
        });
        Ring.IsVisible = false;
    }

    public FileResult()
    {
        InitializeComponent();
    }
}