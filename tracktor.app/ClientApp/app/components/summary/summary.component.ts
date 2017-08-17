import { Component, Input } from '@angular/core';
import { AppService } from '../../app.service';
import { ProjectModel, ContribModel, TaskModel } from "../../app.models";
import * as bootbox from 'bootbox';

@Component({
    selector: '[tracktor-summary]',
    templateUrl: './summary.component.html',
})
export class SummaryComponent {

    public contrib(model: ProjectModel): ContribModel {
        return model.tTasks.reduce((sum: ContribModel, curr: TaskModel) => {
            sum.today += curr.contrib.today;
            sum.thisWeek += curr.contrib.thisWeek;
            sum.thisMonth += curr.contrib.thisMonth;
            return sum;
        }, new ContribModel());
    }

    public totalContrib(): ContribModel {
        if (this.appService.summaryModel && this.appService.summaryModel.projects) {
            return this.appService.summaryModel.projects.reduce((sum: ContribModel, curr: ProjectModel) => {
                var projectContrib = this.contrib(curr);
                sum.today += projectContrib.today;
                sum.thisWeek += projectContrib.thisWeek;
                sum.thisMonth += projectContrib.thisMonth;
                return sum;
            }, new ContribModel());
        }
        return new ContribModel();
    }

    public start(tTaskID: number) {
        this.appService.startTask(tTaskID);
    }

    public stop(tTaskID: number) {
        this.appService.stopTask(tTaskID);
    }

    public switch(tTaskID: number) {
        if (this.appService.statusModel && this.appService.statusModel.tTaskInProgress) {
            this.appService.switchTask(this.appService.statusModel.tTaskInProgress.tTaskID, tTaskID);
        }
    }

    public newProject() {
        bootbox.prompt(<BootboxPromptOptions>{
            title: "Enter name for the new project:",
            animate: false,
            callback: (result: any) => {
                if (result) {
                    this.appService.newProject(result);
                }
            }
        });
    }

    public renameProject(projectId: number, projectName: string) {
        bootbox.prompt({
            title: "Enter new name for the project:",
            value: projectName,
            animate: false,
            callback: (result: any) => {
                if (result) {
                    this.appService.renameProject(projectId, result);
                }
            }
        });
    }

    public obsoleteProject(projectId: number, projectName: string, isObsolete: boolean) {
        bootbox.confirm({
            title: "Confirm toggle obsolete",
            message: "Are you sure you'd like toggle project " + projectName + "?",
            animate: false,
            callback: (result: any) => {
                if (result) {
                    this.appService.obsoleteProject(projectId, isObsolete);
                }
            }
        });
    }

    public newTask(projectId: number) {
        bootbox.prompt({
            title: "Enter name for the new task:",
            animate: false,
            callback: (result: any) => {
                if (result) {
                    this.appService.newTask(projectId, result);
                }
            }
        });
    }

    public renameTask(taskId: number, taskName: string) {
        bootbox.prompt({
            title: "Enter new name for the task:",
            value: taskName,
            animate: false,
            callback: (result: any) => {
                if (result) {
                    this.appService.renameTask(taskId, result);
                }
            }
        });
    }

    public obsoleteTask(taskId: number, taskName: string) {
        bootbox.confirm({
            title: "Confirm task removal",
            message: "Are you sure you'd like to remove task " + taskName + "?",
            animate: false,
            callback: (result: any) => {
                if (result) {
                    this.appService.obsoleteTask(taskId);
                }
            }
        });
    }

    constructor(public appService: AppService) {
    }
}
