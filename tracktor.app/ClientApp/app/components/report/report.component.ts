import { Component } from '@angular/core';
import { AppService } from '../../app.service';

@Component({
    selector: '[tracktor-report]',
    templateUrl: './report.component.html',
})
export class ReportComponent {

    constructor(public appService: AppService) {
    }

    public selectedProject: number = 0;
    public selectedTask: number = 0;

    public generateReport() {
        if (this.appService.reportModel) {
            this.appService.generateReport(this.appService.reportModel.selectedYear, this.appService.reportModel.selectedMonth, this.selectedProject, this.selectedTask);
        }
    }
}
