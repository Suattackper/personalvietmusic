// Follows Blazor's active NavLink without changing routing.
(() => {
    let pending = false;
    function update() {
        pending = false;
        const nav = document.querySelector(".sidebar-nav");
        if (!nav) return;
        const basePath = new URL(document.querySelector("base")?.href || "/", location.href).pathname;
        const path = "/" + location.pathname.slice(basePath.length).replace(/^\/+/, "");
        const derivedPath = path === "/my-playlist" || path.startsWith("/playlist/") ? "/playlists" : null;
        const links = [...nav.querySelectorAll(".sidebar-link")];
        const derived = derivedPath && links.find(link => new URL(link.href).pathname.endsWith(derivedPath));
        for (const link of links) link.classList.toggle("active-derived", link === derived);
        const active = derived || nav.querySelector(".sidebar-link.active");
        nav.classList.toggle("has-active", !!active);
        if (!active) return;
        nav.style.setProperty("--marker-y", `${active.offsetTop + active.offsetHeight * .225}px`);
        nav.style.setProperty("--marker-height", `${active.offsetHeight * .55}px`);
    }
    function schedule() {
        if (!pending) { pending = true; requestAnimationFrame(update); }
    }
    new MutationObserver(schedule).observe(document.body, { subtree: true, childList: true, attributes: true, attributeFilter: ["class"] });
    window.addEventListener("resize", schedule, { passive: true });
    window.addEventListener("popstate", schedule);
    schedule();
})();
