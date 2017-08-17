import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { AppService } from './app.service';
import { AppComponent } from './components/app/app.component';
import { TopBarComponent } from './components/topbar/topbar.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { StatusComponent } from './components/status/status.component';
import { SummaryComponent } from './components/summary/summary.component';
import { LogComponent } from './components/log/log.component';
import { EditComponent } from './components/edit/edit.component';
import { ReportComponent } from './components/report/report.component';
import { ExportComponent } from './components/export/export.component';
import { UserComponent } from './components/user/user.component';
import { TimeSpanPipe, TimeSpanFullPipe, DateTimeFullPipe } from './app.pipes';
import { DateTimePickerDirective } from './components/datetimepicker/datetimepicker';
import { HttpClientXsrfModule } from "@angular/common/http";

@NgModule({
    declarations: [
        DateTimePickerDirective,
        AppComponent,
        TopBarComponent,
        LoginComponent,
        RegisterComponent,
        StatusComponent,
        SummaryComponent,
        LogComponent,
        EditComponent,
        ReportComponent,
        ExportComponent,
        UserComponent,
        TimeSpanPipe,
        TimeSpanFullPipe,
        DateTimeFullPipe
    ],
    imports: [
        CommonModule,
        HttpModule,
        FormsModule,
        HttpClientModule,
        HttpClientXsrfModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: '**', redirectTo: 'home' }
        ])
    ],
    providers: [AppService]
})
export class AppModuleShared {
}
