using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module.IO.Network;

namespace YMCL.Views.Main.Pages.MusicPages;

public partial class Search : UserControl
{
    private string _realDefaultKeyword = string.Empty;
    private string _lastKeyword = string.Empty;
    private uint _number = 1;
    private int _page = 1;

    public Search()
    {
        InitializeComponent();
        BindingEvent();
        _ = SetDefaultKeyWord();
    }

    private void BindingEvent()
    {
        SearchBox.KeyDown += (o, e) =>
        {
            if (e.Key != Key.Enter) return;
            _ = SearchAction();
            e.Handled = true;
        };
        SearchBtn.Click += (_, _) => { _ = SearchAction(); };
        LoadMoreBtn.Click += (_, _) => { _ = LoadMore(); };
        SearchSongListView.SelectionChanged += async (_, _) =>
        {
            var item = SearchSongListView.SelectedItem;
            if (item == null) return;
            Data.RecordSongEntries.Add((item as RecordSongEntry)!);
            await System.Threading.Tasks.Task.Delay(200);
            SearchSongListView.SelectedItem = null;
            Data.UiProperty.SelectedRecordSong = Data.RecordSongEntries.Last();
            _ = File.WriteAllTextAsync(ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(Data.RecordSongEntries, Formatting.Indented));
        };
    }

    private async System.Threading.Tasks.Task SearchAction()
    {
        var key = !string.IsNullOrWhiteSpace(SearchBox.Text) ? SearchBox.Text : _realDefaultKeyword;
        if (string.IsNullOrWhiteSpace(key)) return;
        _number = 1;
        _page = 1;
        _lastKeyword = key;
        Data.SearchSongEntries.Clear();
        Loading.IsVisible = true;
        SearchBox.IsEnabled = false;
        SearchBtn.IsEnabled = false;
        LoadMoreBtn.IsVisible = false;
        var songs = await NeteaseMusic.SearchForSingleSong(key);
        if (songs.Count == 0)
        {
            Toast(MainLang.SearchNoResult, NotificationType.Warning);
        }

        songs.ForEach(song =>
        {
            Data.SearchSongEntries.Add(new RecordSongEntry()
            {
                Duration = TimeSpan.FromMilliseconds(song.dt),
                Number = _number,
                DisplayNumber = _number < 10 ? $"0{_number}" : _number.ToString(),
                DisplayDuration = Public.Module.Value.Converter.MsToTime(song.dt),
                SongName = song.name,
                SongId = song.id,
                AlbumId = song.al.id,
                AlbumName = song.al.name,
                Type = SongType.Netease,
                SongPicUri = song.al.picUrl,
                SongAuthors = string.Join("/", song.ar.Select(a => a.name))
            });
            _number++;
        });
        Loading.IsVisible = false;
        LoadMoreBtn.IsVisible = songs.Count >= 30;
        SearchBox.IsEnabled = true;
        SearchBtn.IsEnabled = true;
    }

    private async System.Threading.Tasks.Task SetDefaultKeyWord()
    {
        var key = await NeteaseMusic.GetDefaultKeyword();
        if (string.IsNullOrWhiteSpace(key.real) || string.IsNullOrWhiteSpace(key.show)) return;
        _realDefaultKeyword = key.real;
        SearchBox.Watermark = key.show;
    }

    private async System.Threading.Tasks.Task LoadMore()
    {
        _page++;
        Loading.IsVisible = true;
        LoadMoreBtn.IsVisible = false;
        SearchBox.IsEnabled = false;
        SearchBtn.IsEnabled = false;
        var songs = await NeteaseMusic.SearchForSingleSong(_lastKeyword, _page);
        if (songs.Count == 0)
        {
            Toast(MainLang.SearchNoResult, NotificationType.Warning);
        }

        songs.ForEach(song =>
        {
            Data.SearchSongEntries.Add(new RecordSongEntry()
            {
                Duration = TimeSpan.FromMilliseconds(song.dt),
                Number = _number,
                DisplayNumber = _number < 10 ? $"0{_number}" : _number.ToString(),
                DisplayDuration = Public.Module.Value.Converter.MsToTime(song.dt),
                SongName = song.name,
                SongId = song.id,
                AlbumId = song.al.id,
                AlbumName = song.al.name,
                Type = SongType.Netease,
                SongPicUri = song.al.picUrl,
                SongAuthors = string.Join("/", song.ar.Select(a => a.name))
            });
            _number++;
        });
        Loading.IsVisible = false;
        LoadMoreBtn.IsVisible = songs.Count >= 30;
        SearchBox.IsEnabled = true;
        SearchBtn.IsEnabled = true;
    }
}