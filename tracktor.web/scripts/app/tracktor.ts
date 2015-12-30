/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />

/// <reference path="definitions.ts" />
/// <reference path="helpers.ts" />
/// <reference path="model.ts" />

module Tracktor {
    export var startTask = function (taskId: number) {
        requestData("task/start", "POST", {
            newTaskID: taskId
        }, updateHomeModel);
    };

    export var switchTask = function (taskId: number) {
        requestData("task/switch", "POST", {
            currentTaskID: statusModel.TTaskInProgress.TTaskID(),
            newTaskID: taskId
        }, updateHomeModel);
    };

    export var stopTask = function (taskId: number) {
        requestData("task/stop", "POST", {
            currentTaskID: taskId
        }, updateHomeModel);
    };

    export var saveEntry = function () {
        $("#IsDeleted").val("false");
        requestData("entry/update", "POST",
            $("#entryEditForm").serialize(),
            function (data) {
                updateHomeModel(data);
                refreshModel();
            });
    };

    export var deleteEntry = function () {
        $("#IsDeleted").val("true");
        requestData("entry/update", "POST",
            $("#entryEditForm").serialize(),
            function (data) {
                refreshModel();
            });
    };

    export var updateTitle = function () {
        if (statusModel.InProgress()) {
            document.title = "tr: " + statusModel.LatestEntry.ProjectName() + " / " + statusModel.LatestEntry.TaskName();
        } else {
            document.title = "tracktor (idle)";
        }
    };

    export var showReport = function () {
        $(".reportcurtain").show();
    };

    export var hideReport = function () {
        $(".reportcurtain").hide();
    };

    export var requestData = function (url: string, method: string, data: any, callback: (any: any) => void) {
        var token = sessionStorage.getItem(_tokenKey);

        if (token === "") {
            alert("Authorization expired, please sign in.");
            window.location.assign(_urlRoot + "signin");
        }
        var headers: { [key: string]: any; } = {
            Authorization: "Bearer " + token
        };

        var settings: JQueryAjaxSettings = {
            type: method,
            url: _urlRoot + url,
            data: data,
            headers: headers
        };

        disableButtons();
        $.ajax(settings).done(callback);
        enableButtons();
    };

    export var timerFunc = function () {
        if (statusModel.InProgress()) {
            summaryModel.Projects().forEach(function (p) {
                p.TTasks().forEach(function (t) {
                    if (t.InProgress()) {
                        var today = t.Contrib.Today();
                        var thisWeek = t.Contrib.ThisWeek();
                        var thisMonth = t.Contrib.ThisMonth();
                        t.Contrib.Today(today + _timeTick);
                        t.Contrib.ThisWeek(thisWeek + _timeTick);
                        t.Contrib.ThisMonth(thisMonth + _timeTick);
                    }
                });
            });

            entriesModel.Entries().forEach(function (t) {
                if (t.InProgress()) {
                    var contrib = t.Contrib();
                    t.Contrib(contrib + _timeTick);
                }
            });

            var current = statusModel.LatestEntry.Contrib();
            statusModel.LatestEntry.Contrib(current + _timeTick);
        }
        setTimeout(timerFunc, 1000);
    };

    export var updateEditingEntry = function (data) {
        entriesModel.EditingEntry(data);
    };

    export var editEntry = function (entryId) {
        requestData("entry/get", "GET", { entryID: entryId }, function (data) {
            updateHomeModel(data);
            $("#entryEditModal").modal();
        });
    };

    export var signOut = function () {
        requestData("account/signout", "POST", null, function () {
            var token = sessionStorage.removeItem(_tokenKey);

            window.location.assign(_urlRoot);
        });
    };

    export var disableButtons = function () {
        $("button").prop("disabled", true);
    };

    export var enableButtons = function () {
        $("button").prop("disabled", false);
    };

    export var generateReport = function () {
        var data = {
            year: $("#ReportYear").val(),
            month: $("#ReportMonth").val(),
            projectID: $("#ReportProject").val(),
            taskID: $("#ReportTask").val(),
        };
        hideReport();
        requestData("report", "GET", data,
            function (data) {
                updateHomeModel(data);
                showReport();
            });
    };

