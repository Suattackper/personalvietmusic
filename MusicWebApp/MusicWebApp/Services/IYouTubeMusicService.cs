using MusicWebApp.Models;

namespace MusicWebApp.Services;

public interface IYouTubeMusicService
{
    Task<YouTubeSearchResult> SearchAsync(
        string keyword,
        SearchFilters filters,
        string? pageToken = null,
        CancellationToken cancellationToken = default);
}
