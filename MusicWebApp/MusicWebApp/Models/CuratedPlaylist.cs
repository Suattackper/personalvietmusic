using System.Text.Json.Serialization;

namespace MusicWebApp.Models;

/// <summary>
/// Playlist có sẵn, nạp từ wwwroot/data/playlists.json (dữ liệu tĩnh, set cứng videoId —
/// không gọi YouTube API nên không tốn quota khi phát).
/// </summary>
public class CuratedPlaylist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = "🎵";
    public string Description { get; set; } = string.Empty;
    public List<PlaylistTrack> Tracks { get; set; } = new();
}

public class PlaylistTrack
{
    [JsonPropertyName("v")]
    public string VideoId { get; set; } = string.Empty;

    [JsonPropertyName("t")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("a")]
    public string Artist { get; set; } = string.Empty;

    [JsonPropertyName("d")]
    public int? DurationSeconds { get; set; }

    public MusicTrack ToMusicTrack() => new()
    {
        VideoId = VideoId,
        Title = Title,
        Artist = Artist,
        DurationSeconds = DurationSeconds,
        // Thumbnail suy ra từ videoId — không cần gọi API.
        ThumbnailUrl = $"https://i.ytimg.com/vi/{VideoId}/hqdefault.jpg"
    };
}
