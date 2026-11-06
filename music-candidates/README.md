# Danh sách nhạc ứng viên

Sau khi lọc video không cho nhúng hoặc không còn công khai, bốn danh sách chính gồm `vi.json` (123 bài), `en.json`, `ja.json`, `zh.json` (130 bài mỗi danh sách). Bốn danh sách riêng `vi-recent.json`, `en-recent.json`, `ja-recent.json`, `zh-recent.json` có 30 bài đăng trong giai đoạn 2025–2026 mỗi danh sách. Các danh sách này đã được xuất sang `wwwroot/data/music/` để ứng dụng hiển thị và phát. `check-embeds.mjs` tạo báo cáo cục bộ `embed-report.json` khi kiểm tra quyền nhúng.

Các bài được chọn từ video phổ biến lâu dài trên các trang thống kê của nghệ sĩ và một số video mới được chú ý tại thời điểm 06/11/2026. Lượt xem và xu hướng chỉ là tín hiệu tham khảo; “hay” vẫn là đánh giá chủ quan. Danh sách `zh` gồm nhạc Hoa, chủ yếu tiếng Quan thoại. Các tiêu đề giữ theo nguồn video.

Nguồn tham khảo:

- Việt: https://kworb.net/youtube/topvideos_vietnamese.html, https://kworb.net/youtube/trending/vn.html, cùng các trang thống kê Sơn Tùng M-TP, Hoàng Thùy Linh, Mỹ Tâm, Bích Phương, Hương Tràm và JustaTee.
- Anh: https://kworb.net/youtube/topvideos_anglophone.html
- Nhật: https://kworb.net/youtube/topvideos_japanese.html, cùng các trang YOASOBI, Kenshi Yonezu, Ado và Mrs. GREEN APPLE.
- Trung: https://kworb.net/youtube/artist/jaychou.html, https://kworb.net/youtube/artist/gem.html và https://kworb.net/youtube/trending/tw.html

Mỗi bài có `videoId`, tiêu đề, URL YouTube và URL nguồn. Tệp `expand.ps1` có thể lấy lại dữ liệu từ các nguồn; `export-app.mjs` xuất dữ liệu sang định dạng playlist của ứng dụng. Thời lượng được để trống vì nguồn thống kê không cung cấp nhất quán; trình phát sẽ nhận thời lượng khi video chạy. Video có thể thay đổi quyền nhúng theo thời gian.
