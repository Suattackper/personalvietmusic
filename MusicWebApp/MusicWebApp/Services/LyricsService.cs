using System.Net.Http.Json;

namespace MusicWebApp.Services;

public sealed class LyricDocument
{
    public string? Text { get; set; }
    public List<LyricLine> Lines { get; set; } = new();
}

public sealed class LyricLine
{
    public int StartMs { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<LyricWord> Words { get; set; } = new();
}

public sealed class LyricWord
{
    public int StartMs { get; set; }
    public int EndMs { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class LyricsService(HttpClient http)
{
    private Dictionary<string, LyricDocument>? _library;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<LyricDocument?> GetAsync(string videoId)
    {
        await _gate.WaitAsync();
        try
        {
            _library ??= await http.GetFromJsonAsync<Dictionary<string, LyricDocument>>("data/lyrics.json")
                ?? new Dictionary<string, LyricDocument>();
            return _library.GetValueOrDefault(videoId);
        }
        catch (HttpRequestException) { return null; }
        catch (System.Text.Json.JsonException) { return null; }
        finally { _gate.Release(); }
    }
}
