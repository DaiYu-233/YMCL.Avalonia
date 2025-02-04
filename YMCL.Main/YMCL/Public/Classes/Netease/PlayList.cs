using System.Collections.Generic;

namespace YMCL.Public.Classes.Netease;

public class PlayList
{
    public class Creator
    {
        public string nickname { get; set; }
        public long userId { get; set; }
        public int userType { get; set; }
        public object avatarUrl { get; set; }
        public int authStatus { get; set; }
        public object expertTags { get; set; }
        public object experts { get; set; }
    }

    public class Playlist
    {
        public long id { get; set; }
        public string name { get; set; }
        public string coverImgUrl { get; set; }
        public Creator creator { get; set; }
        public bool subscribed { get; set; }
        public int trackCount { get; set; }
        public long userId { get; set; }
        public long playCount { get; set; }
        public long bookCount { get; set; }
        public int specialType { get; set; }
        public object officialTags { get; set; }
        public object action { get; set; }
        public object actionType { get; set; }
        public object recommendText { get; set; }
        public object score { get; set; }
        public string description { get; set; }
        public bool highQuality { get; set; }
    }

    public class Result
    {
        public object searchQcReminder { get; set; }
        public List<Playlist> playlists { get; set; }
        public int playlistCount { get; set; }
    }

    public class Root
    {
        public Result result { get; set; }
        public int code { get; set; }
    }

}