    export var downloadCSV = function () {
        window.location.assign(_urlRoot + "home/csv");
    };

    export var updateEditContrib = function () {
        var startDate = moment(editModel.Entry.StartDate());
        var endDate = editModel.Entry.EndDate();
        if (endDate == null || endDate === "Invalid date") {
            endDate = moment();
        } else {
            endDate = moment(endDate);
        }
        endDate.subtract(startDate);
        var duration = moment.duration(endDate);
        editModel.Entry.Contrib(duration.asSeconds());
    };

    export var newTask = function (projectId: number) {
        bootbox.prompt({
            message: "",
            title: "Enter name for the new task:",
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("task/update", "POST", {
                        TProjectID: projectId,
                        TTaskID: 0,
                        Name: result
                    }, refreshModel);
                }
            }
        });
    };

    export var obsoleteTask = function (taskId: number, taskName: string) {
        bootbox.confirm({
            title: "Confirm task removal",
            message: "Are you sure you'd like to remove task " + taskName + "?",
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("task/update", "POST", {
                        TTaskID: taskId,
                        IsObsolete: true
                    }, refreshModel);
                }
            }
        });
    };

    export var newProject = function () {
        bootbox.prompt({
            message: "",
            title: "Enter name for the new project:",
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("project/update", "POST", {
                        Name: result
                    }, refreshModel);
                }
            }
        });
    };

    export var obsoleteProject = function (projectId: number, projectName: string, isObsolete: boolean) {
        bootbox.confirm({
            title: "Confirm toggle obsolete",
            message: "Are you sure you'd like toggle project " + projectName + "?",
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("project/update", "POST", {
                        TProjectID: projectId,
                        IsObsolete: !isObsolete
                    }, refreshModel);
                }
            }
        });
    };

    export var renameProject = function (projectId: number, projectName: string) {
        bootbox.prompt({
            message: "",
            title: "Enter new name for the project:",
            value: projectName,
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("project/update", "POST", {
                        TProjectID: projectId,
                        Name: result
                    }, refreshModel);
                }
            }
        });
    };

    export var renameTask = function (taskId: number, taskName: string) {
        bootbox.prompt({
            message: "",
            title: "Enter new name for the task:",
            value: taskName,
            animate: false,
            callback: function (result) {
                if (result) {
                    requestData("task/update", "POST", {
                        TTaskID: taskId,
                        Name: result
                    }, refreshModel);
                }
            }
        });
    };

    export var initializeControls = function () {
        hideReport();

        $("#EditStartDate").datetimepicker({
            format: "DD/MM/YYYY HH:mm:ss",
            sideBySide: true,
            useCurrent: false
        });

        $("#EditStartDate").on("dp.change", function (e) {
            $("#EditEndDate").data("DateTimePicker").minDate(e.date);
            editModel.Entry.StartDate(moment(e.date).format("YYYY-MM-DDTHH:mm:ss.SSS"));
            updateEditContrib();
        });

        $("#EditEndDate").datetimepicker({
            format: "DD/MM/YYYY HH:mm:ss",
            sideBySide: true,
            useCurrent: false
        });


        $("#EditEndDate").on("dp.change", function (e) {
            if (e.date != null) {
                $("#EditStartDate").data("DateTimePicker").maxDate(e.date);
            } else {
                $("#EditStartDate").data("DateTimePicker").maxDate(null);
            }
            editModel.Entry.EndDate(moment(e.date).format("YYYY-MM-DDTHH:mm:ss.SSS"));
            updateEditContrib();
        });
    };

    $.ajaxSetup({
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                alert("Authorization expired, please sign in.");
                window.location.assign(_urlRoot + "signin");
            } else {
                alert("Error: " + textStatus + ": " + errorThrown);
                enableButtons();
            }
        }
    });

    export var updateUser = function () {
        hideReport();
        requestData("user/update", "POST",
            $("#userEditForm").serialize(),
            updateHomeModel);
    };
};
