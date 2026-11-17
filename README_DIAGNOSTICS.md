# YouTube IFrame Diagnostics

Bộ đo này ghi lại hành vi của ứng dụng và các giá trị **công khai** từ YouTube IFrame Player API. Nó không đọc iframe YouTube, không chặn quảng cáo, không giả trạng thái tab và không sửa luồng phát. YouTube IFrame API không có sự kiện quảng cáo chính thức; thời điểm quảng cáo do người kiểm thử tự đánh dấu.

## Bật và chạy

Chạy local theo `README.md`, rồi mở `http://localhost:5098/?ytdebug=1` (dùng đúng cổng mà `dotnet run` thông báo). Trên GitHub Pages, thêm `?ytdebug=1` vào URL trang. Production mặc định tắt; khi thiếu tham số này, module chỉ có các hàm rỗng và không tạo bảng, timer hay listener chẩn đoán.

1. Mở bảng **YT Debug**, bấm **Start Test** trước khi chọn bài/playlist.
2. Khi nghe thấy quảng cáo, bấm **Mark Ad Start**; khi quảng cáo kết thúc, bấm **Mark Ad End**. Khi nhạc bắt đầu, bấm **Mark Song Start**. Marker là quan sát thủ công, không phải kết quả phát hiện tự động.
3. Bấm **Stop Test** để ngừng đo và tháo các listener, timer, `PerformanceObserver`. Bấm **Export JSON** hoặc **Export CSV** để tải dữ liệu về máy. Mỗi lần Start Test tạo một đợt sự kiện mới trong phiên trang hiện tại.

Khi tab bị ẩn, không thể bấm marker ngay lúc nghe. Hãy ghi lại thời điểm bằng ghi chú bên ngoài; sau khi quay lại tab, đánh dấu thủ công và nêu rõ độ trễ. Đừng xem marker muộn là thời điểm quảng cáo thực sự bắt đầu.

## Bốn kịch bản

Chạy riêng từng kịch bản bằng **Start Test**, chọn playlist có ít nhất hai bài, rồi **Stop Test** và xuất JSON. Nên ghi thêm tên trình duyệt, hệ điều hành, trạng thái màn hình khóa, âm lượng và khoảng thời gian quan sát vào ghi chú riêng (bộ đo không thu thập các dữ liệu này).

| Kịch bản | Thao tác | Điều cần đối chiếu |
|---|---|---|
| Visible / manual next | Giữ tab visible, phát bài rồi bấm Next | `transition` với `manual_next`, `api_call` `loadVideoById`, marker do bạn bấm |
| Visible / auto next | Giữ tab visible đến khi bài kết thúc và tự chuyển | `player_state` `ENDED`, `transition` với `auto_after_end`, khoảng cách thời gian đến `PLAYING` và marker |
| Hidden / auto next | Bắt đầu khi visible, chuyển sang tab khác trước lúc bài kết thúc, chờ tự chuyển | `visibilitychange`, `hidden`, `ENDED`, `auto_after_end`, các khoảng `progress_timer` và `main_thread_lag` |
| Return visible / auto next | Tiếp tục kịch bản hidden, quay lại tab sau khi tự chuyển | `visibilitychange` visible, `focus`, trạng thái player và marker quảng cáo/nhạc bạn quan sát |

Lặp lại nhiều lần vì phân phối quảng cáo có thể thay đổi. Không kết luận “không có quảng cáo” chỉ vì không nghe thấy âm thanh quảng cáo. Khi tab ẩn, trình duyệt có thể hạn chế timer hoặc phát media; bộ đo chỉ thể hiện các event mà trang thực sự nhận được.

## Dữ liệu và giới hạn

JSON gồm `environment` (localhost/production, URL chỉ có origin và path, tên trình duyệt, viewport, hỗ trợ API), `sessionId` ngẫu nhiên, `events`, `markers` và `summary`. Mỗi event có ISO timestamp, `performance.now()`, visibility, focus, fullscreen, videoId, thời gian phát, duration, playerState, playbackRate và chi tiết thao tác. CSV là bảng event; marker cũng có dòng `manual_marker`. `summary.transitionCount` đếm lần ứng dụng chọn track mới; `api_call` cho biết liệu `loadVideoById` đã thực sự được gọi hay chưa. Phát lại cùng video bằng `playVideo` không được tính là đổi bài.

Module giữ tối đa 5.000 event và 500 marker; `summary` ghi số bản ghi đã bị loại. Bảng chỉ vẽ 120 event mới nhất. `progress_timer.detail.intervalMs` đo khoảng cách giữa các callback của timer 500 ms hiện có; callback đầu tiên sau khi timer khởi động có giá trị `null`. `main_thread_lag` đo độ trễ của timer chẩn đoán 1 giây; `long_task` dùng PerformanceObserver nếu trình duyệt hỗ trợ. Các cờ `environment.support` cho biết event/API nào không hỗ trợ. Tất cả phép đo có thể bị ảnh hưởng bởi việc tab ẩn và chính trình duyệt.

Mã hiện khởi tạo `new YT.Player` tại `MusicWebApp/MusicWebApp/wwwroot/js/youtubePlayer.js`; nó dùng `loadVideoById`, `playVideo`, `pauseVideo`, `seekTo`, `stopVideo`, `onStateChange`, `onError`. Ứng dụng **không gọi** `cueVideoById`, `loadPlaylist` hoặc `nextVideo`. Chuyển bài thuộc `MusicPlayerState` và `MusicPlayer.razor`, sau đó Blazor JS Interop gọi `vietmusicPlayer.loadVideo`.

Export do bạn chủ động tải về, không tự gửi lên server. Module không thu thập cookie, token, API key, request body hoặc query URL. File JSON/CSV có videoId và thời điểm nghe; hãy xem lại trước khi chia sẻ.

## Đối chiếu Network bằng Chrome DevTools

Nếu cần xem mạng, mở **DevTools → Network**, bật **Preserve log**, chọn **All**, rồi chạy lại kịch bản. Sau đó có thể chọn **Save all as HAR with content**. HAR có thể chứa URL, query, header, cookie và dữ liệu tài khoản. Trước khi chia sẻ, mở file HAR cục bộ và xóa hoặc thay thế các giá trị nhạy cảm trong `request.url`, `request.headers`, `response.headers`, `cookies`, `postData` và cả nội dung response nếu có. Kiểm tra lại file đã làm sạch. Không dùng DevTools để chặn hoặc sửa request YouTube trong phép thử này.

Chỉ so sánh thời điểm request và các event quan sát được. Network request riêng lẻ hoặc trạng thái `BUFFERING` không chứng minh có quảng cáo. Các giả thuyết về nguyên nhân cần được kiểm tra bằng nhiều phiên và dữ liệu bổ sung.
