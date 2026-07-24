namespace MusicWebApp.Services;

public interface ILocalStorageService
{
    Task<string?> GetItemAsync(string key);
    Task SetItemAsync(string key, string value);
    Task RemoveItemAsync(string key);

    /// <summary>Đọc và deserialize JSON. Trả về default nếu key không tồn tại hoặc JSON hỏng.</summary>
    Task<T?> GetItemAsync<T>(string key);

    /// <summary>Serialize thành JSON rồi lưu.</summary>
    Task SetItemAsync<T>(string key, T value);
}
