import { Component } from '@angular/core';
import { AppService } from '../../app.service';

@Component({
    selector: '[tracktor-log]',
    templateUrl: './log.component.html',
})
export class LogComponent {

    constructor(public appService: AppService) {
    }

    public edit(tEntryID: number) {
        this.appService.editEntry(tEntryID);
    }
}
