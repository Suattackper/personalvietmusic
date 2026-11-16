using MusicWebApp.Models;

namespace MusicWebApp.Services;

public sealed class PersonalPlaylistService(ILocalStorageService storage)
{
    private const string Key = "anhtan_personal_playlist";
    private readonly List<MusicTrack> _tracks = new();
    private bool _loaded;
    public IReadOnlyList<MusicTrack> Tracks => _tracks;
    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_loaded) return;
        var saved = await storage.GetItemAsync<List<MusicTrack>>(Key);
        if (saved is not null) _tracks.AddRange(saved.Where(t => !string.IsNullOrWhiteSpace(t.VideoId)).DistinctBy(t => t.VideoId));
        _loaded = true;
        OnChange?.Invoke();
    }

    public async Task<bool> AddAsync(MusicTrack track)
    {
        await InitializeAsync();
        if (_tracks.Any(t => t.VideoId == track.VideoId)) return false;
        _tracks.Add(new MusicTrack { VideoId = track.VideoId, Title = track.Title, Artist = track.Artist, ThumbnailUrl = track.ThumbnailUrl, PublishedAt = track.PublishedAt });
        OnChange?.Invoke();
        await storage.SetItemAsync(Key, _tracks);
        return true;
    }

    public async Task RemoveAsync(string videoId)
    {
        await InitializeAsync();
        if (_tracks.RemoveAll(t => t.VideoId == videoId) == 0) return;
        OnChange?.Invoke();
        await storage.SetItemAsync(Key, _tracks);
    }
}
