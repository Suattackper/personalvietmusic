// Quản lý MỘT YouTube IFrame Player duy nhất cho toàn app.
// Nạp https://www.youtube.com/iframe_api đúng một lần, callback về Blazor
// qua DotNetObjectReference.
window.vietmusicPlayer = (function () {
    const IFRAME_API_SRC = "https://www.youtube.com/iframe_api";
    const PROGRESS_INTERVAL_MS = 500;
    // Chất lượng thấp nhất để tiết kiệm băng thông (app chủ yếu để nghe nhạc).
    // "tiny" = 144p; YouTube hiện ưu tiên tự chọn theo kích thước player,
    // player nhỏ (~360x204) nên thực tế sẽ ở mức 144p-240p.
    const PREFERRED_QUALITY = "tiny";

    let player = null;
    let playerReady = false;
    let dotNetRef = null;
    let apiReadyPromise = null;
    let progressTimer = null;
    let lastProgressCallback = null;
    const diagnostics = window.anhTanYtDiagnostics;
    diagnostics.setPlayerProvider?.(() => player);
    function observe(type, detail) { diagnostics.record(type, detail); }

    // Chống việc trình duyệt (nhất là mobile) tự pause khi tab bị ẩn:
    // chỉ tự phát lại khi KHÔNG phải người dùng chủ động pause.
    let userPaused = false;
    let pausedWhileHidden = false;
    let backgroundResumeAttempts = 0;
    const MAX_BACKGROUND_RESUME_ATTEMPTS = 2;

    function tryResume() {
        if (player && playerReady && !userPaused) {
            try {
                observe("api_call", { method: "playVideo", reason: "hidden_pause_recovery" });
                player.playVideo();
            } catch (e) { }
        }
    }

    document.addEventListener("visibilitychange", function () {
        if (document.visibilityState !== "visible") {
            return;
        }
        backgroundResumeAttempts = 0;
        // Quay lại tab: nếu nhạc bị pause trong lúc tab ẩn (do trình duyệt,
        // không phải do người dùng) thì phát tiếp.
        if (pausedWhileHidden && !userPaused) {
            pausedWhileHidden = false;
            try {
                if (player && playerReady && player.getPlayerState() === YT.PlayerState.PAUSED) {
                    observe("api_call", { method: "playVideo", reason: "visible_resume_recovery" });
                    player.playVideo();
                }
            } catch (e) { }
        }
    });

    function loadIframeApi() {
        if (window.YT && window.YT.Player) {
            return Promise.resolve();
        }
        if (apiReadyPromise) {
            return apiReadyPromise;
        }
        apiReadyPromise = new Promise(function (resolve) {
            const previous = window.onYouTubeIframeAPIReady;
            window.onYouTubeIframeAPIReady = function () {
                if (typeof previous === "function") {
                    previous();
                }
                resolve();
            };
            if (!document.querySelector('script[src="' + IFRAME_API_SRC + '"]')) {
                const tag = document.createElement("script");
                tag.src = IFRAME_API_SRC;
                document.head.appendChild(tag);
            }
        });
        return apiReadyPromise;
    }

    function startProgressTimer() {
        stopProgressTimer();
        lastProgressCallback = null;
        progressTimer = setInterval(function () {
            const now = performance.now();
            const intervalMs = lastProgressCallback === null ? null : Math.round((now - lastProgressCallback) * 100) / 100;
            lastProgressCallback = now;
            if (!player || !playerReady || !dotNetRef) {
                return;
            }
            try {
                const current = player.getCurrentTime() || 0;
                const duration = player.getDuration() || 0;
                diagnostics.tick(intervalMs, current, duration);
                dotNetRef.invokeMethodAsync("OnPlayerProgress", current, duration);
            } catch (e) {
                // Player đang bị hủy — bỏ qua.
            }
        }, PROGRESS_INTERVAL_MS);
    }

    function stopProgressTimer() {
        if (progressTimer) {
            clearInterval(progressTimer);
            progressTimer = null;
        }
        lastProgressCallback = null;
    }

    function notify(methodName, ...args) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync(methodName, ...args).catch(function () { });
        }
    }

    return {
        initializePlayer: async function (elementId, dotNetReference) {
            dotNetRef = dotNetReference;
            if (player) {
                return; // Chỉ tạo một player duy nhất.
            }
            await loadIframeApi();
            if (player) {
                return;
            }
            observe("api_call", { method: "YT.Player", reason: "initialize" });
            player = new YT.Player(elementId, {
                width: "100%",
                height: "100%",
                playerVars: {
                    playsinline: 1,
                    rel: 0,
                    modestbranding: 1
                },
                events: {
                    onReady: function () {
                        playerReady = true;
                        observe("player_ready");
                        try {
                            player.setPlaybackQuality(PREFERRED_QUALITY);
                        } catch (e) { }
                        notify("OnPlayerReady");
                    },
                    onStateChange: function (event) {
                        observe("player_state", { stateCode: event.data });
                        if (window.YT && event.data === YT.PlayerState.PLAYING) {
                            // Nhắc lại chất lượng thấp mỗi khi bắt đầu phát,
                            // vì YouTube có thể tự nâng chất lượng khi đổi video.
                            try {
                                player.setPlaybackQuality(PREFERRED_QUALITY);
                            } catch (e) { }
                            startProgressTimer();
                        } else {
                            stopProgressTimer();
                        }

                        // Bị pause trong lúc tab ẩn mà người dùng không bấm dừng
                        // => trình duyệt/YouTube tự pause. Thử phát lại (tối đa 2 lần
                        // mỗi lượt ẩn tab) để giữ nhạc chạy nền trên mobile.
                        if (window.YT && event.data === YT.PlayerState.PAUSED
                            && document.hidden && !userPaused) {
                            pausedWhileHidden = true;
                            if (backgroundResumeAttempts < MAX_BACKGROUND_RESUME_ATTEMPTS) {
                                backgroundResumeAttempts++;
                                setTimeout(function () {
                                    if (document.hidden) {
                                        tryResume();
                                    }
                                }, 300);
                            }
                        }

                        notify("OnPlayerStateChanged", event.data);
                    },
                    onError: function (event) {
                        observe("player_error", { errorCode: event.data });
                        stopProgressTimer();
                        notify("OnPlayerError", event.data);
                    }
                }
            });
        },

        loadVideo: function (videoId, reason) {
            if (player && playerReady && videoId) {
                userPaused = false;
                pausedWhileHidden = false;
                backgroundResumeAttempts = 0;
                observe("api_call", { method: "loadVideoById", videoId: videoId, reason: reason || "unspecified" });
                player.loadVideoById({
                    videoId: videoId,
                    suggestedQuality: PREFERRED_QUALITY
                });
            }
        },

        noteTransition: function (videoId, reason) {
            observe("transition", { reason: reason || "unspecified", toVideoId: videoId });
        },

        playVideo: function () {
            if (player && playerReady) {
                userPaused = false;
                pausedWhileHidden = false;
                observe("api_call", { method: "playVideo", reason: "app_request" });
                player.playVideo();
            }
        },

        pauseVideo: function () {
            if (player && playerReady) {
                userPaused = true;
                observe("api_call", { method: "pauseVideo", reason: "app_request" });
                player.pauseVideo();
            }
        },

        stopVideo: function () {
            if (player && playerReady) {
                userPaused = true;
                observe("api_call", { method: "stopVideo", reason: "app_request" });
                player.stopVideo();
            }
        },

        seekTo: function (seconds, reason) {
            if (player && playerReady) {
                observe("api_call", { method: "seekTo", seconds: seconds, reason: reason || "user_seek" });
                player.seekTo(seconds, true);
            }
        },

        setVolume: function (volume) {
            if (player && playerReady) {
                player.setVolume(volume);
            }
        },

        getVolume: function () {
            return player && playerReady ? player.getVolume() : 0;
        },

        getCurrentTime: function () {
            return player && playerReady ? (player.getCurrentTime() || 0) : 0;
        },

        getDuration: function () {
            return player && playerReady ? (player.getDuration() || 0) : 0;
        },

        getPlayerState: function () {
            return player && playerReady ? player.getPlayerState() : -1;
        },

        destroyPlayer: function () {
            observe("api_call", { method: "destroy", reason: "component_dispose" });
            diagnostics.stop();
            stopProgressTimer();
            if (player) {
                try {
                    player.destroy();
                } catch (e) {
                    // Bỏ qua lỗi khi hủy.
                }
                player = null;
            }
            playerReady = false;
            dotNetRef = null;
        }
    };
})();
