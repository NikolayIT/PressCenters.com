$(function () {
    moment.locale("bg");
    $('time').each(function (i, e) {
        var time = moment.utc($(e).attr('datetime')).local();
        $(e).html(time.format('llll'));
    });
});
