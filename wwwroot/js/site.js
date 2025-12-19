(() => {
    const key = "cm_theme";
    const btn = document.getElementById("themeToggle");

    const apply = (mode) => {
        // Ton thème (variables CSS)
        document.documentElement.classList.toggle("cm-light", mode === "light");

        // IMPORTANT : Bootstrap suit le thème aussi
        document.documentElement.setAttribute("data-bs-theme", mode === "light" ? "light" : "dark");

        // Icône du bouton
        if (btn) {
            btn.innerHTML = mode === "light"
                ? '<i class="bi bi-sun"></i>'
                : '<i class="bi bi-moon-stars"></i>';
        }
    };

    const saved = localStorage.getItem(key) || "dark";
    apply(saved);

    btn?.addEventListener("click", () => {
        const next = document.documentElement.classList.contains("cm-light") ? "dark" : "light";
        localStorage.setItem(key, next);
        apply(next);
    });
})();
