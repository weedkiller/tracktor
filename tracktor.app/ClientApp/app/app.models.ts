export class ContribModel {
    public thisMonth: number;
    public thisWeek: number;
    public today: number;

    constructor() {
        this.today = 0;
        this.thisWeek = 0;
        this.thisMonth = 0;
    }
}

export class TaskModel {
    public displayOrder: number;
    public inProgress: boolean;
    public isObsolete: boolean;
    public name: string;
    public tProjectID: number;
    public tTaskID: number;
    public contrib: ContribModel;
}

export class ProjectModel {
    public displayOrder: number;
    public inProgress: boolean;
    public isObsolete: boolean;
    public name: string;
    public tProjectID: number;
    public tTasks: TaskModel[];
}

export class SummaryModel {
    public inProgress: boolean;
    public projects: ProjectModel[];
}

export class EntryModel {
    public contrib: number;
    public endDate: string;
    public inProgress: boolean;
    public isDeleted: boolean;
    public projectName: string;
    public startDate: string;
    public tEntryID: number;
    public tTaskID: number;
    public taskName: string;
}

export class StatusModel {
    public inProgress: boolean;
    public latestEntry: EntryModel;
    public tTaskInProgress: TaskModel;
}

export class EntriesModel {
    public entries: EntryModel[];
}

export class ContextModel {
    public userID: number;
    public uTCOffset: number;
}

export class EditModel {
    public entry: EntryModel;
}

export class ReportDay {
    public day: number;
    public contrib: number;
    public inFocus: boolean;
}

export class ReportWeek {
    public contrib: number;
    public days: ReportDay[];
}

export class ReportTaskContrib {
    public taskName: string;
    public contrib: number;
}

export class ReportProjectContrib {
    public projectName: string;
    public taskContribs: ReportTaskContrib[];
    public contrib: number;
}

export class KeyValuePair {
    public key: number;
    public value: string;
}

export class ReportModel {
    public projects: KeyValuePair[];
    public tasks: KeyValuePair[];
    public years: number[];
    public months: number[];
    public report: ReportWeek[];
    public projectContribs: ReportProjectContrib[];
    public contrib: number;
    public selectedYear: number;
    public selectedMonth: number;
}

export class WebModel {
    public summaryModel?: SummaryModel;
    public statusModel?: StatusModel;
    public editModel?: EditModel;
    public reportModel?: ReportModel;
    public entriesModel?: EntriesModel;
}