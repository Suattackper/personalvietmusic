using System.Text.Json.Serialization;

namespace MusicWebApp.Models;

/// <summary>
/// Model dùng trong UI đại diện cho một bài hát.
/// </summary>
public class MusicTrack
{
    public string VideoId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPlaying { get; set; }

    /// <summary>Thời lượng dạng "m:ss" hoặc "h:mm:ss"; null nếu chưa biết.</summary>
    [JsonIgnore]
    public string? DurationText =>
        DurationSeconds is > 0 ? FormatDuration(DurationSeconds.Value) : null;

    public static string FormatDuration(int seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{time.Minutes}:{time.Seconds:00}";
    }
}
