namespace MusicWebApp.Models;

/// <summary>
/// Các model deserialize JSON trả về từ YouTube Data API v3 (endpoint search).
/// </summary>
public class YouTubeSearchResponse
{
    public List<YouTubeSearchItem> Items { get; set; } = new();
    public string? NextPageToken { get; set; }
    public string? PrevPageToken { get; set; }
    public YouTubePageInfo? PageInfo { get; set; }
}

public class YouTubePageInfo
{
    public int TotalResults { get; set; }
    public int ResultsPerPage { get; set; }
}

public class YouTubeSearchItem
{
    public YouTubeVideoId? Id { get; set; }
    public YouTubeSnippet? Snippet { get; set; }
}

public class YouTubeVideoId
{
    public string? Kind { get; set; }
    public string? VideoId { get; set; }
}

public class YouTubeSnippet
{
    public string? Title { get; set; }
    public string? ChannelTitle { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedAt { get; set; }
    public YouTubeThumbnailCollection? Thumbnails { get; set; }
}

public class YouTubeThumbnailCollection
{
    public YouTubeThumbnail? Default { get; set; }
    public YouTubeThumbnail? Medium { get; set; }
    public YouTubeThumbnail? High { get; set; }
}

public class YouTubeThumbnail
{
    public string? Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>Response của endpoint videos (dùng lấy thời lượng, 1 quota unit/lần).</summary>
public class YouTubeVideosResponse
{
    public List<YouTubeVideoItem> Items { get; set; } = new();
}

public class YouTubeVideoItem
{
    public string? Id { get; set; }
    public YouTubeContentDetails? ContentDetails { get; set; }
}

public class YouTubeContentDetails
{
    public string? Duration { get; set; }
}
