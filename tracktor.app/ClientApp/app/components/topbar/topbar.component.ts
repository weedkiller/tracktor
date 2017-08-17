import { Component } from '@angular/core';
import { AppService } from '../../app.service';

@Component({
    selector: 'tracktor-topbar',
    templateUrl: './topbar.component.html',
})
export class TopBarComponent {
    public logout() {
        this.appService.logout();
    }

    public user() {
        this.appService.userDialogShown = true;
    }

    constructor(public appService: AppService) {
    }
}
