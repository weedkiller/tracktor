import { Component, OnInit } from '@angular/core';
import { AppService } from '../../app.service';

@Component({
    selector: 'tracktor-app',
    templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
    public year: number;

    constructor(public appService: AppService) {
        this.year = new Date().getFullYear();
    }

    ngOnInit(): void {
        this.appService.handshake();
    }
}
