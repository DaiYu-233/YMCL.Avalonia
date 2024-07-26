using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using static YMCL.Main.Public.Classes.PlaySongListViewItemEntry;

namespace YMCL.Main.Views.Main.Pages.Music;

public partial class MusicPage : UserControl
{
    private bool _firstLoad = true;
    private bool _isHoldingSlider;
    private bool _isPlaying;
    private PlaySongListViewItemEntry _selectedItem;
    private IWavePlayer _waveOut;

    private AudioFileReader _waveSource;
    private WaveStream _waveStream;
    public List<UrlImageDataListEntry> BitmapDataList = new();
    private string keyword = string.Empty;
    private int page;
    public List<PlaySongListViewItemEntry> playSongList = new();
    private Timer timer;

    public MusicPage()
    {
        InitializeComponent();
        BindingEvent();
    }

    private void BindingEvent()
    {
        VolumeBtn.PointerPressed += (_, _) =>
        {
            if (VolumeRoot.Opacity == 0)
                VolumeRoot.Opacity = (double)Application.Current.Resources["Opacity"]!;
            else
                VolumeRoot.Opacity = 0;
        };
        VolumeSlider.ValueChanged += (_, _) =>
        {
            VolumeText.Text = Math.Round(VolumeSlider.Value).ToString();
            _waveOut.Volume = (float)VolumeSlider.Value / 100;
        };
        Loaded += (s, e) =>
        {
            Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
            if (_firstLoad)
            {
                _firstLoad = false;
                var list = JsonConvert.DeserializeObject<List<PlaySongListViewItemEntry>>(
                    File.ReadAllText(Const.PlayerDataPath));
                list.ForEach(list =>
                {
                    playSongList.Add(list);
                    PlayListView.Items.Add(list);
                });

                timer = new Timer(300);
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                timer.Enabled = true;
            }
        };
        UpSong.Click += (s, e) =>
        {
            var index = PlayListView.SelectedIndex;
            var count = PlayListView.Items.Count;
            if (index == 0) return;
            var item = PlayListView.SelectedItem;
            if (item == null) return;
            PlayListView.Items.Remove(item);
            PlayListView.Items.Insert(index - 1, item);
            PlayListView.SelectedItem = item;
        };
        DownSong.Click += (s, e) =>
        {
            var index = PlayListView.SelectedIndex;
            var count = PlayListView.Items.Count;
            if (index == count - 1) return;
            var item = PlayListView.SelectedItem;
            if (item == null) return;
            PlayListView.Items.Remove(item);
            PlayListView.Items.Insert(index + 1, item);
            PlayListView.SelectedItem = item;
        };
        SearchBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                page = 0;
                keyword = SearchBox.Text;
                _ = SearchForListViewAsync(keyword!, page);
            }
        };
        SearchBtn.Click += (s, e) =>
        {
            page = 0;
            keyword = SearchBox.Text;
            _ = SearchForListViewAsync(keyword!, page);
        };
        LoadMoreBtn.Click += (s, e) => { _ = LoadMoreAsync(); };
        SearchSongListView.SelectionChanged += async (_, _) =>
        {
            if (SearchSongListView.SelectedIndex == -1) return;
            var song = SearchSongListView.SelectedItem as SearchSongListViewItemEntry;
            if (song == null) return;
            PlayListView.Items.Add(new PlaySongListViewItemEntry
            {
                SongName = song.SongName,
                SongId = song.SongId,
                Authors = song.Authors,
                DisplayDuration = song.DisplayDuration,
                Duration = song.Duration,
                Img = song.Img,
                Path = null,
                Type = PlaySongListViewItemEntryType.Network
            });
            playSongList.Add(new PlaySongListViewItemEntry
            {
                SongName = song.SongName,
                SongId = song.SongId,
                Authors = song.Authors,
                DisplayDuration = song.DisplayDuration,
                Duration = song.Duration,
                Img = song.Img,
                Path = null,
                Type = PlaySongListViewItemEntryType.Network
            });
            PlayListView.SelectedIndex = PlayListView.Items.Count - 1;
            File.WriteAllText(Const.PlayerDataPath, JsonConvert.SerializeObject(playSongList, Formatting.Indented));
            await Task.Delay(250);
            SearchSongListView.SelectedIndex = -1;
        };
        DelSelectedSong.Click += (s, e) =>
        {
            if (PlayListView.Items.Count == 0 || PlayListView.SelectedIndex == -1) return;
            playSongList.RemoveAt(PlayListView.SelectedIndex);
            PlayListView.Items.RemoveAt(PlayListView.SelectedIndex);
            File.WriteAllText(Const.PlayerDataPath, JsonConvert.SerializeObject(playSongList, Formatting.Indented));
            PlayListView.SelectedIndex = PlayListView.Items.Count - 1;
        };
        var _theLastLocalSong = string.Empty;
        AddLocalSong.Click += async (s, e) =>
        {
            var files = await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this)!,
                new FilePickerOpenOptions { AllowMultiple = true, Title = MainLang.SelectMusicFile });
            if (files == null) return;
            foreach (var file in files)
            {
                if (_theLastLocalSong == file.Path) continue;
                _theLastLocalSong = file.Path;
                using (var reader = new MediaFoundationReader(file.Path))
                {
                    var time = Method.Value.MsToTime(reader.TotalTime.TotalMilliseconds);
                    var song = new PlaySongListViewItemEntry
                    {
                        DisplayDuration = time,
                        Duration = reader.TotalTime.TotalMilliseconds,
                        Img = null,
                        SongName = file.Name,
                        Authors = file.Extension.TrimStart('.'),
                        Path = file.Path,
                        Type = PlaySongListViewItemEntryType.Local
                    };
                    playSongList.Add(song);
                    PlayListView.Items.Add(song);
                }
            }

            File.WriteAllText(Const.PlayerDataPath, JsonConvert.SerializeObject(playSongList, Formatting.Indented));
            PlayListView.SelectedIndex = PlayListView.Items.Count - 1;
        };
        PlayBtn.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (_isPlaying)
                    PausePlaying();
                else
                    BeginPlaying();
            }
        };
        PlayListView.SelectionChanged += (s, e) =>
        {
            var song = (PlaySongListViewItemEntry)PlayListView.SelectedItem;
            _selectedItem = song;
            if (song.Type == PlaySongListViewItemEntryType.Local) SongImg.Source = Img.Source;
            PlaySong(song!);
        };
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        var slider = true;
        if (_selectedItem == null) return;
        try
        {
            if (_selectedItem.Type == PlaySongListViewItemEntryType.Local)
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (_waveSource == null) return;
                    CurrentTimeText.Text = Method.Value.MsToTime(_waveSource.CurrentTime.TotalMilliseconds);
                    if (slider) PlayerSlider.Value = _waveSource.CurrentTime.TotalMilliseconds;
                });
            else if (_selectedItem.Type == PlaySongListViewItemEntryType.Network)
                Dispatcher.UIThread.Invoke(() =>
                {
                    if (_waveStream == null) return;
                    CurrentTimeText.Text = Method.Value.MsToTime(_waveStream.CurrentTime.TotalMilliseconds);
                    if (slider) PlayerSlider.Value = _waveStream.CurrentTime.TotalMilliseconds;
                });
        }
        catch
        {
        }
    }

    private void BeginPlaying()
    {
        _isPlaying = true;
        PauseIcon.IsVisible = false;
        PlayingIcon.IsVisible = true;
        try
        {
            _waveOut.Play();
        }
        catch
        {
        }
    }

    private void PausePlaying()
    {
        _isPlaying = false;
        PauseIcon.IsVisible = true;
        PlayingIcon.IsVisible = false;
        try
        {
            _waveOut.Pause();
        }
        catch
        {
        }
    }

    private async Task SearchForListViewAsync(string keyword, int page)
    {
        if (string.IsNullOrEmpty(keyword)) return;
        LoadMoreBtn.IsVisible = false;
        Loading.IsVisible = true;
        SearchBtn.IsEnabled = false;
        SearchBox.IsEnabled = false;
        SearchSongListView.Items.Clear();
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
        var json = string.Empty;
        try
        {
            var url = $"{Const.MusicApiUrl}/cloudsearch?keywords={keyword}&offset={page * 30}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            SearchBox.IsEnabled = true;
            SearchBtn.IsEnabled = true;
            Method.Ui.ShowShortException(MainLang.ErrorCallingApi, ex);
            Loading.IsVisible = false;
            return;
        }

        var obj = JsonConvert.DeserializeObject<SearchSongEntry.Root>(json);
        if (obj.code == 200)
        {
            if (obj.result.songCount > 0)
            {
                var songs = obj.result.songs.ToArray();
                foreach (var song in songs)
                {
                    var authors = string.Empty;
                    foreach (var author in song.ar) authors += $"{author.name} ";
                    SearchSongListView.Items.Add(new SearchSongListViewItemEntry
                    {
                        SongId = song.id,
                        SongName = song.name,
                        Authors = authors,
                        Img = song.al.picUrl,
                        Duration = song.dt,
                        DisplayDuration = Method.Value.MsToTime(Convert.ToInt32(song.dt))
                    });
                }
            }
            else
            {
                Method.Ui.Toast(MainLang.SearchNoResult);
            }
        }
        else
        {
            Method.Ui.Toast(MainLang.SearchNoResult, type: NotificationType.Error);
        }

        if (SearchSongListView.Items.Count > 0) LoadMoreBtn.IsVisible = true;
        SearchBox.IsEnabled = true;
        SearchBtn.IsEnabled = true;
        Loading.IsVisible = false;
    }

    private async Task LoadMoreAsync()
    {
        Loading.IsVisible = true;
        LoadMoreBtn.IsVisible = false;
        SongListViewScroll.ScrollToEnd();
        page++;
        SearchBox.IsEnabled = false;
        SearchBtn.IsEnabled = false;
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
        var json = string.Empty;
        try
        {
            var url = $"{Const.MusicApiUrl}/cloudsearch?keywords={keyword}&offset={page * 30}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            SearchBox.IsEnabled = true;
            SearchBtn.IsEnabled = true;
            Loading.IsVisible = false;
            LoadMoreBtn.IsVisible = true;
            Method.Ui.ShowShortException(MainLang.ErrorCallingApi, ex);
            return;
        }

        var obj = JsonConvert.DeserializeObject<SearchSongEntry.Root>(json);
        if (obj.code == 200)
        {
            if (obj.result.songCount > 0)
            {
                var songs = obj.result.songs.ToArray();
                foreach (var song in songs)
                {
                    var authors = string.Empty;
                    foreach (var author in song.ar) authors += $"{author.name} ";
                    SearchSongListView.Items.Add(new SearchSongListViewItemEntry
                    {
                        SongId = song.id,
                        SongName = song.name,
                        Authors = authors,
                        Duration = song.dt,
                        Img = song.al.picUrl,
                        DisplayDuration = Method.Value.MsToTime(Convert.ToInt32(song.dt))
                    });
                }
            }
            else
            {
                Method.Ui.Toast(MainLang.SearchNoResult);
            }
        }
        else
        {
            Method.Ui.Toast(MainLang.SearchNoResult, type: NotificationType.Error);
        }

        if (SearchSongListView.Items.Count > 0) LoadMoreBtn.IsVisible = true;
        SearchBox.IsEnabled = true;
        SearchBtn.IsEnabled = true;
        Loading.IsVisible = false;
        LoadMoreBtn.IsVisible = true;
    }

    private async void PlaySong(PlaySongListViewItemEntry entry)
    {
        if (entry == null) return;
        SongName.Text = entry.SongName;
        SongAuthors.Text = entry.Authors;
        try
        {
            _waveOut.Stop();
            _waveOut.Dispose();
        }
        catch
        {
        }

        if (entry.Type == PlaySongListViewItemEntryType.Local)
        {
            if (entry.Path == null)
            {
                Method.Ui.Toast(MainLang.MusicGetFail, type: NotificationType.Error);
                return;
            }

            if (!File.Exists(entry.Path))
            {
                Method.Ui.Toast(MainLang.FileNotExist, type: NotificationType.Error);
                return;
            }

            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
            }

            _waveOut = new WaveOutEvent();
            _waveOut.Volume = (float)VolumeSlider.Value / 100;
            _waveOut.PlaybackStopped += PlayerEnded;
            _waveSource = new AudioFileReader(entry.Path);
            _waveOut.Init(_waveSource);
            PlayerSlider.Maximum = _waveSource.TotalTime.TotalMilliseconds;
            TotalTimeText.Text = Method.Value.MsToTime(_waveSource.TotalTime.TotalMilliseconds);
            BeginPlaying();
        }
        else
        {
            Const.Notification.main.Show(new Notification(
                $"Yu Minecraft Launcher - {DateTime.Now.ToString("HH:mm:ss")}", MainLang.Loading,
                expiration: TimeSpan.FromSeconds(0.9)));
            var data =
                BitmapDataList.Find(UrlImageDataListEntry => UrlImageDataListEntry.Url == entry.Img);
            if (data == null)
            {
                var bitmap = await Method.Value.LoadImageFromUrlAsync(entry.Img!);
                if (bitmap != null) SongImg.Source = bitmap;

                BitmapDataList.Add(new UrlImageDataListEntry { Url = entry.Img, Bitmap = bitmap });
            }
            else
            {
                SongImg.Source = data.Bitmap;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var response = await client.GetAsync($"{Const.MusicApiUrl}/check/music?id={entry.SongId}");
            response.EnsureSuccessStatusCode();
            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var availability = (bool)jObject["success"]!;
            if (!availability)
            {
                Method.Ui.Toast(MainLang.MusicNotAvailable, type: NotificationType.Error);
                return;
            }

            var response1 = await client.GetAsync($"{Const.MusicApiUrl}/song/url?id={entry.SongId}");
            response1.EnsureSuccessStatusCode();
            var jObject1 = JObject.Parse(await response1.Content.ReadAsStringAsync());
            if (jObject1 == null)
            {
                Method.Ui.Toast(MainLang.MusicGetFail, type: NotificationType.Error);
                return;
            }

            var url = (string)((JObject)((JArray)jObject1["data"])[0])["url"];

            var request = (HttpWebRequest)WebRequest.Create(url);
            using (var response4 = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response4.GetResponseStream())
            {
                var memoryStream = new MemoryStream();
                responseStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                _waveStream = new Mp3FileReader(memoryStream);

                var selectedItem = PlayListView.SelectedItem as PlaySongListViewItemEntry;
                if (selectedItem.SongName != entry.SongName || selectedItem.SongId != entry.SongId) return;

                PlayerSlider.Maximum = _waveStream.TotalTime.TotalMilliseconds;
                TotalTimeText.Text = Method.Value.MsToTime(_waveStream.TotalTime.TotalMilliseconds);
                if (_waveOut != null)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                }

                _waveOut = new WaveOutEvent();
                _waveOut.Volume = (float)VolumeSlider.Value / 100;
                _waveOut.PlaybackStopped += PlayerEnded;
                _waveOut.Init(_waveStream);
                BeginPlaying();
            }
        }
    }

    private void PlayerEnded(object? sender, StoppedEventArgs e)
    {
        if (Math.Abs(PlayerSlider.Maximum - PlayerSlider.Value) > 1200) return;
        var a = 0;
    }
}