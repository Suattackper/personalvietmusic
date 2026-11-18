using System.Text.Json;
using MusicWebApp.Models;
using MusicWebApp.Services;

var storage = new MemoryStorage();
var first = new MusicTrack { VideoId = "first", Title = "First" };
var middle = new MusicTrack { VideoId = "middle", Title = "Middle" };
var last = new MusicTrack { VideoId = "last", Title = "Last" };

var favorites = new FavoritesService(storage);
await favorites.InitializeAsync();
foreach (var track in new[] { first, middle, last }) await favorites.ToggleFavoriteAsync(track);
await favorites.ToggleFavoriteAsync(middle);
Check(favorites.Favorites.Select(t => t.VideoId), "first", "last");
var restoredFavorites = new FavoritesService(storage);
await restoredFavorites.InitializeAsync();
Check(restoredFavorites.Favorites.Select(t => t.VideoId), "first", "last");

var playlist = new MyPlaylistService(storage);
await playlist.InitializeAsync();
foreach (var track in new[] { first, middle, last }) await playlist.ToggleAsync(track);
await playlist.ToggleAsync(middle);
Check(playlist.Tracks.Select(t => t.VideoId), "first", "last");
var restoredPlaylist = new MyPlaylistService(storage);
await restoredPlaylist.InitializeAsync();
Check(restoredPlaylist.Tracks.Select(t => t.VideoId), "first", "last");

Console.WriteLine("Collection regression checks passed: one removal preserved the other tracks and persisted correctly.");

static void Check(IEnumerable<string> actual, params string[] expected)
{
    if (!actual.SequenceEqual(expected))
        throw new InvalidOperationException($"Expected [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}]");
}

sealed class MemoryStorage : ILocalStorageService
{
    private readonly Dictionary<string, string> _items = [];
    public Task<string?> GetItemAsync(string key) => Task.FromResult(_items.GetValueOrDefault(key));
    public Task SetItemAsync(string key, string value) { _items[key] = value; return Task.CompletedTask; }
    public Task RemoveItemAsync(string key) { _items.Remove(key); return Task.CompletedTask; }
    public async Task<T?> GetItemAsync<T>(string key)
    {
        var value = await GetItemAsync(key);
        return value is null ? default : JsonSerializer.Deserialize<T>(value);
    }
    public Task SetItemAsync<T>(string key, T value) => SetItemAsync(key, JsonSerializer.Serialize(value));
}
