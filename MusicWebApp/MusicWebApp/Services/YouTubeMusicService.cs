using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using MusicWebApp.Models;

namespace MusicWebApp.Services;

public class YouTubeMusicService : IYouTubeMusicService
{
    private const string SearchEndpoint = "https://www.googleapis.com/youtube/v3/search";
    private const string VideosEndpoint = "https://www.googleapis.com/youtube/v3/videos";
    private const string ApiKeyPlaceholder = "YOUR_YOUTUBE_API_KEY";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly YouTubeOptions _options;
    private readonly LanguageService _language;

    public YouTubeMusicService(HttpClient httpClient, YouTubeOptions options, LanguageService language)
    {
        _httpClient = httpClient;
        _options = options;
        _language = language;
    }

    public async Task<YouTubeSearchResult> SearchAsync(
        string keyword,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            throw new YouTubeApiException(_language.T("Vui lòng nhập từ khóa tìm kiếm."));
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey == ApiKeyPlaceholder)
        {
            throw new YouTubeApiException(
                _language.T("YouTube API key chưa được cấu hình. Hãy điền API key vào wwwroot/appsettings.json (xem hướng dẫn trong README)."));
        }

        var url = BuildSearchUrl(keyword.Trim(), pageToken);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(url, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new YouTubeApiException(
                _language.T("Không thể kết nối tới YouTube. Vui lòng kiểm tra kết nối mạng và thử lại."), ex);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new YouTubeApiException(BuildErrorMessage(response.StatusCode, body));
        }

        YouTubeSearchResponse? searchResponse;
        try
        {
            searchResponse = JsonSerializer.Deserialize<YouTubeSearchResponse>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new YouTubeApiException(
                _language.T("Dữ liệu YouTube trả về không hợp lệ. Vui lòng thử lại sau."), ex);
        }

        if (searchResponse is null)
        {
            throw new YouTubeApiException(_language.T("Dữ liệu YouTube trả về không hợp lệ. Vui lòng thử lại sau."));
        }

        var tracks = searchResponse.Items
            .Where(item => !string.IsNullOrEmpty(item.Id?.VideoId))
            .Select(MapToTrack)
            .ToList();

        // Bổ sung thời lượng qua endpoint videos (1 quota unit cho cả trang kết quả).
        await EnrichDurationsAsync(tracks, cancellationToken);

        return new YouTubeSearchResult
        {
            Tracks = tracks,
            NextPageToken = searchResponse.NextPageToken,
            PrevPageToken = searchResponse.PrevPageToken,
            TotalResults = searchResponse.PageInfo?.TotalResults ?? 0
        };
    }

    private async Task EnrichDurationsAsync(List<MusicTrack> tracks, CancellationToken cancellationToken)
    {
        if (tracks.Count == 0)
        {
            return;
        }

        var url = VideosEndpoint
            + "?part=contentDetails"
            + "&id=" + Uri.EscapeDataString(string.Join(",", tracks.Select(t => t.VideoId)))
            + "&key=" + Uri.EscapeDataString(_options.ApiKey);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return; // Thời lượng chỉ là thông tin phụ — không chặn kết quả tìm kiếm.
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<YouTubeVideosResponse>(body, JsonOptions);
            if (data?.Items is null)
            {
                return;
            }

            var durations = data.Items
                .Where(item => !string.IsNullOrEmpty(item.Id))
                .ToDictionary(item => item.Id!, item => ParseIsoDuration(item.ContentDetails?.Duration));

            foreach (var track in tracks)
            {
                if (durations.TryGetValue(track.VideoId, out var seconds) && seconds > 0)
                {
                    track.DurationSeconds = seconds;
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Lỗi mạng/JSON khi lấy thời lượng — bỏ qua, kết quả tìm kiếm vẫn dùng được.
        }
    }

    /// <summary>Parse thời lượng ISO 8601 của YouTube (vd "PT1H2M30S") thành giây.</summary>
    private static int ParseIsoDuration(string? iso)
    {
        if (string.IsNullOrEmpty(iso))
        {
            return 0;
        }

        var match = Regex.Match(iso, @"^PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?$");
        if (!match.Success)
        {
            return 0;
        }

        var hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        var minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var seconds = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        return hours * 3600 + minutes * 60 + seconds;
    }

    private string BuildSearchUrl(string keyword, string? pageToken)
    {
        var url = SearchEndpoint
            + "?part=snippet"
            + "&type=video"
            + "&videoCategoryId=10"
            + "&maxResults=20"
            + "&q=" + Uri.EscapeDataString(keyword)
            + "&key=" + Uri.EscapeDataString(_options.ApiKey);

        if (!string.IsNullOrEmpty(pageToken))
        {
            url += "&pageToken=" + Uri.EscapeDataString(pageToken);
        }

        return url;
    }

    private MusicTrack MapToTrack(YouTubeSearchItem item)
    {
        var snippet = item.Snippet;
        var thumbnails = snippet?.Thumbnails;
        var thumbnailUrl = thumbnails?.High?.Url
            ?? thumbnails?.Medium?.Url
            ?? thumbnails?.Default?.Url
            ?? string.Empty;

        return new MusicTrack
        {
            VideoId = item.Id!.VideoId!,
            Title = WebUtility.HtmlDecode(snippet?.Title ?? _language.T("Không có tiêu đề")),
            Artist = WebUtility.HtmlDecode(snippet?.ChannelTitle ?? _language.T("Không rõ nghệ sĩ")),
            ThumbnailUrl = thumbnailUrl,
            PublishedAt = snippet?.PublishedAt
        };
    }

    private string BuildErrorMessage(HttpStatusCode statusCode, string body)
    {
        // Đọc "reason" từ JSON lỗi của Google để phân biệt sai key / hết quota.
        var reason = TryGetErrorReason(body);

        return (statusCode, reason) switch
        {
            (_, "quotaExceeded") or (_, "dailyLimitExceeded") =>
                _language.T("YouTube API đã vượt quota trong ngày. Vui lòng thử lại sau hoặc dùng API key khác."),
            (HttpStatusCode.BadRequest, "keyInvalid") or (HttpStatusCode.BadRequest, "badRequest") =>
                _language.T("YouTube API key không hợp lệ. Hãy kiểm tra lại key trong wwwroot/appsettings.json."),
            (HttpStatusCode.Forbidden, _) =>
                _language.T("YouTube từ chối yêu cầu (403). API key có thể bị giới hạn sai domain hoặc chưa bật YouTube Data API v3."),
            (HttpStatusCode.BadRequest, _) =>
                _language.T("Yêu cầu tìm kiếm không hợp lệ (400). Vui lòng thử từ khóa khác."),
            _ =>
                _language.F("YouTube API trả về lỗi ({0}). Vui lòng thử lại sau.", (int)statusCode)
        };
    }

    private static string? TryGetErrorReason(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("errors", out var errors)
                && errors.ValueKind == JsonValueKind.Array
                && errors.GetArrayLength() > 0
                && errors[0].TryGetProperty("reason", out var reasonElement))
            {
                return reasonElement.GetString();
            }
        }
        catch (JsonException)
        {
            // Body không phải JSON — bỏ qua, dùng thông điệp theo status code.
        }

        return null;
    }
}
