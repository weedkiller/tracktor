/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="typings/moment/moment.d.ts" />
/// <reference path="typings/bootstrap/bootstrap.d.ts" />
/// <reference path="typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="typings/bootbox/bootbox.d.ts" />
var padNumber = function (n) {
    var s = n.toString();
    if (s.length === 1) {
        return "0" + s;
    }
    return s;
};
var dateTime = function (s) {
    if (s == null) {
        return "-";
    }
    var date = new Date(s);
    var dateString = padNumber(date.getUTCDate()) + "/" + padNumber(date.getUTCMonth() + 1) + " " + padNumber(date.getUTCHours()) + ":" + padNumber(date.getUTCMinutes());
    return dateString;
};
var timeSpan = function (n) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "-";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var time = padNumber(hours) + ":" + padNumber(minutes);
    return time;
};
var timeSpanFull = function (n) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "00:00:00";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = Math.floor((sec_num - (hours * 3600) - (minutes * 60)));
    var time = padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
    return time;
};
var isZero = function (n) {
    return (n == 0);
};
var projectMapping = function (data) {
    var self = this;
    ko.mapping.fromJS(data, {}, this);
    self.Contrib = {
        Today: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.Today();
            }, 0);
        }, self),
        ThisWeek: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.ThisWeek();
            }, 0);
        }, self),
        ThisMonth: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.ThisMonth();
            }, 0);
        }, self)
    };
};
var startTask = function (taskId) {
    requestData("task/start", "POST", {
        newTaskID: taskId
    }, updateHomeModel);
};
var switchTask = function (taskId) {
    requestData("task/switch", "POST", {
        currentTaskID: statusModel.TTaskInProgress.TTaskID,
        newTaskID: taskId
    }, updateHomeModel);
};
var stopTask = function (taskId) {
    requestData("task/stop", "POST", {
        currentTaskID: taskId
    }, updateHomeModel);
};
var saveEntry = function () {
    $('#IsDeleted').val("false");
    requestData("entry/update", "POST", $("#entryEditForm").serialize(), function (data) {
        updateHomeModel(data);
        refreshModel();
    });
};
var deleteEntry = function () {
    $('#IsDeleted').val("true");
    requestData("entry/update", "POST", $("#entryEditForm").serialize(), function (data) {
        refreshModel();
    });
};
var rootModel = function (data) {
    ko.mapping.fromJS(data, rootMapping, this);
};
var rootMapping = {
    'Projects': {
        create: function (options) {
            return new projectMapping(options.data);
        }
    },
    create: function (options) {
        var viewModel = new rootModel(options.data);
        viewModel.Contrib = {
            Today: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.Today();
                }, 0);
            }, viewModel),
            ThisWeek: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.ThisWeek();
                }, 0);
            }, viewModel),
            ThisMonth: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.ThisMonth();
                }, 0);
            }, viewModel)
        };
        return viewModel;
    }
};
var summaryModel;
var statusModel;
var entriesModel;
var reportModel;
var editModel;
var viewOptions = {
    showObsolete: ko.observable(false)
};
var updateTitle = function () {
    if (statusModel.InProgress()) {
        document.title = "tr: " + statusModel.LatestEntry.ProjectName() + " / " + statusModel.LatestEntry.TaskName();
    }
    else {
        document.title = "tracktor (idle)";
    }
};
var bindHomeModel = function (data) {
    summaryModel = ko.mapping.fromJS(data.SummaryModel, rootMapping);
    ko.applyBindings(summaryModel, document.getElementById('SummaryModel'));
    statusModel = ko.mapping.fromJS(data.StatusModel);
    ko.applyBindings(statusModel, document.getElementById('StatusModel'));
    entriesModel = ko.mapping.fromJS(data.EntriesModel);
    ko.applyBindings(entriesModel, document.getElementById('EntriesModel'));
    reportModel = ko.mapping.fromJS(data.ReportModel);
    ko.applyBindings(reportModel, document.getElementById('ReportModel'));
    editModel = ko.mapping.fromJS(data.EditModel);
    ko.applyBindings(editModel, document.getElementById('EditModel'));
    updateTitle();
    $('.curtain').removeClass('curtain');
    $('.nocurtain').remove();
    timerFunc();
};
var updateHomeModel = function (data) {
    if (data.SummaryModel) {
        ko.mapping.fromJS(data.SummaryModel, rootMapping, summaryModel);
    }
    if (data.StatusModel) {
        ko.mapping.fromJS(data.StatusModel, {}, statusModel);
        updateTitle();
    }
    if (data.EntriesModel) {
        ko.mapping.fromJS(data.EntriesModel, {}, entriesModel);
    }
    if (data.ReportModel) {
        ko.mapping.fromJS(data.ReportModel, {}, reportModel);
    }
    if (data.EditModel) {
        $("#EditStartDate").data("DateTimePicker").maxDate(false);
        $("#EditEndDate").data("DateTimePicker").minDate(false);
        $("#EditEndDate").data("DateTimePicker").date(null);
        ko.mapping.fromJS(data.EditModel, {}, editModel);
        // update datepickers
        $("#EditStartDate").data("DateTimePicker").date(moment(editModel.Entry.StartDate()));
        if (editModel.Entry.EndDate() != null) {
            $("#EditEndDate").data("DateTimePicker").date(moment(editModel.Entry.EndDate()));
        }
    }
};
var refreshModel = function () {
    hideReport();
    requestData("viewmodel", "GET", {}, updateHomeModel);
};
var _urlRoot = "";
var showReport = function () {
    $(".reportcurtain").show();
};
var hideReport = function () {
    $(".reportcurtain").hide();
};
var initializeModel = function (urlRoot) {
    _urlRoot = urlRoot;
    hideReport();
    requestData("viewmodel", "GET", {}, bindHomeModel);
};
var requestData = function (url, method, data, callback) {
    var tokenKey = "TokenKey";
    var token = sessionStorage.getItem(tokenKey);
    if (token === "") {
        alert("Authorization expired, please sign in.");
        window.location.assign(_urlRoot + "signin");
    }
    var headers = {
        Authorization: 'Bearer ' + token
    };
    var settings = {
        type: method,
        url: _urlRoot + url,
        data: data,
        headers: headers
    };
    disableButtons();
    $.ajax(settings).done(callback);
    enableButtons();
};
var timeTick = 1;
$.ajaxSetup({
    error: function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status == 401) {
            alert("Authorization expired, please sign in.");
            window.location.assign(_urlRoot + "signin");
        }
        else {
            alert("Error: " + textStatus + ": " + errorThrown);
            enableButtons();
        }
    }
});
var timerFunc = function () {
    if (statusModel.InProgress()) {
        summaryModel.Projects().forEach(function (p) {
            p.TTasks().forEach(function (t) {
                if (t.InProgress()) {
                    var today = t.Contrib.Today();
                    var thisWeek = t.Contrib.ThisWeek();
                    var thisMonth = t.Contrib.ThisMonth();
                    t.Contrib.Today(today + timeTick);
                    t.Contrib.ThisWeek(thisWeek + timeTick);
                    t.Contrib.ThisMonth(thisMonth + timeTick);
                }
            });
        });
        entriesModel.Entries().forEach(function (t) {
            if (t.InProgress()) {
                var contrib = t.Contrib();
                t.Contrib(contrib + timeTick);
            }
        });
        var current = statusModel.LatestEntry.Contrib();
        statusModel.LatestEntry.Contrib(current + timeTick);
    }
    setTimeout(timerFunc, 1000);
};
var updateEditingEntry = function (data) {
    entriesModel.EditingEntry(data);
};
var editEntry = function (entryId) {
    requestData("entry/get", "GET", { entryID: entryId }, function (data) {
        updateHomeModel(data);
        $("#entryEditModal").modal();
    });
};
var signOut = function () {
    requestData("account/signout", "POST", null, function () {
        var tokenKey = "TokenKey";
        var token = sessionStorage.removeItem(tokenKey);
        window.location.assign(_urlRoot);
    });
};
var disableButtons = function () {
    $("button").prop("disabled", true);
};
var enableButtons = function () {
    $("button").prop("disabled", false);
};
var generateReport = function () {
    var data = {
        year: $('#ReportYear').val(),
        month: $('#ReportMonth').val(),
        projectID: $('#ReportProject').val()
    };
    hideReport();
    requestData("report", "GET", data, function (data) {
        updateHomeModel(data);
        showReport();
    });
};
var downloadCSV = function () {
    window.location.assign(_urlRoot + "/home/csv");
};
var updateEditContrib = function () {
    var startDate = moment(editModel.Entry.StartDate());
    var endDate = editModel.Entry.EndDate();
    if (endDate == null || endDate == "Invalid date") {
        endDate = moment();
    }
    else {
        endDate = moment(endDate);
    }
    endDate.subtract(startDate);
    var duration = moment.duration(endDate);
    editModel.Entry.Contrib(duration.asSeconds());
};
var newTask = function (projectId) {
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
var obsoleteTask = function (taskId, taskName) {
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
var newProject = function () {
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
var obsoleteProject = function (projectId, projectName) {
    bootbox.confirm({
        title: "Confirm project removal",
        message: "Are you sure you'd like to remove project " + projectName + "?",
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("project/update", "POST", {
                    TProjectID: projectId,
                    IsObsolete: true
                }, refreshModel);
            }
        }
    });
};
var renameProject = function (projectId, projectName) {
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
var renameTask = function (taskId, taskName) {
    bootbox.prompt({
        message: "",
        title: "Enter new name for the task:",
        value: taskName,
        animate: false,
        callback: function (result) {
            if (result) {
                requestData("project/update", "POST", {
                    TTaskID: taskId,
                    Name: result
                }, refreshModel);
            }
        }
    });
};
var initializeComponents = function () {
    $("#EditStartDate").datetimepicker({
        format: 'DD/MM/YYYY HH:mm:ss',
        sideBySide: true,
        useCurrent: false
    });
    $("#EditStartDate").on("dp.change", function (e) {
        $('#EditEndDate').data("DateTimePicker").minDate(e.date);
        editModel.Entry.StartDate(moment(e.date).toISOString());
        updateEditContrib();
    });
    $("#EditEndDate").datetimepicker({
        format: 'DD/MM/YYYY HH:mm:ss',
        sideBySide: true,
        useCurrent: false
    });
    $("#EditEndDate").on("dp.change", function (e) {
        if (e.date != null) {
            $('#EditStartDate').data("DateTimePicker").maxDate(e.date);
        }
        else {
            $('#EditStartDate').data("DateTimePicker").maxDate(false);
        }
        editModel.Entry.EndDate(moment(e.date).toISOString());
        updateEditContrib();
    });
};
//# sourceMappingURL=tracktor.js.map