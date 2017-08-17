import { Pipe, PipeTransform } from '@angular/core';
import { padNumber } from "./helpers";

@Pipe({ name: 'timeSpan' })
export class TimeSpanPipe implements PipeTransform {
    transform(n: number): string {
        var sec_num = parseInt(n.toString(), 10);
        if (sec_num === 0) {
            return "-";
        }
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);

        var time = padNumber(hours) + ":" + padNumber(minutes);
        return time;
    }
}

@Pipe({ name: 'timeSpanFull' })
export class TimeSpanFullPipe implements PipeTransform {
    transform(n: number): string {
        var sec_num = parseInt(n.toString(), 10);
        if (sec_num === 0) {
            return "00:00:00";
        }
        var hours = Math.floor(sec_num / 3600);
        var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
        var seconds = Math.floor((sec_num - (hours * 3600) - (minutes * 60)));

        var time = padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
        return time;
    }
}

@Pipe({ name: 'dateTimeFull' })
export class DateTimeFullPipe implements PipeTransform {
    transform(s: string) {
        if (s == null) {
            return "-";
        }
        var date = new Date(s);
        var dateString =
            padNumber(date.getDate()) + "/" +
            padNumber(date.getMonth() + 1) + " " +
            padNumber(date.getHours()) + ":" +
            padNumber(date.getMinutes());
        return dateString;
    };
}