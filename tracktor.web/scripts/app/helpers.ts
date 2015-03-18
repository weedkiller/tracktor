/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />

var padNumber = function (n: number) {
    var s = n.toString();
    if (s.length === 1) {
        return "0" + s;
    }
    return s;
};

var dateTime = function (s: string) {
    if (s == null) {
        return "-";
    }
    var date = new Date(s);
    var dateString =
        padNumber(date.getUTCDate()) + "/" +
        padNumber(date.getUTCMonth() + 1) + " " +
        padNumber(date.getUTCHours()) + ":" +
        padNumber(date.getUTCMinutes());
    return dateString;
};

var timeSpan = function (n: number) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "-";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);

    var time = padNumber(hours) + ":" + padNumber(minutes);
    return time;
};

var timeSpanFull = function (n: number) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "00:00:00";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = Math.floor((sec_num - (hours * 3600) - (minutes * 60)));

    var time = padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
    return time;
};

var isZero = function (n: number) {
    return (n === 0);
};
