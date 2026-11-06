using Microsoft.JSInterop;

namespace MusicWebApp.Services;

public sealed class LanguageService
{
    private readonly ILocalStorageService _storage;
    private readonly IJSRuntime _js;
    private bool _initialized;

    public LanguageService(ILocalStorageService storage, IJSRuntime js)
    {
        _storage = storage;
        _js = js;
    }

    public string Code { get; private set; } = "vi";
    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        var saved = await _storage.GetItemAsync(StorageKeys.Language);
        Code = saved == "en" ? "en" : "vi";
        _initialized = true;
        await ApplyDocumentLanguageAsync();
        OnChange?.Invoke();
    }

    public async Task SetAsync(string code)
    {
        var next = code == "en" ? "en" : "vi";
        if (next == Code) return;
        Code = next;
        await _storage.SetItemAsync(StorageKeys.Language, Code);
        await ApplyDocumentLanguageAsync();
        OnChange?.Invoke();
    }

    public string T(string vietnamese) => Code == "en" && English.TryGetValue(vietnamese, out var value)
        ? value : vietnamese;

    public string F(string vietnamese, params object[] args) => string.Format(T(vietnamese), args);

    private async Task ApplyDocumentLanguageAsync()
    {
        try { await _js.InvokeVoidAsync("vietmusicLanguage.apply", Code); }
        catch (JSException) { }
    }

    private static readonly Dictionary<string, string> English = new(StringComparer.Ordinal)
    {
        ["Bỏ qua điều hướng"] = "Skip navigation",
        ["Mở menu điều hướng"] = "Open navigation menu",
        ["Điều hướng chính"] = "Main navigation",
        ["Trang chủ"] = "Home",
        ["Tìm kiếm"] = "Search",
        ["Playlist"] = "Playlists",
        ["Yêu thích"] = "Favorites",
        ["Nghe gần đây"] = "Recently played",
        ["Hàng đợi"] = "Queue",
        ["Giới thiệu"] = "About",
        ["Chuyển sang giao diện sáng"] = "Switch to light theme",
        ["Chuyển sang giao diện tối"] = "Switch to dark theme",
        ["Ngôn ngữ"] = "Language",
        ["Tiếng Việt"] = "Vietnamese",
        ["Tiếng Anh"] = "English",
        ["Chào mừng đến với AnhTanMusic 🎵"] = "Welcome to AnhTanMusic 🎵",
        ["Khám phá và nghe nhạc yêu thích của bạn. Chọn một bộ sưu tập hoặc tìm kiếm bài hát bất kỳ."] = "Discover and listen to music you love. Choose a collection or search for any song.",
        ["🌍 Nhạc Việt, Anh, Nhật, Trung"] = "🌍 Vietnamese, English, Japanese & Chinese music",
        ["Mỗi danh sách có 130 bài. Mở danh sách để tìm và chọn bài bạn muốn nghe."] = "Each collection has 130 tracks. Open one to find and play a song.",
        ["Mở từng danh sách để tìm và chọn bài bạn muốn nghe."] = "Open a collection to find and play a song.",
        ["🎶 Nghe ngay — playlist có sẵn"] = "🎶 Play now — curated playlists",
        ["Khám phá theo thể loại"] = "Explore genres",
        ["Tìm kiếm {0}"] = "Search {0}",
        ["Playlist có sẵn"] = "Curated playlists",
        ["Chọn một bộ sưu tập theo ngôn ngữ, tìm bài yêu thích rồi phát. Các playlist quen thuộc vẫn có bên dưới."] = "Choose a language collection, find a favorite, and play it. Other playlists are below.",
        ["🌍 Chọn nhạc theo ngôn ngữ"] = "🌍 Choose music by language",
        ["✨ Nhạc mới 2025–2026"] = "✨ New music 2025–2026",
        ["🔥 TikTok viral theo ngôn ngữ"] = "🔥 Viral TikTok music by language",
        ["Mỗi danh sách có 100 bài tuyển từ nhiều mốc xu hướng; thứ tự không phải bảng xếp hạng TikTok chính thức."] = "Each collection has 100 songs selected across trend periods; the order is not an official TikTok ranking.",
        ["100 ca khúc được tuyển từ xu hướng TikTok, Douyin và playlist viral."] = "100 songs selected from TikTok and Douyin trends and viral playlists.",
        ["TikTok Việt — 100 bài"] = "Vietnamese TikTok — 100 songs",
        ["TikTok Anh — 100 bài"] = "English TikTok — 100 songs",
        ["TikTok Nhật — 100 bài"] = "Japanese TikTok — 100 songs",
        ["TikTok Trung — 100 bài"] = "Chinese TikTok — 100 songs",
        ["Nhạc Việt mới 2025–2026"] = "New Vietnamese music 2025–2026",
        ["Nhạc Anh mới 2025–2026"] = "New English music 2025–2026",
        ["Nhạc Nhật mới 2025–2026"] = "New Japanese music 2025–2026",
        ["Nhạc Trung mới 2025–2026"] = "New Chinese music 2025–2026",
        ["30 ca khúc nổi bật được đăng trong giai đoạn 2025–2026."] = "30 notable songs published in 2025–2026.",
        ["🎶 Playlist tuyển chọn"] = "🎶 Curated playlists",
        ["Đang tải playlist..."] = "Loading playlists...",
        ["Chưa có playlist"] = "No playlists yet",
        ["Không tìm thấy dữ liệu playlist (data/playlists.json)."] = "Playlist data could not be found.",
        ["Mở danh sách"] = "Open collection",
        ["Phát playlist"] = "Play playlist",
        ["{0} bài"] = "{0} tracks",
        ["{0} bài hát"] = "{0} songs",
        ["Không tìm thấy playlist"] = "Playlist not found",
        ["Playlist này không tồn tại hoặc đã bị xóa."] = "This playlist does not exist or has been removed.",
        ["Xem tất cả playlist"] = "View all playlists",
        ["Phát tất cả"] = "Play all",
        ["Phát ngẫu nhiên"] = "Shuffle play",
        ["Tìm bài trong danh sách"] = "Find a song in this collection",
        ["Tên bài hát hoặc nghệ sĩ..."] = "Song title or artist...",
        ["Không tìm thấy bài hát"] = "No songs found",
        ["Thử tìm bằng tên bài hoặc nghệ sĩ khác."] = "Try another song title or artist.",
        ["Đang phát playlist \"{0}\" ({1} bài)."] = "Playing playlist \"{0}\" ({1} tracks).",
        ["Đang phát ngẫu nhiên \"{0}\"."] = "Shuffling \"{0}\".",
        ["Bài hát yêu thích"] = "Favorite songs",
        ["Chưa có bài hát yêu thích"] = "No favorite songs yet",
        ["Bấm biểu tượng trái tim trên bài hát bất kỳ để thêm vào danh sách này."] = "Tap the heart on any song to add it here.",
        ["Hàng đợi phát"] = "Play queue",
        ["Xóa toàn bộ"] = "Clear all",
        ["Hàng đợi trống"] = "Queue is empty",
        ["Thêm bài hát vào hàng đợi từ kết quả tìm kiếm hoặc danh sách yêu thích."] = "Add songs from search results or favorites.",
        ["{0} bài hát trong hàng đợi"] = "{0} songs in queue",
        ["Đã xóa toàn bộ hàng đợi."] = "Queue cleared.",
        ["Xóa lịch sử"] = "Clear history",
        ["Chưa có lịch sử nghe"] = "No listening history yet",
        ["Các bài hát bạn phát sẽ xuất hiện ở đây (tối đa 50 bài)."] = "Songs you play will appear here (up to 50).",
        ["{0} bài hát đã nghe gần đây"] = "{0} recently played songs",
        ["Đã xóa lịch sử nghe."] = "Listening history cleared.",
        ["Bắt đầu tìm kiếm"] = "Start searching",
        ["Nhập tên bài hát hoặc nghệ sĩ vào ô tìm kiếm phía trên."] = "Enter a song title or artist in the search box above.",
        ["Kết quả cho \"{0}\""] = "Results for \"{0}\"",
        ["Đang tìm kiếm..."] = "Searching...",
        ["Đã xảy ra lỗi"] = "An error occurred",
        ["Thử lại"] = "Try again",
        ["Không tìm thấy kết quả"] = "No results found",
        ["Không có bài hát nào khớp với từ khóa. Hãy thử từ khóa khác."] = "No songs match your search. Try another keyword.",
        ["Đang tải thêm..."] = "Loading more...",
        ["Tải thêm kết quả"] = "Load more results",
        ["Tìm bài hát, nghệ sĩ..."] = "Search songs and artists...",
        ["Tìm kiếm bài hát"] = "Search for songs",
        ["Tìm"] = "Go",
        ["Không có dữ liệu"] = "No data",
        ["Đang tải..."] = "Loading...",
        ["Ảnh bìa {0}"] = "Cover art for {0}",
        ["Thời lượng {0}"] = "Duration {0}",
        ["Tạm dừng {0}"] = "Pause {0}",
        ["Phát {0}"] = "Play {0}",
        ["Xóa khỏi yêu thích"] = "Remove from favorites",
        ["Thêm vào yêu thích"] = "Add to favorites",
        ["Thêm vào hàng đợi"] = "Add to queue",
        ["Mở trên YouTube"] = "Open on YouTube",
        ["Đã thêm \"{0}\" vào yêu thích."] = "Added \"{0}\" to favorites.",
        ["Đã xóa \"{0}\" khỏi yêu thích."] = "Removed \"{0}\" from favorites.",
        ["Đã thêm \"{0}\" vào hàng đợi."] = "Added \"{0}\" to queue.",
        ["Đã xóa \"{0}\" khỏi hàng đợi."] = "Removed \"{0}\" from queue.",
        ["Phát bài này"] = "Play this song",
        ["Xóa khỏi hàng đợi"] = "Remove from queue",
        ["Xóa {0} khỏi hàng đợi"] = "Remove {0} from queue",
        ["Đóng thông báo"] = "Close notification",
        ["Ẩn khung video"] = "Hide video",
        ["Hiện khung video"] = "Show video",
        ["Trình phát nhạc"] = "Music player",
        ["Chưa phát bài nào"] = "Nothing playing",
        ["Chọn một bài hát để bắt đầu"] = "Choose a song to get started",
        ["Tắt phát ngẫu nhiên"] = "Turn shuffle off",
        ["Bật phát ngẫu nhiên"] = "Turn shuffle on",
        ["Bài trước"] = "Previous song",
        ["Tạm dừng"] = "Pause",
        ["Phát"] = "Play",
        ["Bài tiếp theo"] = "Next song",
        ["Bật lặp lại danh sách"] = "Repeat queue",
        ["Bật lặp lại một bài"] = "Repeat one song",
        ["Tắt lặp lại"] = "Turn repeat off",
        ["Thời gian hiện tại"] = "Current time",
        ["Thanh tiến trình bài hát"] = "Song progress",
        ["Tổng thời lượng"] = "Total duration",
        ["Âm lượng"] = "Volume",
        ["Không có bài tiếp theo trong hàng đợi."] = "No next song in the queue.",
        ["Không có bài trước đó trong hàng đợi."] = "No previous song in the queue.",
        ["Video này không cho phép phát nhúng."] = "This video cannot be embedded.",
        ["Video không tồn tại hoặc đã bị xóa."] = "This video is unavailable or has been deleted.",
        ["Video không hợp lệ."] = "Invalid video.",
        ["Không phát được video này."] = "This video cannot be played.",
        ["Không tìm thấy trang"] = "Page not found",
        ["Trang này không tồn tại."] = "This page does not exist.",
        ["Tải lại trang"] = "Reload page",
        ["Đóng"] = "Close",
        ["Giới thiệu AnhTanMusic"] = "About AnhTanMusic",
        ["Tác giả"] = "Creator",
        ["Kỹ sư phần mềm"] = "Software Engineer",
        ["Sinh năm 2003"] = "Born in 2003",
        ["Bắt đầu dự án: 24/07/2026"] = "Project started: July 24, 2026",
        ["Đang phát triển"] = "In development",
        ["Dự án cá nhân trong portfolio của tôi."] = "A personal project in my portfolio.",
        ["✨ Tính năng chính"] = "✨ Key features",
        ["📖 Cách dùng"] = "📖 How to use",
        ["🛠️ Công nghệ & công cụ"] = "🛠️ Technology & tools",
        ["⚠️ Lưu ý & giới hạn"] = "⚠️ Notes & limitations",
        ["Chúc bạn nghe nhạc vui vẻ! 💜"] = "Enjoy the music! 💜",
        ["Nhạc Việt"] = "Vietnamese music",
        ["Nhạc Anh"] = "English music",
        ["Nhạc Nhật"] = "Japanese music",
        ["Nhạc Trung"] = "Chinese music",
        ["130 bài nhạc Việt được yêu thích, từ hit mới đến ca khúc quen thuộc."] = "130 Vietnamese favorites, from new hits to familiar songs.",
        ["130 ca khúc tiếng Anh nổi bật qua nhiều năm."] = "130 standout English songs from across the years.",
        ["130 bài J-pop và nhạc Nhật nổi bật."] = "130 standout J-pop and Japanese songs.",
        ["130 ca khúc nhạc Hoa được yêu thích."] = "130 favorite Chinese songs.",
        ["Nhạc Việt được yêu thích, từ hit mới đến ca khúc quen thuộc."] = "Vietnamese favorites, from new hits to familiar songs.",
        ["Những ca khúc tiếng Anh nổi bật qua nhiều năm."] = "Standout English songs from across the years.",
        ["J-pop và nhạc Nhật nổi bật."] = "Standout J-pop and Japanese songs.",
        ["Những ca khúc nhạc Hoa được yêu thích."] = "Favorite Chinese songs.",
        ["Hot Hits Việt"] = "Vietnamese Hot Hits",
        ["Rap Việt"] = "Vietnamese Rap",
        ["Ballad Việt"] = "Vietnamese Ballads",
        ["Lofi Chill Dài"] = "Long Lofi Chill",
        ["Remix TikTok Dài"] = "Long TikTok Remixes",
        ["Nhạc Việt mới"] = "New Vietnamese music",
        ["Nhạc trẻ Việt Nam"] = "Vietnamese pop",
        ["Chill Việt"] = "Vietnamese chill",
        ["Acoustic Việt"] = "Vietnamese acoustic",
        ["Vui lòng nhập từ khóa tìm kiếm."] = "Please enter a search keyword.",
        ["YouTube API key chưa được cấu hình. Hãy điền API key vào wwwroot/appsettings.json (xem hướng dẫn trong README)."] = "YouTube API key is not configured. Add it to wwwroot/appsettings.json (see README).",
        ["Không thể kết nối tới YouTube. Vui lòng kiểm tra kết nối mạng và thử lại."] = "Cannot connect to YouTube. Check your network and try again.",
        ["Dữ liệu YouTube trả về không hợp lệ. Vui lòng thử lại sau."] = "YouTube returned invalid data. Please try again later.",
        ["Không có tiêu đề"] = "Untitled",
        ["Không rõ nghệ sĩ"] = "Unknown artist",
        ["YouTube API đã vượt quota trong ngày. Vui lòng thử lại sau hoặc dùng API key khác."] = "The daily YouTube API quota has been reached. Please try again later.",
        ["YouTube API key không hợp lệ. Hãy kiểm tra lại key trong wwwroot/appsettings.json."] = "The YouTube API key is invalid. Check wwwroot/appsettings.json.",
        ["YouTube từ chối yêu cầu (403). API key có thể bị giới hạn sai domain hoặc chưa bật YouTube Data API v3."] = "YouTube rejected the request (403). Check the allowed domain and YouTube Data API v3 settings.",
        ["Yêu cầu tìm kiếm không hợp lệ (400). Vui lòng thử từ khóa khác."] = "Invalid search request (400). Try another keyword.",
        ["YouTube API trả về lỗi ({0}). Vui lòng thử lại sau."] = "YouTube API returned an error ({0}). Please try again later.",
        ["Playlist riêng của Tân — 25 bản nhạc Việt chill chọn tay."] = "Tân's personal playlist — 25 handpicked Vietnamese chill tracks.",
        ["50 MV nhạc Việt đình đám — bài lẻ, bấm là phát ngay."] = "50 popular Vietnamese music videos — choose one and play.",
        ["50 bản rap Việt cháy nhất — từ MV chính chủ đến sân khấu Rap Việt."] = "50 energetic Vietnamese rap tracks, from official videos to live performances.",
        ["50 bản ballad Việt sâu lắng — nghe là thấm."] = "50 heartfelt Vietnamese ballads.",
        ["46 video lofi/chill dài — mở lên là nghe cả buổi."] = "46 long lofi and chill videos for extended listening.",
        ["50 mục gồm nonstop remix dài hàng giờ và hit TikTok sôi động."] = "50 extended remixes and upbeat TikTok hits.",
        ["Video lofi/chill dài — mở lên là nghe cả buổi."] = "Long lofi and chill videos for extended listening.",
        ["Nonstop remix dài hàng giờ và hit TikTok sôi động."] = "Extended remixes and upbeat TikTok hits."
    };
}
