// Shared menu behavior for language and listening-space selectors.
window.anhTanDropdown = (() => {
    const menus = new Map();
    document.addEventListener("pointerdown", event => {
        for (const [root, reference] of menus) {
            if (!root.contains(event.target)) reference.invokeMethodAsync("CloseFromOutside");
        }
    });
    document.addEventListener("keydown", event => {
        const root = event.target.closest?.(".app-dropdown");
        if (!root || !root.querySelector(".app-dropdown-menu")) return;
        if (["ArrowDown", "ArrowUp", "Home", "End"].includes(event.key)) event.preventDefault();
        if (event.key === "Escape") {
            event.preventDefault();
            event.stopImmediatePropagation();
            menus.get(root)?.invokeMethodAsync("CloseFromOutside");
            root.querySelector(".app-dropdown-trigger")?.focus();
        }
    }, true);
    return {
        register(root, reference) { menus.set(root, reference); },
        unregister(root) { menus.delete(root); },
        focusSelected(root) { root.querySelector('.app-dropdown-option[aria-selected="true"]')?.focus(); },
        focusTrigger(root) { root.querySelector(".app-dropdown-trigger")?.focus(); },
        moveFocus(root, key) {
            const options = [...root.querySelectorAll(".app-dropdown-option")];
            if (!options.length) return;
            const current = options.indexOf(document.activeElement);
            const next = key === "Home" ? 0 : key === "End" ? options.length - 1
                : key === "ArrowUp" ? (current - 1 + options.length) % options.length
                : (current + 1) % options.length;
            options[next].focus();
        }
    };
})();
