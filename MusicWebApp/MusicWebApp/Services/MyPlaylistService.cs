using MusicWebApp.Models;

namespace MusicWebApp.Services;

/// <summary>A single personal playlist stored on this device, independent of favorites.</summary>
public class MyPlaylistService
{
    private readonly ILocalStorageService _storage;
    private readonly List<MusicTrack> _tracks = [];
    private bool _initialized;

    public MyPlaylistService(ILocalStorageService storage) => _storage = storage;

    public IReadOnlyList<MusicTrack> Tracks => _tracks;
    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        var saved = await _storage.GetItemAsync<List<MusicTrack>>(StorageKeys.MyPlaylist);
        if (saved is not null)
        {
            _tracks.AddRange(saved.Where(t => !string.IsNullOrWhiteSpace(t.VideoId))
                .DistinctBy(t => t.VideoId));
        }
        _initialized = true;
        OnChange?.Invoke();
    }

    public bool Contains(string videoId) => _tracks.Any(t => t.VideoId == videoId);

    /// <returns>True when the track is in the playlist after toggling.</returns>
    public async Task<bool> ToggleAsync(MusicTrack track)
    {
        var existing = _tracks.FirstOrDefault(t => t.VideoId == track.VideoId);
        if (existing is not null) _tracks.Remove(existing);
        else _tracks.Add(new MusicTrack
        {
            VideoId = track.VideoId,
            Title = track.Title,
            Artist = track.Artist,
            ThumbnailUrl = track.ThumbnailUrl,
            PublishedAt = track.PublishedAt,
            DurationSeconds = track.DurationSeconds
        });
        await _storage.SetItemAsync(StorageKeys.MyPlaylist, _tracks);
        OnChange?.Invoke();
        return existing is null;
    }
}
