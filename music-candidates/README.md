# Danh sách nhạc ứng viên

Sau khi lọc video không cho nhúng hoặc không còn công khai, bốn danh sách chính gồm `vi.json` (123 bài), `en.json`, `ja.json`, `zh.json` (130 bài mỗi danh sách). Bốn danh sách riêng `vi-recent.json`, `en-recent.json`, `ja-recent.json`, `zh-recent.json` có 30 bài đăng trong giai đoạn 2025–2026 mỗi danh sách. Các danh sách này đã được xuất sang `wwwroot/data/music/` để ứng dụng hiển thị và phát. `check-embeds.mjs` tạo báo cáo cục bộ `embed-report.json` khi kiểm tra quyền nhúng.

Bốn danh sách TikTok riêng `vi-tiktok.json`, `en-tiktok.json`, `ja-tiktok.json`, `zh-tiktok.json` có 100 bài mỗi danh sách. Mỗi mục ghi URL nguồn xu hướng, thứ hạng tại nguồn và video YouTube đã đối chiếu. Đây là tập hợp từ nhiều bảng và playlist viral công khai tại ngày 06/11/2026, không phải bảng xếp hạng Top 100 chính thức của TikTok. Các nguồn đã dùng:

- Việt: [Soundcharts TikTok Vietnam](https://www.soundcharts.com/en/charts/tiktok/vietnam) và [Viral Viet Pop của Apple Music](https://music.apple.com/vn/playlist/viral-viet-pop/pl.8f3d660ab42e40dc87c30287185fe787).
- Anh: [bảng Popular](https://trendingtiktoksongs.com/charts/us), [bảng Viral Mỹ](https://trendingtiktoksongs.com/charts/us/viral) và [playlist TikTok October 2026](https://music.apple.com/us/playlist/tiktok-songs-october-2026-viral-chart-hits-every-2026/pl.d9dcaa71eae146549c216c6fc81640bd).
- Nhật: [bảng Popular](https://trendingtiktoksongs.com/charts/jp), [bảng Viral Nhật](https://trendingtiktoksongs.com/charts/jp/viral) và [TikTok Viral Hits Japan](https://music.apple.com/jp/playlist/tiktok-viral-hits-japan/pl.28fccfa0f3e747c9b5a98e748100a33c).
- Trung: [Douyin 2025](https://music.apple.com/cn/playlist/%E6%8A%96%E9%9F%B3-2025-%E5%B9%B4%E5%BA%A6%E7%83%AD%E6%AD%8C%E6%A6%9C/pl.35323002b653460090e3d1b9a9707482) và [Douyin tháng 9/2026](https://music.apple.com/cn/playlist/2026-%E5%B9%B4-9-%E6%9C%88%E6%8A%96%E9%9F%B3%E7%83%AD%E6%AD%8C%E6%A6%9C/pl.d1f66de41a364273bc81284fe843eab0).

Các bài được chọn từ video phổ biến lâu dài trên các trang thống kê của nghệ sĩ và một số video mới được chú ý tại thời điểm 06/11/2026. Lượt xem và xu hướng chỉ là tín hiệu tham khảo; “hay” vẫn là đánh giá chủ quan. Danh sách `zh` gồm nhạc Hoa, chủ yếu tiếng Quan thoại. Các tiêu đề giữ theo nguồn video.

Nguồn tham khảo:

- Việt: https://kworb.net/youtube/topvideos_vietnamese.html, https://kworb.net/youtube/trending/vn.html, cùng các trang thống kê Sơn Tùng M-TP, Hoàng Thùy Linh, Mỹ Tâm, Bích Phương, Hương Tràm và JustaTee.
- Anh: https://kworb.net/youtube/topvideos_anglophone.html
- Nhật: https://kworb.net/youtube/topvideos_japanese.html, cùng các trang YOASOBI, Kenshi Yonezu, Ado và Mrs. GREEN APPLE.
- Trung: https://kworb.net/youtube/artist/jaychou.html, https://kworb.net/youtube/artist/gem.html và https://kworb.net/youtube/trending/tw.html

Mỗi bài có `videoId`, tiêu đề, URL YouTube và URL nguồn. Tệp `expand.ps1` có thể lấy lại dữ liệu từ các nguồn; `export-app.mjs` xuất dữ liệu sang định dạng playlist của ứng dụng. Thời lượng được để trống vì nguồn thống kê không cung cấp nhất quán; trình phát sẽ nhận thời lượng khi video chạy. Video có thể thay đổi quyền nhúng theo thời gian.
