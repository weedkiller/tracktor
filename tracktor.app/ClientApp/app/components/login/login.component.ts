import { Component } from '@angular/core';
import { AppService } from '../../app.service';
import * as bootbox from 'bootbox';

@Component({
    selector: '[tracktor-login]',
    templateUrl: './login.component.html',
})
export class LoginComponent {
    currentUser: string;
    currentPassword: string;

    public login() {
        if (!this.currentUser || !this.currentPassword) {
            bootbox.alert("Username and password cannot be empty.");
            return;
        }

        this.appService.login(this.currentUser, this.currentPassword);
    }

    constructor(public appService: AppService)
    { }
}
