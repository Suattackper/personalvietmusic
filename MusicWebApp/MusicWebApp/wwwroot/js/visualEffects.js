// Visual-only helpers. Playback stays exclusively in youtubePlayer.js.
window.anhTanEffects = (() => {
    const key = "anhtan_motion_enabled";
    let enabled = true;
    let modal = null;
    let previousFocus = null;
    let colorRequest = 0;
    const reduced = window.matchMedia("(prefers-reduced-motion: reduce)");

    function apply() {
        document.documentElement.dataset.motion = enabled && !reduced.matches ? "on" : "off";
    }
    try { enabled = localStorage.getItem(key) !== "false"; } catch { }
    apply();
    document.documentElement.dataset.tabVisible = document.hidden ? "false" : "true";
    reduced.addEventListener("change", apply);
    document.addEventListener("visibilitychange", () => {
        document.documentElement.dataset.tabVisible = document.hidden ? "false" : "true";
    });

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
        getEnabled: () => enabled,
        setEnabled(value) {
            enabled = !!value;
            try { localStorage.setItem(key, String(enabled)); } catch { }
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
