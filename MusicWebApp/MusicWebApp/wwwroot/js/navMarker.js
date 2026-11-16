// Follows Blazor's active NavLink without changing routing.
(() => {
    let pending = false;
    function update() {
        pending = false;
        const nav = document.querySelector(".sidebar-nav");
        const active = nav?.querySelector(".sidebar-link.active");
        if (!nav || !active) return;
        nav.style.setProperty("--marker-y", `${active.offsetTop + active.offsetHeight * .225}px`);
        nav.style.setProperty("--marker-height", `${active.offsetHeight * .55}px`);
    }
    function schedule() {
        if (!pending) { pending = true; requestAnimationFrame(update); }
    }
    new MutationObserver(schedule).observe(document.body, { subtree: true, childList: true, attributes: true, attributeFilter: ["class"] });
    window.addEventListener("resize", schedule, { passive: true });
    schedule();
})();
