// Visual-only helpers. Playback stays exclusively in youtubePlayer.js.
window.anhTanEffects = (() => {
    const key = "anhtan_motion_level";
    let level = "full";
    let modal = null;
    let previousFocus = null;
    let colorRequest = 0;
    let coverGhost = null;
    const reduced = window.matchMedia("(prefers-reduced-motion: reduce)");

    function apply() {
        document.documentElement.dataset.motion = level === "full" && !reduced.matches ? "on" : "off";
        document.documentElement.dataset.motionLevel = reduced.matches ? "off" : level;
    }
    try {
        const saved = localStorage.getItem(key);
        level = ["off", "light", "full"].includes(saved) ? saved : localStorage.getItem("anhtan_motion_enabled") === "false" ? "off" : "full";
    } catch { }
    apply();
    document.documentElement.dataset.tabVisible = document.hidden ? "false" : "true";
    reduced.addEventListener("change", apply);
    document.addEventListener("visibilitychange", () => {
        document.documentElement.dataset.tabVisible = document.hidden ? "false" : "true";
    });
    document.addEventListener("pointermove", event => {
        const slider = event.target.closest?.('.player-progress input[type="range"], .now-playing-progress input[type="range"]');
        if (!slider || slider.disabled) return;
        const preview = slider.parentElement?.querySelector('.seek-preview');
        if (!preview) return;
        const rect = slider.getBoundingClientRect();
        const fraction = Math.max(0, Math.min(1, (event.clientX - rect.left) / rect.width));
        const seconds = Math.floor(Number(slider.max) * fraction);
        preview.textContent = `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}`;
        preview.style.left = `${rect.left - slider.parentElement.getBoundingClientRect().left + fraction * rect.width}px`;
    }, { passive: true });

    function onKeyDown(event) {
        if (!modal) return;
        if (event.key === "Escape") {
            event.preventDefault();
            modal.querySelector("[data-now-close]")?.click();
        } else if (event.key === "Tab") {
            const items = [...modal.querySelectorAll('button:not([disabled]), input:not([disabled]), a[href]')];
            if (!items.length) return;
            const first = items[0], last = items[items.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        }
    }

    return {
        beginCoverTransition() {
            coverGhost?.remove(); coverGhost = null;
            if (document.documentElement.dataset.motionLevel !== "full") return;
            const source = document.querySelector(".player-thumb:not(.player-thumb-placeholder)");
            if (!source) return;
            const rect = source.getBoundingClientRect();
            coverGhost = source.cloneNode(true);
            Object.assign(coverGhost.style, { position: "fixed", left: `${rect.left}px`, top: `${rect.top}px`, width: `${rect.width}px`, height: `${rect.height}px`, zIndex: "10001", borderRadius: "12px", objectFit: "cover", pointerEvents: "none" });
            document.body.appendChild(coverGhost);
        },
        endCoverTransition(selector) {
            const ghost = coverGhost; coverGhost = null;
            const target = document.querySelector(selector);
            if (!ghost || !target) { ghost?.remove(); return; }
            const from = ghost.getBoundingClientRect(), to = target.getBoundingClientRect();
            if (!to.width || !to.height) { ghost.remove(); return; }
            ghost.animate([
                { transform: "translate(0,0) scale(1)", opacity: .95 },
                { transform: `translate(${to.left - from.left}px, ${to.top - from.top}px) scale(${to.width / from.width}, ${to.height / from.height})`, opacity: .2 }
            ], { duration: 270, easing: "cubic-bezier(.22,1,.36,1)" }).finished.finally(() => ghost.remove());
        },
        getEnabled: () => level !== "off",
        getLevel: () => level,
        prefersReducedMotion: () => reduced.matches,
        setEnabled(value) {
            level = value ? "full" : "off";
            try { localStorage.setItem(key, level); } catch { }
            apply();
        },
        setLevel(value) {
            level = ["off", "light", "full"].includes(value) ? value : "full";
            try { localStorage.setItem(key, level); } catch { }
            apply();
        },
        openModal(id) {
            modal = document.getElementById(id);
            if (!modal) return;
            previousFocus = document.activeElement;
            document.body.style.overflow = "hidden";
            document.addEventListener("keydown", onKeyDown);
            modal.querySelector("[data-now-close]")?.focus();
        },
        closeModal() {
            document.removeEventListener("keydown", onKeyDown);
            document.body.style.overflow = "";
            modal = null;
            previousFocus?.focus?.();
            previousFocus = null;
        },
        async setArtworkColor(url) {
            const request = ++colorRequest;
            const fallback = "125, 68, 195";
            const set = color => document.documentElement.style.setProperty("--artwork-rgb", color);
            set(fallback);
            if (!url) return;
            try {
                const image = new Image();
                image.crossOrigin = "anonymous";
                image.src = url;
                await image.decode();
                if (request !== colorRequest) return;
                const canvas = document.createElement("canvas");
                canvas.width = canvas.height = 8;
                const context = canvas.getContext("2d", { willReadFrequently: true });
                context.drawImage(image, 0, 0, 8, 8);
                const pixels = context.getImageData(0, 0, 8, 8).data;
                let r = 0, g = 0, b = 0, count = 0;
                for (let i = 0; i < pixels.length; i += 4) {
                    if (pixels[i + 3] < 200) continue;
                    r += pixels[i]; g += pixels[i + 1]; b += pixels[i + 2]; count++;
                }
                if (count && request === colorRequest) set(`${Math.round(r / count)}, ${Math.round(g / count)}, ${Math.round(b / count)}`);
            } catch { /* Cross-origin thumbnails often disallow canvas sampling. */ }
        }
    };
})();
