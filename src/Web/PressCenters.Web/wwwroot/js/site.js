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

    if (document.cookie.includes('darkMode=true')) {
        document.getElementById("dark-mode").checked = 1;
    }
});

function toggleDarkMode() {
    document.documentElement.classList.toggle('dark-mode');
    if (document.cookie.includes('darkMode=true')) document.cookie = 'darkMode=false';
    else document.cookie = 'darkMode=true';
}
