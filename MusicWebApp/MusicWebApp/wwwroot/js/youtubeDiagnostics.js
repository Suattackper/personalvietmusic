// Opt-in, local-only observations of the public YouTube IFrame Player API.
// No iframe DOM, network requests, cookies, credentials or storage are read.
window.anhTanYtDiagnostics = (() => {
    const enabled = new URLSearchParams(location.search).get("ytdebug") === "1";
    if (!enabled) return { record() {}, tick() {}, stop() {} };

    const MAX_EVENTS = 5000;
    const MAX_MARKERS = 500;
    const displayedEvents = 120;
    const stateNames = { "-1": "UNSTARTED", 0: "ENDED", 1: "PLAYING", 2: "PAUSED", 3: "BUFFERING", 5: "CUED" };
    const support = {
        visibilitychange: "onvisibilitychange" in document,
        focus: "onfocus" in window,
        blur: "onblur" in window,
        pagehide: "onpagehide" in window,
        pageshow: "onpageshow" in window,
        freeze: "onfreeze" in document,
        resume: "onresume" in document,
        performanceObserverLongTask: typeof PerformanceObserver !== "undefined"
            && PerformanceObserver.supportedEntryTypes?.includes("longtask") === true
    };
    const browser = (() => {
        const ua = navigator.userAgent;
        if (/Edg\//.test(ua)) return "Edge";
        if (/Chrome\//.test(ua)) return "Chrome";
        if (/Firefox\//.test(ua)) return "Firefox";
        if (/Safari\//.test(ua)) return "Safari";
        return "Other";
    })();
    const environment = {
        host: /^(localhost|127\.0\.0\.1|\[::1\])$/.test(location.hostname) ? "localhost" : "production",
        url: location.origin + location.pathname, // Deliberately excludes query and hash.
        browser,
        viewport: { width: innerWidth, height: innerHeight },
        support
    };
    const sessionId = crypto.randomUUID ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(36).slice(2)}`;
    let events = [], markers = [], active = false, playerProvider = null;
    let droppedEvents = 0, droppedMarkers = 0, transitions = 0;
    let timerCount = 0, timerTotal = 0, timerMax = 0, timerMin = null;
    let lagCount = 0, lagMax = 0, longTasks = 0, longTaskMax = 0;
    let observer = null, lagTimer = null, expectedLagAt = 0, renderPending = false, renderTimer = null;
    const listeners = [];
    let panel, status, timeline, toggle;

    function player() { try { return playerProvider?.() || null; } catch { return null; } }
    function safe(method, fallback = null) { try { return player()?.[method]?.() ?? fallback; } catch { return fallback; } }
    function snapshot(override = {}) {
        const state = safe("getPlayerState", -1);
        return {
            iso: new Date().toISOString(), performanceMs: Math.round(performance.now() * 100) / 100,
            visibilityState: document.visibilityState, hidden: document.hidden, hasFocus: document.hasFocus(),
            fullscreen: !!document.fullscreenElement, videoId: safe("getVideoData")?.video_id || null,
            currentTime: safe("getCurrentTime"), duration: safe("getDuration"),
            playerState: stateNames[state] || String(state), playerStateCode: state,
            playbackRate: safe("getPlaybackRate"), ...override
        };
    }
    function queueRender() {
        if (renderPending || panel?.hidden) return;
        renderPending = true;
        renderTimer = setTimeout(() => { renderPending = false; renderTimer = null; render(); }, 250);
    }
    function record(type, detail = {}, override = {}) {
        if (!active) return;
        const entry = { type, ...snapshot(override), detail };
        if (events.length >= MAX_EVENTS) { events.shift(); droppedEvents++; }
        events.push(entry);
        if (type === "transition") transitions++;
        queueRender();
    }
    function tick(deltaMs, currentTime, duration) {
        if (!active) return;
        if (deltaMs !== null) {
            timerCount++; timerTotal += deltaMs; timerMax = Math.max(timerMax, deltaMs);
            timerMin = timerMin === null ? deltaMs : Math.min(timerMin, deltaMs);
        }
        record("progress_timer", { intervalMs: deltaMs }, { currentTime, duration });
    }
    function listen(target, name, handler) {
        target.addEventListener(name, handler);
        listeners.push([target, name, handler]);
    }
    function start() {
        if (active) return;
        events = []; markers = []; droppedEvents = 0; droppedMarkers = 0; transitions = 0;
        timerCount = 0; timerTotal = 0; timerMax = 0; timerMin = null;
        lagCount = 0; lagMax = 0; longTasks = 0; longTaskMax = 0;
        active = true;
        for (const name of ["visibilitychange", "freeze", "resume"]) {
            if (support[name]) listen(document, name, () => record("lifecycle", { event: name }));
        }
        for (const name of ["focus", "blur", "pagehide", "pageshow"]) {
            if (support[name]) listen(window, name, () => record("lifecycle", { event: name }));
        }
        expectedLagAt = performance.now() + 1000;
        lagTimer = setInterval(() => {
            const now = performance.now();
            const lag = Math.max(0, now - expectedLagAt);
            expectedLagAt = now + 1000;
            lagCount++; lagMax = Math.max(lagMax, lag);
            if (lag >= 100) record("main_thread_lag", { lagMs: Math.round(lag * 100) / 100 });
        }, 1000);
        if (support.performanceObserverLongTask) {
            try {
                observer = new PerformanceObserver(list => {
                    for (const item of list.getEntries()) {
                        longTasks++; longTaskMax = Math.max(longTaskMax, item.duration);
                        record("long_task", { durationMs: Math.round(item.duration * 100) / 100 });
                    }
                });
                observer.observe({ type: "longtask" });
            } catch { support.performanceObserverLongTask = false; observer = null; }
        }
        record("test_start", { unsupportedEvents: Object.keys(support).filter(key => !support[key]) });
        render();
    }
    function stop() {
        if (!active) return;
        record("test_stop"); active = false;
        for (const [target, name, handler] of listeners) target.removeEventListener(name, handler);
        listeners.length = 0;
        clearInterval(lagTimer); lagTimer = null;
        observer?.disconnect(); observer = null;
        clearTimeout(renderTimer); renderTimer = null; renderPending = false;
        render();
    }
    function mark(label) {
        if (!active) return;
        const entry = { label, ...snapshot() };
        if (markers.length >= MAX_MARKERS) { markers.shift(); droppedMarkers++; }
        markers.push(entry);
        record("manual_marker", { label });
    }
    function summary() {
        return {
            transitionCount: transitions, eventCount: events.length, markerCount: markers.length,
            droppedEvents, droppedMarkers,
            progressTimer: { count: timerCount, minMs: timerMin, maxMs: timerMax, averageMs: timerCount ? timerTotal / timerCount : null },
            mainThreadLag: { samples: lagCount, maxMs: lagMax },
            longTasks: { count: longTasks, maxDurationMs: longTaskMax }
        };
    }
    function exportFile(format) {
        const data = { environment, sessionId, events, markers, summary: summary() };
        let body, mime, suffix;
        if (format === "json") {
            body = JSON.stringify(data, null, 2); mime = "application/json"; suffix = "json";
        } else {
            const columns = ["iso", "performanceMs", "type", "visibilityState", "hidden", "hasFocus", "fullscreen", "videoId", "currentTime", "duration", "playerState", "playerStateCode", "playbackRate", "detail"];
            const quote = value => `"${String(value ?? "").replaceAll('"', '""')}"`;
            body = [columns.join(","), ...events.map(e => columns.map(key => quote(key === "detail" ? JSON.stringify(e.detail) : e[key])).join(","))].join("\r\n");
            mime = "text/csv;charset=utf-8"; suffix = "csv";
        }
        const url = URL.createObjectURL(new Blob([body], { type: mime }));
        const link = document.createElement("a");
        link.href = url; link.download = `yt-diagnostics-${sessionId}.${suffix}`;
        link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
    function render() {
        if (!panel || panel.hidden) return;
        const s = summary();
        status.textContent = `${active ? "Đang đo" : "Đã dừng"} · ${document.visibilityState} · ${s.transitionCount} lần chuyển bài · ${s.eventCount} sự kiện · timer max ${Math.round(s.progressTimer.maxMs)} ms · lag max ${Math.round(s.mainThreadLag.maxMs)} ms`;
        timeline.replaceChildren();
        for (const e of events.slice(-displayedEvents).reverse()) {
            const row = document.createElement("div");
            row.className = "ytdebug-row";
            row.textContent = `${e.iso.slice(11, 23)} +${e.performanceMs}ms  ${e.type}  ${e.visibilityState}  ${e.playerState}  ${JSON.stringify(e.detail)}`;
            timeline.appendChild(row);
        }
    }
    function button(label, action) {
        const b = document.createElement("button"); b.type = "button"; b.textContent = label;
        b.addEventListener("click", action); return b;
    }
    function init() {
        if (panel) return;
        toggle = button("YT Debug", () => { panel.hidden = !panel.hidden; render(); });
        toggle.className = "ytdebug-toggle";
        panel = document.createElement("section"); panel.className = "ytdebug-panel";
        panel.setAttribute("aria-label", "YouTube IFrame Diagnostics");
        const heading = document.createElement("h2"); heading.textContent = "YouTube IFrame Diagnostics";
        const close = button("Đóng", () => { panel.hidden = true; });
        status = document.createElement("p"); status.className = "ytdebug-status";
        const controls = document.createElement("div"); controls.className = "ytdebug-controls";
        controls.append(button("Start Test", start), button("Stop Test", stop),
            button("Mark Ad Start", () => mark("ad_start")), button("Mark Ad End", () => mark("ad_end")),
            button("Mark Song Start", () => mark("song_start")),
            button("Export JSON", () => exportFile("json")), button("Export CSV", () => exportFile("csv")));
        timeline = document.createElement("div"); timeline.className = "ytdebug-timeline";
        panel.append(heading, close, status, controls, timeline);
        document.body.append(toggle, panel);
        render();
    }
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init, { once: true });
    else init();
    return { record, tick, stop, setPlayerProvider(provider) { playerProvider = provider; }, get active() { return active; } };
})();
