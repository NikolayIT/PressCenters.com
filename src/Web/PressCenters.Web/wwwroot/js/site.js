$(function () {
    moment.locale("bg");
    $("time").each(function (i, e) {
        const time = moment.utc($(e).attr("datetime")).local();
        $(e).html(time.format("llll"));
        $(e).attr("title", $(e).attr("datetime"));
    });
});
