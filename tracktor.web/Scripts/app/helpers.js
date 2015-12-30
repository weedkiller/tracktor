/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />
var Tracktor;
(function (Tracktor) {
    Tracktor.padNumber = function (n) {
        var s = n.toString();
        if (s.length === 1) {
            return "0" + s;
        }
        return s;
    };
    Tracktor.dateTime = function (s) {
        if (s == null) {
            return "-";
        }
        var date = new Date(s);
        var dateString = Tracktor.padNumber(date.getUTCDate()) + "/" +
            Tracktor.padNumber(date.getUTCMonth() + 1) + " " +
            Tracktor.padNumber(date.getUTCHours()) + ":" +
            Tracktor.padNumber(date.getUTCMinutes());
        return dateString;
    };
    Tracktor.timeSpan = function (n) {
        var sec_num = parseInt(n.toString(), 10);
        if (sec_num === 0) {
            return "-";
        }
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        var time = Tracktor.padNumber(hours) + ":" + Tracktor.padNumber(minutes);
        return time;
    };
    Tracktor.timeSpanFull = function (n) {
        var sec_num = parseInt(n.toString(), 10);
        if (sec_num === 0) {
            return "00:00:00";
        }
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        var seconds = Math.floor((sec_num - (hours * 3600) - (minutes * 60)));
        var time = Tracktor.padNumber(hours) + ":" + Tracktor.padNumber(minutes) + ":" + Tracktor.padNumber(seconds);
        return time;
    };
    Tracktor.isZero = function (n) {
        return (n === 0);
    };
})(Tracktor || (Tracktor = {}));
;
//# sourceMappingURL=helpers.js.map