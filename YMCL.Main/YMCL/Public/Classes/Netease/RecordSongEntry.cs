namespace YMCL.Public.Classes.Netease;

public class RecordSongEntry
{
    public Enum.SongType Type { get; set; } = Enum.SongType.Unknown;
    public string SongPicUri { get; set; } = string.Empty;
    public string DisplayNumber { get; set; }=string.Empty;
    public uint Number { get; set; } = 0;
    public string SongName { get; set; } = string.Empty;
    public double SongId { get; set; } = -1;
    public string SongAuthors { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string DisplayDuration { get; set; } = string.Empty;
    public double AlbumId { get; set; } = -1;
    public string AlbumName { get; set; } = string.Empty;
}