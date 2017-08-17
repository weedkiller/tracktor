import { Component, Input } from '@angular/core';
import { AppService } from '../../app.service';

@Component({
    selector: 'tracktor-status',
    templateUrl: './status.component.html',
})
export class StatusComponent {

    public start() {
        if (this.appService.statusModel && this.appService.statusModel.latestEntry) {
            this.appService.startTask(this.appService.statusModel.latestEntry.tTaskID);
        }
    }

    public stop() {
        if (this.appService.statusModel) {
            this.appService.stopTask(this.appService.statusModel.latestEntry.tTaskID);
        }
    }

    public edit() {
        if (this.appService.statusModel && this.appService.statusModel.latestEntry) {
            this.appService.editEntry(this.appService.statusModel.latestEntry.tEntryID);
        }
    }

    constructor(public appService: AppService)
    { }
}
