// JSInterop đơn giản cho localStorage.
// Bọc try/catch để không crash khi localStorage bị chặn (chế độ riêng tư, quota...).
window.vietmusicTheme = {
    apply: function (theme) {
        document.documentElement.setAttribute("data-theme", theme === "light" ? "light" : "dark");
    }
};

window.vietmusicStorage = {
    getItem: function (key) {
        try {
            return localStorage.getItem(key);
        } catch (e) {
            console.warn("vietmusicStorage.getItem failed:", e);
            return null;
        }
    },
    setItem: function (key, value) {
        try {
            localStorage.setItem(key, value);
            return true;
        } catch (e) {
            console.warn("vietmusicStorage.setItem failed:", e);
            return false;
        }
    },
    removeItem: function (key) {
        try {
            localStorage.removeItem(key);
            return true;
        } catch (e) {
            console.warn("vietmusicStorage.removeItem failed:", e);
            return false;
        }
    }
};
