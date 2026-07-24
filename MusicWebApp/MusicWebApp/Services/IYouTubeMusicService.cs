using MusicWebApp.Models;

namespace MusicWebApp.Services;

public interface IYouTubeMusicService
{
    Task<YouTubeSearchResult> SearchAsync(
        string keyword,
        string? pageToken = null,
        CancellationToken cancellationToken = default);
}
