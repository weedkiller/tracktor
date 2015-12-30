/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />
/// <reference path="definitions.ts" />
/// <reference path="helpers.ts" />
/// <reference path="model.ts" />
var Tracktor;
(function (Tracktor) {
    Tracktor.startTask = function (taskId) {
        Tracktor.requestData("task/start", "POST", {
            newTaskID: taskId
        }, Tracktor.updateHomeModel);
    };
    Tracktor.switchTask = function (taskId) {
        Tracktor.requestData("task/switch", "POST", {
            currentTaskID: Tracktor.statusModel.TTaskInProgress.TTaskID(),
            newTaskID: taskId
        }, Tracktor.updateHomeModel);
    };
    Tracktor.stopTask = function (taskId) {
        Tracktor.requestData("task/stop", "POST", {
            currentTaskID: taskId
        }, Tracktor.updateHomeModel);
    };
    Tracktor.saveEntry = function () {
        $("#IsDeleted").val("false");
        Tracktor.requestData("entry/update", "POST", $("#entryEditForm").serialize(), function (data) {
            Tracktor.updateHomeModel(data);
            Tracktor.refreshModel();
        });
    };
    Tracktor.deleteEntry = function () {
        $("#IsDeleted").val("true");
        Tracktor.requestData("entry/update", "POST", $("#entryEditForm").serialize(), function (data) {
            Tracktor.refreshModel();
        });
    };
    Tracktor.updateTitle = function () {
        if (Tracktor.statusModel.InProgress()) {
            document.title = "tr: " + Tracktor.statusModel.LatestEntry.ProjectName() + " / " + Tracktor.statusModel.LatestEntry.TaskName();
        }
        else {
            document.title = "tracktor (idle)";
        }
    };
    Tracktor.showReport = function () {
        $(".reportcurtain").show();
    };
    Tracktor.hideReport = function () {
        $(".reportcurtain").hide();
    };
    Tracktor.requestData = function (url, method, data, callback) {
        var token = sessionStorage.getItem(Tracktor._tokenKey);
        if (token === "") {
            alert("Authorization expired, please sign in.");
            window.location.assign(Tracktor._urlRoot + "signin");
        }
        var headers = {
            Authorization: "Bearer " + token
        };
        var settings = {
            type: method,
            url: Tracktor._urlRoot + url,
            data: data,
            headers: headers
        };
        Tracktor.disableButtons();
        $.ajax(settings).done(callback);
        Tracktor.enableButtons();
    };
    Tracktor.timerFunc = function () {
        if (Tracktor.statusModel.InProgress()) {
            Tracktor.summaryModel.Projects().forEach(function (p) {
                p.TTasks().forEach(function (t) {
                    if (t.InProgress()) {
                        var today = t.Contrib.Today();
                        var thisWeek = t.Contrib.ThisWeek();
                        var thisMonth = t.Contrib.ThisMonth();
                        t.Contrib.Today(today + Tracktor._timeTick);
                        t.Contrib.ThisWeek(thisWeek + Tracktor._timeTick);
                        t.Contrib.ThisMonth(thisMonth + Tracktor._timeTick);
                    }
                });
            });
            Tracktor.entriesModel.Entries().forEach(function (t) {
                if (t.InProgress()) {
                    var contrib = t.Contrib();
                    t.Contrib(contrib + Tracktor._timeTick);
                }
            });
            var current = Tracktor.statusModel.LatestEntry.Contrib();
            Tracktor.statusModel.LatestEntry.Contrib(current + Tracktor._timeTick);
        }
        setTimeout(Tracktor.timerFunc, 1000);
    };
    Tracktor.updateEditingEntry = function (data) {
        Tracktor.entriesModel.EditingEntry(data);
    };
    Tracktor.editEntry = function (entryId) {
        Tracktor.requestData("entry/get", "GET", { entryID: entryId }, function (data) {
            Tracktor.updateHomeModel(data);
            $("#entryEditModal").modal();
        });
    };
    Tracktor.signOut = function () {
        Tracktor.requestData("account/signout", "POST", null, function () {
            var token = sessionStorage.removeItem(Tracktor._tokenKey);
            window.location.assign(Tracktor._urlRoot);
        });
    };
    Tracktor.disableButtons = function () {
        $("button").prop("disabled", true);
    };
    Tracktor.enableButtons = function () {
        $("button").prop("disabled", false);
    };
    Tracktor.generateReport = function () {
        var data = {
            year: $("#ReportYear").val(),
            month: $("#ReportMonth").val(),
            projectID: $("#ReportProject").val(),
            taskID: $("#ReportTask").val(),
        };
        Tracktor.hideReport();
        Tracktor.requestData("report", "GET", data, function (data) {
            Tracktor.updateHomeModel(data);
            Tracktor.showReport();
        });
    };
    Tracktor.downloadCSV = function () {
        window.location.assign(Tracktor._urlRoot + "home/csv");
    };
    Tracktor.updateEditContrib = function () {
        var startDate = moment(Tracktor.editModel.Entry.StartDate());
        var endDate = Tracktor.editModel.Entry.EndDate();
        if (endDate == null || endDate === "Invalid date") {
            endDate = moment();
        }
        else {
            endDate = moment(endDate);
        }
        endDate.subtract(startDate);
        var duration = moment.duration(endDate);
        Tracktor.editModel.Entry.Contrib(duration.asSeconds());
    };
    Tracktor.newTask = function (projectId) {
        bootbox.prompt({
            message: "",
            title: "Enter name for the new task:",
            animate: false,
            callback: function (result) {
                if (result) {
                    Tracktor.requestData("task/update", "POST", {
                        TProjectID: projectId,
                        TTaskID: 0,
                        Name: result
                    }, Tracktor.refreshModel);
                }
            }
        });
    };
    Tracktor.obsoleteTask = function (taskId, taskName) {
        bootbox.confirm({
            title: "Confirm task removal",
            message: "Are you sure you'd like to remove task " + taskName + "?",
            animate: false,
            callback: function (result) {
                if (result) {
                    Tracktor.requestData("task/update", "POST", {
                        TTaskID: taskId,
                        IsObsolete: true
                    }, Tracktor.refreshModel);
                }
            }
        });
    };
    Tracktor.newProject = function () {
        bootbox.prompt({
            message: "",
            title: "Enter name for the new project:",
            animate: false,
            callback: function (result) {
                if (result) {
                    Tracktor.requestData("project/update", "POST", {
                        Name: result
                    }, Tracktor.refreshModel);
                }
            }
        });
    };
    Tracktor.obsoleteProject = function (projectId, projectName, isObsolete) {
        bootbox.confirm({
            title: "Confirm toggle obsolete",
            message: "Are you sure you'd like toggle project " + projectName + "?",
            animate: false,
            callback: function (result) {
                if (result) {
                    Tracktor.requestData("project/update", "POST", {
                        TProjectID: projectId,
                        IsObsolete: !isObsolete
                    }, Tracktor.refreshModel);
                }
            }
        });
    };
    Tracktor.renameProject = function (projectId, projectName) {
        bootbox.prompt({
            message: "",
            title: "Enter new name for the project:",
            value: projectName,
            animate: false,
            callback: function (result) {
                if (result) {
                    Tracktor.requestData("project/update", "POST", {
                        TProjectID: projectId,
                        Name: result
                    }, Tracktor.refreshModel);
                }
            }
        });
    };
    Tracktor.renameTask = function (taskId, taskName) {
        bootbox.prompt({
            message: "",
            title: "Enter new name for the task:",
            value: taskName,
            animate: false,
            callback: function (result) {
                if (result) {
                    Tracktor.requestData("task/update", "POST", {
                        TTaskID: taskId,
                        Name: result
                    }, Tracktor.refreshModel);
                }
            }
        });
    };
    Tracktor.initializeControls = function () {
        Tracktor.hideReport();
        $("#EditStartDate").datetimepicker({
            format: "DD/MM/YYYY HH:mm:ss",
            sideBySide: true,
            useCurrent: false
        });
        $("#EditStartDate").on("dp.change", function (e) {
            $("#EditEndDate").data("DateTimePicker").minDate(e.date);
            Tracktor.editModel.Entry.StartDate(moment(e.date).format("YYYY-MM-DDTHH:mm:ss.SSS"));
            Tracktor.updateEditContrib();
        });
        $("#EditEndDate").datetimepicker({
            format: "DD/MM/YYYY HH:mm:ss",
            sideBySide: true,
            useCurrent: false
        });
        $("#EditEndDate").on("dp.change", function (e) {
            if (e.date != null) {
                $("#EditStartDate").data("DateTimePicker").maxDate(e.date);
            }
            else {
                $("#EditStartDate").data("DateTimePicker").maxDate(null);
            }
            Tracktor.editModel.Entry.EndDate(moment(e.date).format("YYYY-MM-DDTHH:mm:ss.SSS"));
            Tracktor.updateEditContrib();
        });
    };
    $.ajaxSetup({
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status === 401) {
                alert("Authorization expired, please sign in.");
                window.location.assign(Tracktor._urlRoot + "signin");
            }
            else {
                alert("Error: " + textStatus + ": " + errorThrown);
                Tracktor.enableButtons();
            }
        }
    });
    Tracktor.updateUser = function () {
        Tracktor.hideReport();
        Tracktor.requestData("user/update", "POST", $("#userEditForm").serialize(), Tracktor.updateHomeModel);
    };
})(Tracktor || (Tracktor = {}));
;
//# sourceMappingURL=tracktor.js.map