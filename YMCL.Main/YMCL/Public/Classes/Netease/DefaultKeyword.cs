namespace YMCL.Public.Classes.Netease;

public class DefaultKeyword
{
    public class StyleKeyword
    {
        public string keyWord { get; set; }
        public object descWord { get; set; }
    }

    public class Data
    {
        public string showKeyword { get; set; }
        public StyleKeyword styleKeyword { get; set; }
        public string realkeyword { get; set; }
        public int searchType { get; set; }
        public int action { get; set; }
        public string alg { get; set; }
        public int gap { get; set; }
        public object source { get; set; }
        public string bizQueryInfo { get; set; }
        public object logInfo { get; set; }
        public object imageUrl { get; set; }
        public object trp_type { get; set; }
        public object trp_id { get; set; }
    }

    public class Root
    {
        public int code { get; set; }
        public object message { get; set; }
        public Data data { get; set; }
    }
}