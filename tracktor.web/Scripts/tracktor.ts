/// <reference path="typings/jquery/jquery.d.ts" />
/// <reference path="typings/knockout.mapping/knockout.mapping.d.ts" />

var padNumber = function (n: number) {
    var s = n.toString();
    if (s.length === 1) {
        return "0" + s;
    }
    return s;
};

var dateTime = function (s: string) {
    if (s == null) {
        return "-";
    }
    var date = new Date(s);
    var dateString =
        padNumber(date.getDate()) + "/" +
        padNumber(date.getMonth() + 1) + " " +
        padNumber(date.getHours()) + ":" +
        padNumber(date.getMinutes());
    return dateString;
}

var timeSpan = function (n: number) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "-";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);

    var time = padNumber(hours) + ":" + padNumber(minutes);
    return time;
}

var timeSpanFull = function (n: number) {
    var sec_num = parseInt(n.toString(), 10);
    if (sec_num === 0) {
        return "00:00:00";
    }
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = Math.floor((sec_num - (hours * 3600) - (minutes * 60)));

    var time = padNumber(hours) + ":" + padNumber(minutes) + ":" + padNumber(seconds);
    return time;
}

var isZero = function (n: number) {
    return (n == 0);
}

var projectMapping = function (data) {
    var self = this;
    ko.mapping.fromJS(data, {}, this);
    self.Contrib = {
        Today: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.Today();
            }, 0)
        }, self),
        ThisWeek: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.ThisWeek();
            }, 0)
        }, self),
        ThisMonth: ko.pureComputed(function () {
            return self.TTasks().reduce(function (pv, vc) {
                return pv + vc.Contrib.ThisMonth();
            }, 0)
        }, self)
    };
};

var startTask = function (taskId: number) {
    requestData("api/Tracktor/StartTask", "POST", {
        newTaskID: taskId
    }, updateHomeModel);
}

var switchTask = function (taskId: number) {
    requestData("api/Tracktor/SwitchTask", "POST", {
        currentTaskID: statusModel.TTaskInProgress.TTaskID,
        newTaskID: taskId
    }, updateHomeModel);
}

var stopTask = function (taskId: number) {
    requestData("api/Tracktor/StopTask", "POST", {
        currentTaskID: taskId
    }, updateHomeModel);
}

var saveEntry = function () {
    $('#IsDeleted').val("false");
    requestData("api/Tracktor/UpdateEntry", "POST",
        $("#entryEditForm").serialize(),
        updateHomeModel);
}

var deleteEntry = function () {
    $('#IsDeleted').val("true");
    requestData("api/Tracktor/UpdateEntry", "POST",
        $("#entryEditForm").serialize(),
        function (data) {
            refreshModel();
        });
}

var rootModel = function (data) {
    ko.mapping.fromJS(data, rootMapping, this)
};

var editingEntry: any = ko.mapping.fromJS({
    TaskName: "-",
    ProjectName: "-",
    Contrib: 0,
    StartDate: 0,
    EndDate: 0
});

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
                }, 0)
            }, viewModel),
            ThisWeek: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.ThisWeek();
                }, 0)
            }, viewModel),
            ThisMonth: ko.pureComputed(function () {
                return viewModel.Projects().reduce(function (pv, vc) {
                    return pv + vc.Contrib.ThisMonth();
                }, 0)
            }, viewModel)
        };
        viewModel.EditingEntry = ko.observable(editingEntry);
        return viewModel;
    }
};

var summaryModel;
var statusModel;
var entriesModel;
var reportModel;
var editModel;

var updateTitle = function () {
    if (statusModel.InProgress()) {
        document.title = "tr: " + statusModel.LatestEntry.ProjectName() + " / " + statusModel.LatestEntry.TaskName();
    } else {
        document.title = "tracktor (idle)";
    }
}

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
        ko.mapping.fromJS(data.EditModel, {}, editModel);
    }
};

var refreshModel = function () {
    requestData("api/Tracktor/GetModel", "GET", {}, updateHomeModel);
};

var _urlRoot = "";

var initializeModel = function (urlRoot: string) {
    _urlRoot = urlRoot;
    $(".reportcurtain").hide();
    requestData("api/Tracktor/GetModel", "GET", {}, bindHomeModel);
};

var requestData = function (url: string, method: string, data: any, callback: (any) => void) {
    var tokenKey = "TokenKey";
    var token = sessionStorage.getItem(tokenKey);

    if (token === "") {
        alert("Authorization expired, please sign in.");
        window.location.assign(_urlRoot + "Home/SignIn");
    }
    var headers: { [key: string]: any; } = {
        Authorization: 'Bearer ' + token
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
}

var timeTick = 1;

$.ajaxSetup({
    error: function (jqXHR, textStatus, errorThrown) {
        if (jqXHR.status == 401) {
            alert("Authorization expired, please sign in.");
            window.location.assign(_urlRoot + "Home/SignIn");
        } else {
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
}

var updateEditingEntry = function (data) {
    entriesModel.EditingEntry(data);
}

var editEntry = function (entryId)
{
    requestData("api/Tracktor/GetEntry", "GET", { entryID: entryId }, function (data) {
        updateHomeModel(data);
        $("#entryEditModal").modal();
    });
}

var signOut = function () {
    requestData("api/Account/Logout", "POST", null, function () {
        var tokenKey = "TokenKey";
        var token = sessionStorage.removeItem(tokenKey);

        window.location.assign("/");
    });
}

var disableButtons = function () {
    $("button").prop("disabled", true);
}

var enableButtons = function () {
    $("button").prop("disabled", false);
}

var generateReport = function () {
    var data = {
        year: $('#ReportYear').val(),
        month: $('#ReportMonth').val(),
        projectID: $('#ReportProject').val()
    };
    $(".reportcurtain").hide();
    requestData("api/Tracktor/GetWebReport", "GET", data,
        function (data) {
            updateHomeModel(data);
            $(".reportcurtain").show();
    });
}