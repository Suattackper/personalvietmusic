using System.Net.Http.Json;
using MusicWebApp.Models;

namespace MusicWebApp.Services;

/// <summary>
/// Nạp danh sách playlist có sẵn từ wwwroot/data/playlists.json (file tĩnh cùng origin).
/// Chỉ tải một lần rồi giữ trong bộ nhớ — hoàn toàn không đụng tới YouTube API.
/// </summary>
public class PlaylistService
{
    private const string CatalogVersion = "2026-11-06-tiktok-100";
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlaylistService> _logger;
    private List<CuratedPlaylist>? _playlists;

    public PlaylistService(HttpClient httpClient, ILogger<PlaylistService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CuratedPlaylist>> GetPlaylistsAsync()
    {
        if (_playlists is not null)
        {
            return _playlists;
        }

        try
        {
            _playlists = await _httpClient.GetFromJsonAsync<List<CuratedPlaylist>>($"data/playlists.json?v={CatalogVersion}")
                ?? new List<CuratedPlaylist>();
            foreach (var language in new[] { "vi", "en", "ja", "zh" })
            {
                await AddCollectionAsync($"data/music/{language}.json");
                await AddCollectionAsync($"data/music/{language}-recent.json");
                await AddCollectionAsync($"data/music/{language}-tiktok.json");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not load featured playlists");
            _playlists = new List<CuratedPlaylist>();
        }

        return _playlists;
    }

    private async Task AddCollectionAsync(string path)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                var version = attempt == 0 ? CatalogVersion : $"{CatalogVersion}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                var collection = await _httpClient.GetFromJsonAsync<CuratedPlaylist>($"{path}?v={version}");
                if (collection is null || collection.Tracks.Count == 0)
                {
                    throw new InvalidDataException($"Playlist {path} is empty");
                }
                _playlists!.Add(collection);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load {PlaylistPath} (attempt {Attempt})", path, attempt + 1);
            }
        }
    }

    public async Task<CuratedPlaylist?> GetByIdAsync(string id)
    {
        var playlists = await GetPlaylistsAsync();
        return playlists.FirstOrDefault(p => p.Id == id);
    }
}
