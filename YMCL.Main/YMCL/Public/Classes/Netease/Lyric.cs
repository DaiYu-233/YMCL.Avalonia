using System.Collections.Generic;

namespace YMCL.Public.Classes.Netease;

public class Lyric
{
    public class LyricInfo
    {
        public int t { get; set; }
        public List<LyricPart> c { get; set; }
    }

    public class LyricPart
    {
        public string tx { get; set; }
        public string li { get; set; }
        public string or { get; set; }
    }

    public class LyricItem
    {
        public int version { get; set; }
        public string lyric { get; set; }
    }

    public class Root
    {
        public bool sgc { get; set; }
        public bool sfy { get; set; }
        public bool qfy { get; set; }
        public User transUser { get; set; }
        public User lyricUser { get; set; }
        public LyricItem lrc { get; set; }
        public LyricItem klyric { get; set; }
        public LyricItem tlyric { get; set; }
        public LyricItem romalrc { get; set; }
        public LyricItem yrc { get; set; }
        public LyricItem ytlrc { get; set; }
        public LyricItem yromalrc { get; set; }
        public int code { get; set; }
    }

    public class User
    {
        public int id { get; set; }
        public int status { get; set; }
        public int demand { get; set; }
        public int userid { get; set; }
        public string nickname { get; set; }
        public long uptime { get; set; }
    }
}