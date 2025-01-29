using System.Collections.Generic;

namespace YMCL.Public.Classes.Netease;

public class SongUrl
{
    public class DataItem
    {
        public string url { get; set; }
        public double time { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public List<DataItem> data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int code { get; set; }
    }
}