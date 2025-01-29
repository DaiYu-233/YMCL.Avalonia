using Avalonia.Controls.Notifications;
using YMCL.Public.Classes;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.IO.Network;
using YMCL.ViewModels;

namespace YMCL.Views.Main.Pages;

public partial class Music : UserControl
{
    public readonly MusicPages.PlayList Playlist = new();
    public readonly MusicPages.Search Search = new();
    private bool _isPlaying = false;

    public Music()
    {
        InitializeComponent();
        RightControl.Content = Playlist;
        LeftControl.Content = Search;
        DataContext = Data.Instance;
        BindingEvent();
    }

    private void BindingEvent()
    {
        Data.UiProperty.PropertyChanged += async (o, e) =>
        {
            var song = Data.UiProperty.SelectedRecordSong;
            if (e.PropertyName != nameof(UiProperty.SelectedRecordSong) ||
                song == null) return;
            PlayUi();
            if (song.Type == SongType.Local)
            {
                AudioPlayer.Instance.PlayLocal(song.SongPicUri);
            }
            else if (song.Type == SongType.Netease)
            {
                var ava = await NeteaseMusic.GetMusicAvailabilityById(song.SongId);
                if (!ava)
                {
                    Toast(MainLang.MusicNotAvailable, NotificationType.Error);
                    return;
                }

                var url = await NeteaseMusic.GetSongUrlByIdAndLevel(song.SongId);
                Data.UiProperty.MusicTotalTime = url.ms;
                _ = AudioPlayer.Instance.PlayNetwork(url.url);
            }
        };
        PlayBtn.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            if (_isPlaying)
                PauseUi();
            else
                PlayUi();
        };
        AudioPlayer.Instance.ProgressChanged += (_, e) =>
        {
            Data.UiProperty.MusicCurrentTime = e.CurrentTime;
        };
    }

    public void PlayUi()
    {
        _isPlaying = true;
        PauseIcon.IsVisible = false;
        PlayingIcon.IsVisible = true;
    }

    public void PauseUi()
    {
        _isPlaying = false;
        PauseIcon.IsVisible = true;
        PlayingIcon.IsVisible = false;
    }
}