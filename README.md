# 🎵 VietMusic — Web nghe nhạc Việt

Ứng dụng web nghe nhạc Việt hiện đại, responsive, xây dựng hoàn toàn phía frontend bằng **Blazor WebAssembly Standalone**. Tìm kiếm bài hát qua **YouTube Data API v3** và phát nhạc qua **YouTube IFrame Player API**. Có thể deploy miễn phí dưới dạng static website (Cloudflare Pages, GitHub Pages...).

## Tính năng

- 🏠 Trang chủ với các nhóm thể loại đề xuất (Nhạc Việt mới, V-Pop, Ballad Việt, Rap Việt...)
- 🔍 Tìm kiếm bài hát (Enter hoặc nút Tìm), tải thêm kết quả bằng `nextPageToken`
- ▶️ Trình phát cố định cuối màn hình: Play/Pause, Previous/Next, Repeat (None/All/One), Shuffle, thanh tiến trình (seek được), âm lượng
- 📃 Hàng đợi phát (Queue): phát, xóa từng bài, xóa tất cả — lưu và khôi phục qua localStorage
- ❤️ Yêu thích (Favorites) — không trùng lặp, lưu localStorage
- 🕘 Nghe gần đây (Recently Played) — tối đa 50 bài, có nút xóa lịch sử
- 🌙 Dark mode mặc định, chuyển dark/light, ghi nhớ lựa chọn
- 📱 Responsive: sidebar cố định (desktop) → thu gọn icon (tablet) → drawer (mobile)
- 🔔 Toast thông báo lỗi/thành công thống nhất, tự chuyển bài khi video lỗi hoặc không cho nhúng

## Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Framework | Blazor WebAssembly Standalone (.NET 8) |
| Ngôn ngữ | C#, JavaScript (interop), CSS thuần |
| Tìm kiếm nhạc | YouTube Data API v3 (`/youtube/v3/search`) |
| Phát nhạc | YouTube IFrame Player API |
| Lưu trữ | localStorage của trình duyệt |
| Backend | **Không có** — 100% static frontend |

## Kiến trúc frontend-only

- Không có server, không database, không Web API riêng.
- Toàn bộ logic chạy trong trình duyệt (WebAssembly).
- Nhạc phát qua **YouTube player nhúng chính thức** — ứng dụng không tải/trích xuất MP3.
- Sau khi `dotnet publish`, kết quả là một thư mục static (`wwwroot`) có thể host ở bất kỳ static hosting nào.

## Yêu cầu môi trường

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên
- Trình duyệt hiện đại (Chrome, Edge, Firefox, Safari)
- Một YouTube API key (miễn phí — xem bên dưới)

## Chạy local

```bash
cd MusicWebApp
dotnet run --project MusicWebApp
```

Mở địa chỉ hiển thị trong console (mặc định `http://localhost:5098`).

## Truy cập từ thiết bị khác trong mạng LAN

Dev server được cấu hình lắng nghe trên mọi địa chỉ IP (`http://0.0.0.0:5098` trong `Properties/launchSettings.json`), nên điện thoại/máy tính cùng mạng có thể truy cập qua IP của máy chạy server:

1. Xem IP máy: chạy `ipconfig`, lấy địa chỉ IPv4 (ví dụ `192.168.1.10`).
2. Trên thiết bị khác, mở `http://<IP-máy>:5098`.
3. Nếu không vào được, mở port 5098 trên Windows Firewall (chạy PowerShell **với quyền Administrator**):

```bash
netsh advfirewall firewall add rule name="VietMusic Dev 5098" dir=in action=allow protocol=TCP localport=5098
```

Lưu ý:
- Nếu API key có giới hạn HTTP referrer, cần thêm `http://<IP-máy>:5098/*` vào danh sách referrer trong Google Cloud Console, nếu không thiết bị khác sẽ bị lỗi 403 khi tìm kiếm.
- Favorites/Queue/Lịch sử lưu theo origin của trình duyệt, nên dữ liệu trên `localhost` và trên `<IP-máy>` là hai bộ riêng biệt.

## Chất lượng video

Ứng dụng chủ động yêu cầu chất lượng thấp nhất (`tiny` ≈ 144p) khi phát để tiết kiệm băng thông — cấu hình trong `wwwroot/js/youtubePlayer.js` (hằng số `PREFERRED_QUALITY`). YouTube vẫn có thể tự điều chỉnh theo kích thước player, nhưng player nhỏ nên thực tế giữ ở mức 144p–240p.

## Tạo YouTube API key

