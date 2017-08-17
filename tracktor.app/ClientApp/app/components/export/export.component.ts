import { Component } from '@angular/core';
import { AppService } from '../../app.service';

@Component({
    selector: '[tracktor-export]',
    templateUrl: './export.component.html',
})
export class ExportComponent {

    constructor(public appService: AppService) {
    }

    public export() {
        this.appService.export();
    }
}
