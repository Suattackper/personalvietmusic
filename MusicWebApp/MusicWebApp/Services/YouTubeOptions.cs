namespace MusicWebApp.Services;

/// <summary>
/// Cấu hình YouTube Data API, đọc từ wwwroot/appsettings.json.
/// Lưu ý: đây là Blazor WebAssembly nên API key hiển thị được từ trình duyệt —
/// key KHÔNG phải secret, cần giới hạn theo HTTP referrer trong Google Cloud Console.
/// </summary>
public class YouTubeOptions
{
    public string ApiKey { get; set; } = string.Empty;
}
