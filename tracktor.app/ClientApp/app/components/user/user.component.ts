import { Component, AfterViewChecked } from '@angular/core';
import { AppService } from '../../app.service';
import * as $ from 'jquery';

@Component({
    selector: '[tracktor-user]',
    templateUrl: './user.component.html'
})
export class UserComponent implements AfterViewChecked {
    constructor(public appService: AppService) {
    }

    ngAfterViewChecked() {
        (<any>$("#userEditModal")).modal();
    }

    public save() {
        this.appService.saveUser(this.appService.timeZone);
        this.close();
    }

    public close() {
        (<any>$("#userEditModal")).modal('hide');
        this.appService.userDialogShown = false;
    }
}
