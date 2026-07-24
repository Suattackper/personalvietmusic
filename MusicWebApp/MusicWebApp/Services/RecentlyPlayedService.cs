using MusicWebApp.Models;

namespace MusicWebApp.Services;

/// <summary>
/// Lịch sử nghe gần đây: tối đa 50 bài, không lưu trùng liên tiếp cùng một bài,
/// lưu trong localStorage.
/// </summary>
public class RecentlyPlayedService
{
    private const int MaxItems = 50;

    private readonly ILocalStorageService _storage;
    private List<MusicTrack> _history = new();
    private bool _initialized;

    public RecentlyPlayedService(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public IReadOnlyList<MusicTrack> History => _history;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        var saved = await _storage.GetItemAsync<List<MusicTrack>>(StorageKeys.RecentlyPlayed);
        if (saved is not null)
        {
            _history = saved
                .Where(t => !string.IsNullOrEmpty(t.VideoId))
                .Take(MaxItems)
                .ToList();
            foreach (var track in _history)
            {
                track.IsPlaying = false;
            }
        }

        _initialized = true;
        OnChange?.Invoke();
    }

    public async Task AddAsync(MusicTrack track)
    {
        // Không lưu trùng liên tiếp: bài mới nhất trong lịch sử trùng với bài vừa phát thì bỏ qua.
        if (_history.Count > 0 && _history[0].VideoId == track.VideoId)
        {
            return;
        }

        _history.Insert(0, new MusicTrack
        {
            VideoId = track.VideoId,
            Title = track.Title,
            Artist = track.Artist,
            ThumbnailUrl = track.ThumbnailUrl,
            PublishedAt = track.PublishedAt
        });

        if (_history.Count > MaxItems)
        {
            _history.RemoveRange(MaxItems, _history.Count - MaxItems);
        }

        await _storage.SetItemAsync(StorageKeys.RecentlyPlayed, _history);
        OnChange?.Invoke();
    }

    public async Task ClearAsync()
    {
        _history.Clear();
        await _storage.RemoveItemAsync(StorageKeys.RecentlyPlayed);
        OnChange?.Invoke();
    }
}
