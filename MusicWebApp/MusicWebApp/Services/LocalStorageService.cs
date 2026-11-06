using System.Text.Json;
using Microsoft.JSInterop;

namespace MusicWebApp.Services;

/// <summary>
/// Các key localStorage của ứng dụng.
/// </summary>
public static class StorageKeys
{
    public const string Favorites = "vietmusic_favorites";
    public const string Queue = "vietmusic_queue";
    public const string RecentlyPlayed = "vietmusic_recently_played";
    public const string Theme = "vietmusic_theme";
    public const string Language = "vietmusic_language";
    public const string Volume = "vietmusic_volume";
    public const string RepeatMode = "vietmusic_repeat_mode";
    public const string Shuffle = "vietmusic_shuffle";
}

public class LocalStorageService : ILocalStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("vietmusicStorage.getItem", key);
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("vietmusicStorage.setItem", key, value);
        }
        catch (JSException)
        {
            // localStorage bị chặn — bỏ qua, app vẫn hoạt động không lưu trữ.
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("vietmusicStorage.removeItem", key);
        }
        catch (JSException)
        {
        }
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            // JSON hỏng — xóa key lỗi và trả về giá trị mặc định thay vì crash.
            await RemoveItemAsync(key);
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await SetItemAsync(key, json);
    }
}
