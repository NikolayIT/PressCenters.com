$(function () {
    moment.locale("bg");
    $("time").each(function (i, e) {
        const dateTimeValue = $(e).attr("datetime");
        if (!dateTimeValue) {
            return;
        }

        const time = moment.utc(dateTimeValue).local();
        $(e).html(time.format("llll"));
        $(e).attr("title", $(e).attr("datetime"));
    });
});

// The navbar theme toggle. The choice is kept in a site-wide cookie for a year; the inline script at the top of
// _BaseLayout reads it back before the next page is painted.
(function () {
    const toggle = document.getElementById("theme-toggle");
    if (!toggle) {
        return;
    }

    const root = document.documentElement;
    const updateLabel = function () {
        const label = root.getAttribute("data-bs-theme") === "dark" ? "Светла тема" : "Тъмна тема";
        toggle.title = label;
        toggle.setAttribute("aria-label", label);
    };

    updateLabel();
    toggle.addEventListener("click", function () {
        const theme = root.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
        root.setAttribute("data-bs-theme", theme);
        document.cookie = "theme=" + theme + "; path=/; max-age=31536000; samesite=lax"
            + (location.protocol === "https:" ? "; secure" : "");
        updateLabel();
    });
})();
