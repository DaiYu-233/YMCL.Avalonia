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
    public static async Task<List<SearchSingleSong.SongsItem>> SearchForSingleSong(string keyword, int page = 1)
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Data.Setting.MusicApi)}cloudsearch?keywords={keyword}&offset={(page - 1) * 30}&realIP=116.25.146.177");
        var entry = JsonConvert.DeserializeObject<SearchSingleSong.Root>(json);
        if (entry is { code: 200 }) return entry.result.songCount == 0 ? [] : entry.result.songs;
        Toast(MainLang.ApiError, NotificationType.Error);
        return [];
    }

    public static async Task<(string show, string real)> GetDefaultKeyword()
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}search/default?realIP=116.25.146.177");
        var entry = JsonConvert.DeserializeObject<DefaultKeyword.Root>(json);
        if (entry is { code: 200 }) return (entry.data.showKeyword, entry.data.realkeyword);
        Toast(MainLang.ApiError, NotificationType.Error);
        return (string.Empty, string.Empty);
    }

    public static async Task<bool> GetMusicAvailabilityById(double id)
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}check/music?id={id}&realIP=116.25.146.177");
        var entry = JsonConvert.DeserializeObject<Availability.Root>(json);
        if (entry is { code: 200 }) return entry.success;
        Toast(MainLang.ApiError, NotificationType.Error);
        return false;
    }

    public static async Task<(string url, double ms)> GetSongUrlByIdAndLevel(double id, string level = "standard")
    {
        var json = await Http.Get.GetStringAsync(
            $"{Value.Converter.StandardizeUrl(Const.Data.Setting.MusicApi)}song/url/v1?id={id}&level={level}&realIP=116.25.146.177");
        var entry = JsonConvert.DeserializeObject<SongUrl.Root>(json);
        if (entry is { code: 200 }) return (entry.data[0].url, entry.data[0].time);
        Toast(MainLang.ApiError, NotificationType.Error);
        return (string.Empty, 0);
    }
}