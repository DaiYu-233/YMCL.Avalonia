namespace YMCL.Public.Classes;

public class Player
{
    public class PlaySongListViewItemEntry
    {
        public enum PlaySongListViewItemEntryType
        {
            Local,
            Network
        }

        public string? SongName { get; set; }
        public string? DisplayDuration { get; set; }
        public double Duration { get; set; }
        public PlaySongListViewItemEntryType Type { get; set; }
        public string? Authors { get; set; } = "Null";
        public string? Path { get; set; }
        public double SongId { get; set; }
        public string? Img { get; set; } = string.Empty;
    }

}