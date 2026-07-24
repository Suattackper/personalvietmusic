using System.Net.Http.Json;
using MusicWebApp.Models;

namespace MusicWebApp.Services;

/// <summary>
/// Nạp danh sách playlist có sẵn từ wwwroot/data/playlists.json (file tĩnh cùng origin).
/// Chỉ tải một lần rồi giữ trong bộ nhớ — hoàn toàn không đụng tới YouTube API.
/// </summary>
public class PlaylistService
{
    private readonly HttpClient _httpClient;
    private List<CuratedPlaylist>? _playlists;

    public PlaylistService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CuratedPlaylist>> GetPlaylistsAsync()
    {
        if (_playlists is not null)
        {
            return _playlists;
        }

        try
        {
            _playlists = await _httpClient.GetFromJsonAsync<List<CuratedPlaylist>>("data/playlists.json")
                ?? new List<CuratedPlaylist>();
        }
        catch (Exception)
        {
            // File thiếu hoặc JSON hỏng — trả danh sách rỗng thay vì crash.
            _playlists = new List<CuratedPlaylist>();
        }

        return _playlists;
    }

    public async Task<CuratedPlaylist?> GetByIdAsync(string id)
    {
        var playlists = await GetPlaylistsAsync();
        return playlists.FirstOrDefault(p => p.Id == id);
    }
}
