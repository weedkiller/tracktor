import { Inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpClientXsrfModule } from "@angular/common/http";
import { Subscription } from "rxjs";
import * as $ from 'jquery';
import * as bootbox from 'bootbox';
import { TimerObservable } from "rxjs/observable/TimerObservable";
import { SummaryModel, WebModel, StatusModel, EditModel, ReportModel, EntriesModel, ProjectModel } from "./app.models";

export interface IAppSettings {
    timeZones: any[];
    baseUri: string;
}

@Injectable()
export class AppService {
    public appSettings: IAppSettings;

    public summaryModel?: SummaryModel;
    public statusModel?: StatusModel;
    public editModel?: EditModel;
    public reportModel?: ReportModel;
    public entriesModel?: EntriesModel;
    public initialized: boolean;

    public modelRefreshTime: Date;
    public unrealizedContrib: number;
    public currentUser: string;
    public timeZone: string;
    public editDialogShown: boolean;
    public userDialogShown: boolean;

    private subscription: Subscription;

    constructor(private http: HttpClient, @Inject('APP_SETTINGS') appSettings: IAppSettings) {
        this.initialized = false;
        let timer = TimerObservable.create(1000, 1000);
        this.subscription = timer.subscribe(this.updateTimer);
        this.appSettings = appSettings;
    }

    private timeSinceModelRefresh(): number {
        if (this.modelRefreshTime) {
            return (new Date().valueOf() - this.modelRefreshTime.valueOf()) / 1000;
        }
        return 0;
    }

    private apiUri(action: string)
    {
        return /*this.appSettings.baseUri +*/ 'api/' + action; // XSRF not sent on abolute URIs
    }

    private apiPost(action: string, body: any, success: (data: any) => any) {
        this.http.post(this.apiUri(action), body).subscribe(success, this.error);
    }

    private apiGet(action: string, success: (data: any) => any) {
        this.http.get(this.apiUri(action)).subscribe(success, this.error);
    }

    public error = (e: any) => {
        if (e) {
            if (e.status === 401) {
                bootbox.alert("Authorization expired, please sign in.");
                this.logout();
            } else {
                bootbox.alert("Error: " + e.statusText);
            }
        }
    }

    public onUserChanged(data: any) {
        this.clearModel();
        if (data && data.email) {
            this.currentUser = data.email;
            this.timeZone = data.timeZone;
            this.loadModel();
        } else {
            this.currentUser = '';
        }
    }

    public clearModel() {
        this.summaryModel = undefined;
        this.statusModel = undefined;
        this.editModel = undefined;
        this.reportModel = undefined;
        this.entriesModel = undefined;
        this.unrealizedContrib = 0;
        this.modelRefreshTime = new Date();
    }

    public updateTimer = () => {
        // update contrib of active project by time since last refresh
        if (this.statusModel && this.statusModel.inProgress) {
            var previousUnrealizedContrib = this.unrealizedContrib;
            this.unrealizedContrib = this.timeSinceModelRefresh();
            var newContrib = this.unrealizedContrib - previousUnrealizedContrib;

            this.statusModel.latestEntry.contrib += newContrib;
            if (this.summaryModel && this.summaryModel.inProgress && this.summaryModel.projects) {
                for (var p of this.summaryModel.projects) {
                    for (var t of p.tTasks) {
                        if (t.inProgress && t.contrib) {
                            t.contrib.today += newContrib;
                            t.contrib.thisWeek += newContrib;
                            t.contrib.thisMonth += newContrib;
                        }
                    }
                }
            }
        }
    }

    public updateWebModel = (data: WebModel) => {
        if (data) {
            if (data.summaryModel) { this.summaryModel = data.summaryModel };
            if (data.statusModel) { this.statusModel = data.statusModel };
            if (data.editModel) { this.editModel = data.editModel };
            if (data.reportModel) { this.reportModel = data.reportModel };
            if (data.entriesModel) { this.entriesModel = data.entriesModel };
            this.unrealizedContrib = 0;
            this.modelRefreshTime = new Date();
        }
    }

    public handshake() {
        this.apiGet('account/handshake', (data: any) => {
            if (data && data.email) {
                this.onUserChanged(data);
            }
            this.initialized = true;
        });
    }

    public loadModel = () => {
        this.apiGet('tracktor/viewmodel', this.updateWebModel);
    }

    public login(user: string, password: string): void {
        this.apiPost('account/login', { email: user, password: password }, (data: any) => {
            if (data) {
                this.onUserChanged(data);
            }
        });
    }

    public register(code: string, user: string, password: string, timezone: string) {
        this.apiPost('account/register', { code: code, email: user, password: password, timezone: timezone }, (data: any) => {
            if (data) {
                this.onUserChanged(data);
            }
        });
    }

    public logout() {
        this.apiPost('account/logout', null, (data: any) => {
            this.onUserChanged(undefined);
        });
    }

    public startTask(taskID: number) {
        this.apiPost('task/start', { newTaskID: taskID }, this.updateWebModel);
    }

    public stopTask(taskID: number) {
        this.apiPost('task/stop', { currentTaskID: taskID }, this.updateWebModel);
    }

    public switchTask(currentTaskID: number, newTaskID: number) {
        this.apiPost('task/switch', { currentTaskID: currentTaskID, newTaskID: newTaskID }, this.updateWebModel);
    }

    public newProject(name: string) {
        this.apiPost('project/update', { Name: name }, this.loadModel);
    }

    public obsoleteProject(projectId: number, isObsolete: boolean) {
        this.apiPost('project/update', { TProjectID: projectId, IsObsolete: !isObsolete }, this.loadModel);
    };

    public renameProject(projectId: number, projectName: string) {
        this.apiPost('project/update', { TProjectID: projectId, Name: projectName }, this.loadModel);
    };

    public newTask(projectId: number, taskName: string) {
        this.apiPost('task/update', { TProjectID: projectId, TTaskID: 0, Name: taskName }, this.loadModel);
    };

    public obsoleteTask(taskId: number) {
        this.apiPost('task/update', { TTaskID: taskId, IsObsolete: true }, this.loadModel);
    };

    public renameTask(taskId: number, taskName: string) {
        this.apiPost('task/update', { TTaskID: taskId, Name: taskName }, this.loadModel);
    };

    public editEntry(entryId: number) {
        this.apiGet('entry/' + entryId, (data: WebModel) => {
            this.updateWebModel(data);
            this.editDialogShown = true;
        });
    }

    public saveEntry(tEntryID: number, startDate: string, endDate: string, isDeleted: boolean) {
        this.apiPost('entry/update', { TEntryID: tEntryID, StartDate: startDate, EndDate: endDate, IsDeleted: isDeleted }, this.loadModel);
    }

    public saveUser(timeZone: string) {
        this.apiPost('user/update', { timeZone: timeZone }, this.updateWebModel);
    }

    public generateReport(year: number, month: number, projectID: number, taskID: number) {
        this.apiGet('report?year=' + year + '&month=' + month + '&projectID=' + projectID + '&taskID=' + taskID, this.updateWebModel);
    }

    public export() {
        window.location.assign(this.appSettings.baseUri + '/home/csv');
    }
}
