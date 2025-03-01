using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using NAudio.Wave;
using Newtonsoft.Json;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Enum;
using YMCL.Public.Langs;
using YMCL.Public.Module;
using YMCL.Public.Module.Value;

namespace YMCL.Views.Main.Pages.MusicPages;

public partial class PlayList : UserControl
{
    public PlayList()
    {
        InitializeComponent();
        BindingEvent();
        LoadPlayList();
    }

    private static void LoadPlayList()
    {
        var songs = JsonConvert.DeserializeObject<List<RecordSongEntry>>(File.ReadAllText(ConfigPath.PlayerDataPath));
        songs.ForEach(song => Data.RecordSongEntries.Add(song));
    }

    private void BindingEvent()
    {
        UpSong.Click += (_, _) =>
        {
            var index = PlayListView.SelectedIndex;
            if (index == 0) return;
            var item = Data.UiProperty.SelectedRecordSong;
            if (item == null) return;
            Data.RecordSongEntries.Remove(item);
            Data.RecordSongEntries.Insert(index - 1, item);
            Data.UiProperty.SelectedRecordSong = item;
            File.WriteAllText(ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(Data.RecordSongEntries, Formatting.Indented));
        };
        DownSong.Click += (_, _) =>
        {
            var index = PlayListView.SelectedIndex;
            var count = Data.RecordSongEntries.Count;
            if (index == count - 1) return;
            var item = Data.UiProperty.SelectedRecordSong;
            if (item == null) return;
            Data.RecordSongEntries.Remove(item);
            Data.RecordSongEntries.Insert(index + 1, item);
            Data.UiProperty.SelectedRecordSong = item;
            File.WriteAllText(ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(Data.RecordSongEntries, Formatting.Indented));
        };
        DelSelectedSong.Click += (_, _) =>
        {
            if (Data.RecordSongEntries.Count == 0 || Data.UiProperty.SelectedRecordSong == null) return;
            Data.RecordSongEntries.RemoveAt(PlayListView.SelectedIndex);
            File.WriteAllText(ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(Data.RecordSongEntries, Formatting.Indented));
            Data.UiProperty.SelectedRecordSong = Data.RecordSongEntries.LastOrDefault();
        };
        AddLocalSong.Click += async (_, _) =>
        {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = true, Title = MainLang.SelectMusicFile, FileTypeFilter =
                    [
                        new FilePickerFileType("All Audio Files")
                        {
                            Patterns =
                            [
                                "*.mp3",
                                "*.wav",
                                "*.aac",
                                "*.flac",
                                "*.ogg",
                                "*.alac",
                                "*.m4a",
                                "*.wma",
                                "*.aiff",
                                "*.mid"
                            ]
                        }
                    ]
                });
            if (files.Count == 0) return;
            foreach (var file in files)
            {
                await using var reader = new MediaFoundationReader(file.Path.LocalPath);
                var time = Converter.MsToTime(reader.TotalTime.TotalMilliseconds);
                var song = new RecordSongEntry()
                {
                    DisplayDuration = time,
                    Duration = reader.TotalTime,
                    SongPicUri = null,
                    SongName = file.Name,
                    SongAuthors = Path.GetExtension(file.Path.LocalPath).TrimStart('.'),
                    Path = file.Path.LocalPath,
                    Type = SongType.Local
                };
                Data.RecordSongEntries.Add(song);
            }

            await File.WriteAllTextAsync(ConfigPath.PlayerDataPath,
                JsonConvert.SerializeObject(Data.RecordSongEntries, Formatting.Indented));
            Data.UiProperty.SelectedRecordSong = Data.RecordSongEntries.LastOrDefault();
        };
    }

    public void NextSong()
    {
        if (Data.RecordSongEntries.Count <= 1) return;
        Data.UiProperty.SelectedRecordSong = PlayListView.SelectedIndex == Data.RecordSongEntries.Count - 1
            ? Data.RecordSongEntries.First()
            : Data.RecordSongEntries[PlayListView.SelectedIndex + 1];
    }

    public void PreviousSong()
    {
        if (Data.RecordSongEntries.Count <= 1) return;
        Data.UiProperty.SelectedRecordSong = PlayListView.SelectedIndex == 0
            ? Data.RecordSongEntries.LastOrDefault()
            : Data.RecordSongEntries[PlayListView.SelectedIndex - 1];
    }
}