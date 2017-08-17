import { Component } from '@angular/core';
import { AppService } from '../../app.service';
import * as bootbox from 'bootbox';

@Component({
    selector: '[tracktor-register]',
    templateUrl: './register.component.html',
})
export class RegisterComponent {
    public newCode: string;
    public newUser: string;
    public newPassword: string;
    public repeatPassword: string;
    public timeZone: string;

    constructor(public appService: AppService)
    { }

    public register() {
        if (!this.newCode || !this.newUser || !this.newPassword || !this.timeZone) {
            bootbox.alert("Please fill in all fields on the registration form.");
            return;
        }
        if (this.newPassword !== this.repeatPassword) {
            bootbox.alert("Password and confirmation don't match!");
            return;
        }

        this.appService.register(this.newCode, this.newUser, this.newPassword, this.timeZone);
    }
}