1. Vào [Google Cloud Console](https://console.cloud.google.com/).
2. Tạo một project mới (hoặc chọn project có sẵn).
3. Vào **APIs & Services → Library**, tìm **YouTube Data API v3** và bấm **Enable**.
4. Vào **APIs & Services → Credentials → Create Credentials → API key**.
5. Copy API key vừa tạo.

### Giới hạn API key theo HTTP referrer (bắt buộc nên làm)

Vì đây là ứng dụng chạy hoàn toàn phía trình duyệt, **API key sẽ hiển thị công khai** trong request. Key này **không phải secret** — thay vào đó bạn phải giới hạn nơi được phép dùng key:

1. Trong **Credentials**, bấm vào API key vừa tạo.
2. Ở mục **Application restrictions**, chọn **Websites** (HTTP referrers).
3. Thêm domain của bạn, ví dụ:
   - `http://localhost:5098/*` (chạy local)
   - `https://ten-mien-cua-ban.pages.dev/*` (Cloudflare Pages)
   - `https://username.github.io/*` (GitHub Pages)
4. Ở mục **API restrictions**, chọn **Restrict key** và chỉ tick **YouTube Data API v3**.
5. Lưu lại.

### Thêm key vào ứng dụng

Mở file `MusicWebApp/MusicWebApp/wwwroot/appsettings.json` và thay `YOUR_YOUTUBE_API_KEY`:

```json
{
  "YouTube": {
    "ApiKey": "YOUR_YOUTUBE_API_KEY"
  }
}
```

⚠️ Không commit API key thật lên repository công khai nếu key chưa được giới hạn referrer.

## Build

```bash
cd MusicWebApp
dotnet build
```

## Publish (tạo static website)

```bash
cd MusicWebApp
dotnet publish -c Release
```

Thư mục static để deploy nằm tại:

```
MusicWebApp/MusicWebApp/bin/Release/net8.0/publish/wwwroot
```

Toàn bộ nội dung thư mục `wwwroot` này chính là website tĩnh hoàn chỉnh.

## Deploy lên Cloudflare Pages

1. Đăng nhập [Cloudflare Pages](https://pages.cloudflare.com/) → **Create a project**.
2. Cách đơn giản nhất — **Direct Upload**: chạy `dotnet publish -c Release` ở máy, rồi kéo thả thư mục `bin/Release/net8.0/publish/wwwroot` lên.
3. Hoặc kết nối Git repository và cấu hình build:
   - **Build command**:
     ```
     curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- -c 8.0 && ~/.dotnet/dotnet publish MusicWebApp/MusicWebApp/MusicWebApp.csproj -c Release -o output
     ```
   - **Build output directory**: `output/wwwroot`
4. Sau khi có domain (`*.pages.dev`), quay lại Google Cloud Console thêm domain đó vào HTTP referrer của API key.

Blazor WASM dùng client-side routing — nếu truy cập trực tiếp `/search` bị 404, thêm file `_redirects` vào `wwwroot` với nội dung:

```
/* /index.html 200
```

## Deploy lên GitHub Pages (tự động bằng GitHub Actions)

Project đã có sẵn workflow `.github/workflows/deploy.yml` lo hết phần khó (sửa `base href` theo tên repo, tạo `404.html`, thêm `.nojekyll`). Bạn chỉ cần push code:

1. Tạo repository mới trên [github.com](https://github.com/new) (ví dụ tên `vietmusic`).
2. Ở thư mục gốc project (`Music Webapp`), chạy:
   ```bash
   git init
   git add .
   git commit -m "VietMusic"
   git branch -M main
   git remote add origin https://github.com/<username>/<ten-repo>.git
   git push -u origin main
   ```
3. Trên GitHub: **Settings → Pages → Build and deployment → Source** chọn **GitHub Actions**.
4. (Khuyến nghị) Để API key không nằm trong source: **Settings → Secrets and variables → Actions → New repository secret**, tên `YOUTUBE_API_KEY`, giá trị là key thật. Đồng thời để `wwwroot/appsettings.json` trong repo ở dạng placeholder `YOUR_YOUTUBE_API_KEY`. Workflow sẽ tự chèn key từ secret khi build.
5. Mỗi lần push lên `main`, Actions tự build và deploy. Xem tiến trình ở tab **Actions**; xong sẽ có URL `https://<username>.github.io/<ten-repo>/`.
6. **Bắt buộc** — vào Google Cloud Console thêm `https://<username>.github.io/*` vào HTTP referrer của API key, nếu không tìm kiếm sẽ bị lỗi 403.

## Giới hạn cần biết

- **Quota API**: YouTube Data API v3 cho 10.000 units/ngày miễn phí; mỗi lần search tốn 100 units (~100 lần tìm kiếm/ngày). Hết quota sẽ có thông báo lỗi trong app.
- **API key không phải secret**: chạy phía trình duyệt nên ai cũng xem được key — bắt buộc giới hạn theo HTTP referrer + chỉ cho phép YouTube Data API v3.
- **Một số video không phát được**: video chặn nhúng (embed) sẽ báo lỗi và ứng dụng tự chuyển bài tiếp theo.
- **Phát qua YouTube player**: ứng dụng nhúng player chính thức của YouTube, không phải phát file MP3 độc lập; cần có mạng khi nghe.
- **localStorage theo trình duyệt**: favorites/lịch sử/queue lưu trên máy người dùng, xóa dữ liệu trình duyệt sẽ mất.

## Cấu trúc thư mục chính

```
MusicWebApp/MusicWebApp/
├── Components/        # MusicPlayer, TrackList, TrackCard, SearchBox, QueuePanel...
├── Layout/            # MainLayout, NavMenu (sidebar)
├── Models/            # MusicTrack, YouTubeSearchResponse, RepeatMode...
├── Pages/             # Home, Search, Favorites, RecentlyPlayed, Queue
├── Services/          # YouTubeMusicService, MusicPlayerState, LocalStorage, Favorites...
└── wwwroot/
    ├── appsettings.json   # 👈 điền YouTube API key tại đây
    ├── css/app.css        # toàn bộ style (CSS variables, dark/light)
    └── js/                # youtubePlayer.js, storage.js
```
