namespace MusicWebApp.Services;

/// <summary>
/// Lỗi khi gọi YouTube Data API, với thông điệp thân thiện để hiển thị trực tiếp lên UI.
/// </summary>
public class YouTubeApiException : Exception
{
    public YouTubeApiException(string friendlyMessage) : base(friendlyMessage)
    {
    }

    public YouTubeApiException(string friendlyMessage, Exception innerException)
        : base(friendlyMessage, innerException)
    {
    }
}
