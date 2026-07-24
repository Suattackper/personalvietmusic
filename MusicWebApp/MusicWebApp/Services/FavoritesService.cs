using MusicWebApp.Models;

namespace MusicWebApp.Services;

/// <summary>
/// Quản lý danh sách bài hát yêu thích, lưu trong localStorage. Không lưu trùng VideoId.
/// </summary>
public class FavoritesService
{
    private readonly ILocalStorageService _storage;
    private List<MusicTrack> _favorites = new();
    private bool _initialized;

    public FavoritesService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public IReadOnlyList<MusicTrack> Favorites => _favorites;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        var saved = await _storage.GetItemAsync<List<MusicTrack>>(StorageKeys.Favorites);
        if (saved is not null)
        {
            // Loại trùng VideoId phòng trường hợp dữ liệu cũ bị lỗi.
            _favorites = saved
                .Where(t => !string.IsNullOrEmpty(t.VideoId))
                .GroupBy(t => t.VideoId)
                .Select(g => g.First())
                .ToList();
            foreach (var track in _favorites)
            {
                track.IsFavorite = true;
                track.IsPlaying = false;
            }
        }

        _initialized = true;
        OnChange?.Invoke();
    }

    public bool IsFavorite(string videoId)
        => _favorites.Any(t => t.VideoId == videoId);

    /// <summary>Thêm/bỏ yêu thích. Trả về true nếu sau thao tác bài nằm trong favorites.</summary>
    public async Task<bool> ToggleFavoriteAsync(MusicTrack track)
    {
        var existing = _favorites.FirstOrDefault(t => t.VideoId == track.VideoId);
        bool added;
        if (existing is not null)
        {
            _favorites.Remove(existing);
            track.IsFavorite = false;
            added = false;
        }
        else
        {
            _favorites.Add(new MusicTrack
            {
                VideoId = track.VideoId,
                Title = track.Title,
                Artist = track.Artist,
                ThumbnailUrl = track.ThumbnailUrl,
                PublishedAt = track.PublishedAt,
                IsFavorite = true
            });
            track.IsFavorite = true;
            added = true;
        }

        await _storage.SetItemAsync(StorageKeys.Favorites, _favorites);
        OnChange?.Invoke();
        return added;
    }

    public async Task RemoveAsync(string videoId)
    {
        var existing = _favorites.FirstOrDefault(t => t.VideoId == videoId);
        if (existing is null)
        {
            return;
        }

        _favorites.Remove(existing);
        await _storage.SetItemAsync(StorageKeys.Favorites, _favorites);
        OnChange?.Invoke();
    }
}
