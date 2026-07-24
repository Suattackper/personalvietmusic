namespace MusicWebApp.Models;

/// <summary>
/// Kết quả tìm kiếm đã được map sang model UI.
/// </summary>
public class YouTubeSearchResult
{
    public List<MusicTrack> Tracks { get; set; } = new();
    public string? NextPageToken { get; set; }
    public string? PrevPageToken { get; set; }
    public int TotalResults { get; set; }
}
