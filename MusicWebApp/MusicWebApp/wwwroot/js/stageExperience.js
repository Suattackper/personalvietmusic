// Visual layer only. The YouTube iframe remains the sole audio source.
window.anhTanStage = (() => {
    let canvas, context, observer, frame = 0, playing = false, mode = "ring";
    let stagePanel, idleTimer, pointerStart = null, dotnetRef = null, enteredFullscreen = false, queueRects = null;
    const themes = ["neon", "rain", "vinyl"];
    let theme = "neon";
    try { theme = themes.includes(localStorage.getItem("anhtan_stage_theme")) ? localStorage.getItem("anhtan_stage_theme") : "neon"; } catch { }

    function resize() {
        if (!canvas || !context) return;
        const rect = canvas.getBoundingClientRect();
        const ratio = Math.min(window.devicePixelRatio || 1, 2);
        canvas.width = Math.max(1, Math.round(rect.width * ratio));
        canvas.height = Math.max(1, Math.round(rect.height * ratio));
        context.setTransform(ratio, 0, 0, ratio, 0, 0);
        draw();
    }
    function draw() {
        if (!canvas || !context) return;
        const width = canvas.clientWidth, height = canvas.clientHeight;
        context.clearRect(0, 0, width, height);
        const reduced = document.documentElement.dataset.motionLevel !== "full";
        const count = reduced ? 28 : Math.min(64, Math.floor(width / 7));
        const time = playing ? performance.now() / 650 : 0;
        context.lineWidth = mode === "ring" ? 3 : Math.max(3, width / count * .5);
        context.lineCap = "round";
        for (let i = 0; i < count; i++) {
            const wave = .35 + .65 * Math.abs(Math.sin(i * .73 + time) * Math.sin(i * .17 - time * .69));
            const strength = playing && !reduced ? wave : .32;
            context.strokeStyle = i % 3 === 0 ? "rgba(236,72,153,.82)" : "rgba(160,113,255,.84)";
            context.beginPath();
            if (mode === "ring") {
                const angle = i / count * Math.PI * 2;
                const radius = Math.min(width, height) * .34;
                const length = 5 + strength * 23;
                const x = width / 2, y = height / 2;
                context.moveTo(x + Math.cos(angle) * radius, y + Math.sin(angle) * radius);
                context.lineTo(x + Math.cos(angle) * (radius + length), y + Math.sin(angle) * (radius + length));
            } else {
                const x = (i + .5) * width / count;
                const length = 6 + strength * Math.min(58, height * .7);
                context.moveTo(x, height / 2 - length / 2);
                context.lineTo(x, height / 2 + length / 2);
            }
            context.stroke();
        }
    }
    function tick() {
        frame = 0;
        draw();
        if (canvas && playing && !document.hidden && document.documentElement.dataset.motionLevel === "full") frame = requestAnimationFrame(tick);
    }
    function schedule() {
        if (frame) cancelAnimationFrame(frame);
        frame = 0;
        if (canvas && playing && !document.hidden && document.documentElement.dataset.motionLevel === "full") frame = requestAnimationFrame(tick);
        else draw();
    }
    document.addEventListener("visibilitychange", schedule);
    document.addEventListener("fullscreenchange", () => {
        if (enteredFullscreen && !document.fullscreenElement) {
            enteredFullscreen = false;
            dotnetRef?.invokeMethodAsync("OnStageFullscreenExited").catch(() => {});
        }
    });
    if (window.matchMedia("(hover: hover) and (pointer: fine)").matches) {
        let tiltFrame = 0, hovered = null, latest = null;
        document.addEventListener("pointermove", event => {
            const card = event.target.closest?.(".track-card");
            if (card !== hovered) {
                hovered?.style.removeProperty("--tilt-x"); hovered?.style.removeProperty("--tilt-y");
                hovered = card;
            }
            if (!card || document.documentElement.dataset.motionLevel !== "full") return;
            latest = { card, x: event.clientX, y: event.clientY };
            if (tiltFrame) return;
            tiltFrame = requestAnimationFrame(() => {
                tiltFrame = 0;
                if (!latest) return;
                const rect = latest.card.getBoundingClientRect();
                latest.card.style.setProperty("--tilt-x", `${((latest.y - rect.top) / rect.height - .5) * -4}deg`);
                latest.card.style.setProperty("--tilt-y", `${((latest.x - rect.left) / rect.width - .5) * 4}deg`);
            });
        }, { passive: true });
        document.addEventListener("pointerout", event => {
            const card = event.target.closest?.(".track-card");
            if (card && !card.contains(event.relatedTarget)) { card.style.removeProperty("--tilt-x"); card.style.removeProperty("--tilt-y"); if (hovered === card) hovered = null; }
        });
    }

    function showControls(event) {
        if (!stagePanel) return;
        if (event?.type === 'pointermove' && document.activeElement?.matches?.('[data-now-close]')) document.activeElement.blur();
        stagePanel.classList.remove("stage-idle");
        clearTimeout(idleTimer);
        idleTimer = setTimeout(() => {
            if (stagePanel && !stagePanel.contains(document.activeElement) && !stagePanel.querySelector('input:active')) stagePanel.classList.add("stage-idle");
        }, 3200);
    }
    function down(event) {
        if (event.target.closest("button, input, .lyrics-scroll, select")) return;
        pointerStart = { y: event.clientY, x: event.clientX };
    }
    function up(event) {
        if (!pointerStart) return;
        const delta = event.clientY - pointerStart.y;
        if (delta > 110 && Math.abs(event.clientX - pointerStart.x) < 90 && !stagePanel?.classList.contains("stage-mode")) stagePanel?.querySelector("[data-now-close]")?.click();
        else if (delta > 35 && !stagePanel?.classList.contains("stage-mode")) {
            stagePanel?.classList.remove('stage-swipe-back');
            void stagePanel?.offsetWidth;
            stagePanel?.classList.add('stage-swipe-back');
        }
        pointerStart = null;
    }
    return {
        captureQueue(from, to) {
            if (from === to || document.documentElement.dataset.motionLevel === "off") return;
            const items = [...document.querySelectorAll('.queue-list > .queue-item')];
            queueRects = items.map(item => ({ item, top: item.getBoundingClientRect().top }));
        },
        animateQueue() {
            if (!queueRects) return;
            for (const { item, top } of queueRects) {
                if (!item.isConnected) continue;
                const delta = top - item.getBoundingClientRect().top;
                if (Math.abs(delta) > 1) item.animate([{ transform: `translateY(${delta}px)` }, { transform: 'translateY(0)' }], { duration: 220, easing: 'cubic-bezier(.2,.8,.2,1)' });
            }
            queueRects = null;
        },
        getTheme: () => theme,
        setTheme(value) {
            theme = themes.includes(value) ? value : "neon";
            try { localStorage.setItem("anhtan_stage_theme", theme); } catch { }
        },
        mount(canvasId, selectedMode, isPlaying) {
            const next = document.getElementById(canvasId);
            if (canvas !== next) {
                observer?.disconnect();
                canvas = next;
                context = canvas?.getContext("2d") || null;
                if (canvas) { observer = new ResizeObserver(resize); observer.observe(canvas); resize(); }
            }
            mode = selectedMode === "bars" ? "bars" : "ring";
            playing = !!isPlaying;
            schedule();
        },
        stop() {
            if (frame) cancelAnimationFrame(frame);
            frame = 0; observer?.disconnect(); observer = null;
            canvas = context = null;
        },
        refresh: schedule,
        resetScroll() { stagePanel?.querySelector('.now-playing-scroll')?.scrollTo({ top: 0, behavior: 'instant' }); },
        attachPanel(id, reference) {
            const next = document.getElementById(id);
            if (stagePanel === next) return;
            this.detachPanel(); stagePanel = next; dotnetRef = reference;
            stagePanel?.addEventListener("pointerdown", down);
            stagePanel?.addEventListener("pointerup", up);
            stagePanel?.addEventListener("pointermove", showControls);
            stagePanel?.addEventListener("keydown", showControls);
            showControls();
        },
        detachPanel() {
            clearTimeout(idleTimer);
            stagePanel?.removeEventListener("pointerdown", down);
            stagePanel?.removeEventListener("pointerup", up);
            stagePanel?.removeEventListener("pointermove", showControls);
            stagePanel?.removeEventListener("keydown", showControls);
            stagePanel = null; pointerStart = null; dotnetRef = null;
        },
        async enterFullscreen() {
            try { await stagePanel?.requestFullscreen?.(); enteredFullscreen = !!document.fullscreenElement; return enteredFullscreen; } catch { return false; }
        },
        async exitFullscreen() { if (document.fullscreenElement) try { await document.exitFullscreen(); } catch { } },
        scrollLyric(id) { document.getElementById(id)?.scrollIntoView({ block: "center", behavior: document.documentElement.dataset.motionLevel === "off" ? "instant" : "smooth" }); }
    };
})();
