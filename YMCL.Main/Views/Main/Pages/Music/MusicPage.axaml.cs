using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
//using LibVLCSharp.Avalonia;
//using LibVLCSharp.Shared;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using YMCL.Main.Public;
using YMCL.Main.Public.Classes;
using YMCL.Main.Public.Langs;
using static YMCL.Main.Public.Classes.PlaySongListViewItemEntry;

namespace YMCL.Main.Views.Main.Pages.Music
{
    public partial class MusicPage : UserControl
    {
        public List<PlaySongListViewItemEntry> playSongList = new();
        string keyword = string.Empty;
        private bool _isPlaying = false;
        bool _firstLoad = true;
        int page = 0;
        public MusicPage()
        {
            InitializeComponent();
            BindingEvent();
        }
        private void BindingEvent()
        {
            Loaded += (s, e) =>
            {
                Method.Ui.PageLoadAnimation((-50, 0, 50, 0), (0, 0, 0, 0), TimeSpan.FromSeconds(0.45), Root, true);
                if (_firstLoad)
                {
                    _firstLoad = false;
                    Method.Ui.Toast(MainLang.ThisFeatureIsCurrentlyUnderDevelopment);
                    try
                    {
                        //_libVLC = new LibVLC();
                        //_mediaPlayer = new MediaPlayer(_libVLC);
                        PlayerEvent();
                    }
                    catch (Exception ex)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            _ = Method.Ui.ShowDialogAsync(MainLang.InitAudioLibraryFail, MainLang.InstallVlcTip, b_primary: MainLang.Ok);
                        }
                        else
                        {
                            Method.Ui.ShowLongException(MainLang.InitAudioLibraryFail, ex);
                        }
                    }
                    var list = JsonConvert.DeserializeObject<List<PlaySongListViewItemEntry>>(File.ReadAllText(Const.PlayerDataPath));
                    list.ForEach(list => { playSongList.Add(list); PlayListView.Items.Add(list); });
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
                if (e.Key == Avalonia.Input.Key.Enter)
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
            LoadMoreBtn.Click += (s, e) =>
            {
                _ = LoadMoreAsync();
            };
            SearchSongListView.SelectionChanged += async (_, _) =>
            {
                if (SearchSongListView.SelectedIndex == -1) return;
                var song = SearchSongListView.SelectedItem as SearchSongListViewItemEntry;
                if (song == null) return;
                PlayListView.Items.Add(new PlaySongListViewItemEntry()
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
                playSongList.Add(new PlaySongListViewItemEntry()
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
            string _theLastLocalSong = string.Empty;
            AddLocalSong.Click += async (s, e) =>
            {
                var files = await Method.IO.OpenFilePicker(TopLevel.GetTopLevel(this)!, new FilePickerOpenOptions() { AllowMultiple = true, Title = MainLang.SelectMusicFile });
                if (files == null) return;
                foreach (var file in files)
                {
                    if (_theLastLocalSong == file.Path) continue;
                    _theLastLocalSong = file.Path;
                    using (var reader = new MediaFoundationReader(file.Path))
                    {
                        var time = Method.Value.MsToTime(reader.TotalTime.TotalMilliseconds);
                        var song = new PlaySongListViewItemEntry()
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
                    {
                        PausePlaying();
                    }
                    else
                    {
                        BeginPlaying();
                    }
                }
            };
            PlayListView.SelectionChanged += (s, e) =>
            {
                var song = (PlaySongListViewItemEntry)PlayListView.SelectedItem;
                PlaySong(song!);
            };
        }
        private void PlayerEvent()
        {
            //_mediaPlayer.EndReached += (s, e) =>
            //{
            //    // MP3播放完成后的处理  
            //};

            //_mediaPlayer.MediaChanged += (s, e) =>
            //{
            //    // MP3加载后的处理  
            //    _mediaPlayer.Play();
            //};
        }
        void BeginPlaying()
        {
            _isPlaying = true;
            PauseIcon.IsVisible = false;
            PlayingIcon.IsVisible = true;
            //_mediaPlayer.Play();
        }
        void PausePlaying()
        {
            _isPlaying = false;
            PauseIcon.IsVisible = true;
            PlayingIcon.IsVisible = false;
            //_mediaPlayer.Pause();
        }
        async Task SearchForListViewAsync(string keyword, int page)
        {
            if (string.IsNullOrEmpty(keyword)) return;
            LoadMoreBtn.IsVisible = false;
            Loading.IsVisible = true;
            SearchBtn.IsEnabled = false;
            SearchBox.IsEnabled = false;
            SearchSongListView.Items.Clear();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var json = string.Empty;
            try
            {
                var url = $"http://music.api.daiyu.fun/cloudsearch?keywords={keyword}&offset={page * 30}";
                HttpResponseMessage response = await client.GetAsync(url);
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
                        foreach (var author in song.ar)
                        {
                            authors += $"{author.name} ";
                        }
                        SearchSongListView.Items.Add(new SearchSongListViewItemEntry()
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
                Method.Ui.Toast(MainLang.SearchNoResult, type: Avalonia.Controls.Notifications.NotificationType.Error);
            }
            if (SearchSongListView.Items.Count > 0)
            {
                LoadMoreBtn.IsVisible = true;
            }
            SearchBox.IsEnabled = true;
            SearchBtn.IsEnabled = true;
            Loading.IsVisible = false;
        }
        async Task LoadMoreAsync()
        {
            Loading.IsVisible = true;
            LoadMoreBtn.IsVisible = false;
            SongListViewScroll.ScrollToEnd();
            page++;
            SearchBox.IsEnabled = false;
            SearchBtn.IsEnabled = false;
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54");
            var json = string.Empty;
            try
            {
                var url = $"http://music.api.daiyu.fun/cloudsearch?keywords={keyword}&offset={page * 30}";
                HttpResponseMessage response = await client.GetAsync(url);
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
                        foreach (var author in song.ar)
                        {
                            authors += $"{author.name} ";
                        }
                        SearchSongListView.Items.Add(new SearchSongListViewItemEntry()
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
                Method.Ui.Toast(MainLang.SearchNoResult, type: Avalonia.Controls.Notifications.NotificationType.Error);
            }
            if (SearchSongListView.Items.Count > 0)
            {
                LoadMoreBtn.IsVisible = true;
            }
            SearchBox.IsEnabled = true;
            SearchBtn.IsEnabled = true;
            Loading.IsVisible = false;
            LoadMoreBtn.IsVisible = true;
        }
        async void PlaySong(PlaySongListViewItemEntry entry)
        {
            if (entry == null) return;
            SongName.Text = entry.SongName;
            SongAuthors.Text = entry.Authors;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] imageData = await client.GetByteArrayAsync(entry.Img);
                    using (var stream = new MemoryStream(imageData))
                    {
                        var bitmap = new Bitmap(stream);
                        SongImg.Source = bitmap;
                    }
                }
            }
            catch { }
            //_mediaPlayer.Media = new Media(_libVLC, entry.Path!, FromType.FromPath);
            BeginPlaying();
        }
    }
}
