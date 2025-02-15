using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Newtonsoft.Json;
using YMCL.Public.Classes.Netease;
using YMCL.Public.Langs;

namespace YMCL.Public.Module.IO.Network;

public class NeteaseMusic
{
    public static string realIP => Data.Setting.MusicApiWithIPAddress;
    public static async Task<List<SearchSingleSong.SongsItem>> SearchForSingleSong(string keyword, int page = 1)
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Data.Setting.MusicApi)}cloudsearch?keywords={keyword}&offset={(page - 1) * 30}&realIP={realIP}");
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }
        var entry = JsonConvert.DeserializeObject<SearchSingleSong.Root>(json);
        if (entry is { code: 200 }) return entry.result.songCount == 0 ? [] : entry.result.songs;
        Notice(MainLang.ApiError, NotificationType.Error);
        return [];
    }

    public static async Task<(string show, string real)> GetDefaultKeyword()
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}search/default?realIP={realIP}");
        if (string.IsNullOrWhiteSpace(json))
        {
            return (string.Empty, string.Empty);
        }
        var entry = JsonConvert.DeserializeObject<DefaultKeyword.Root>(json);
        if (entry is { code: 200 }) return (entry.data.showKeyword, entry.data.realkeyword);
        Notice(MainLang.ApiError, NotificationType.Error);
        return (string.Empty, string.Empty);
    }

    public static async Task<bool> GetMusicAvailabilityById(double id)
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}check/music?id={id}&realIP={realIP}");
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }
        var entry = JsonConvert.DeserializeObject<Availability.Root>(json);
        if (entry is { code: 200 }) return entry.success;
        Notice(MainLang.ApiError, NotificationType.Error);
        return false;
    }

    public static async Task<(string url, double ms)> GetSongUrlByIdAndLevel(double id, string level = "standard")
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}song/url/v1?id={id}&level={level}&realIP={realIP}");
        if (string.IsNullOrWhiteSpace(json))
        {
            return (string.Empty, 0);
        }
        var entry = JsonConvert.DeserializeObject<SongUrl.Root>(json);
        if (entry is { code: 200 }) return (entry.data[0].url, entry.data[0].time);
        Notice(MainLang.ApiError, NotificationType.Error);
        return (string.Empty, 0);
    }
    
    public static async Task<Lyric.Root?> GetSongLyricById(double id)
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}lyric?id={id}&realIP={realIP}");
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }
        var entry = JsonConvert.DeserializeObject<Lyric.Root>(json);
        if (entry is { code: 200 }) return entry;
        Notice(MainLang.ApiError, NotificationType.Error);
        return null;
    }
}