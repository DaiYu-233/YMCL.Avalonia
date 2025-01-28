using System.Collections.Generic;

namespace YMCL.Public.Classes;

public class MojangJavaNews()
{
    public class Image
    {
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string url { get; set; }
    }

    public class EntriesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Image image { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string contentPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string shortText { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<EntriesItem> entries { get; set; }
    }
}