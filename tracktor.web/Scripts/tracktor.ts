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
    requestData("/api/Tracktor/StartTask", "POST", {
        newTaskID: taskId
    }, updateHomeModel);
}

var switchTask = function (taskId: number) {
    requestData("/api/Tracktor/SwitchTask", "POST", {
        currentTaskID: viewModel.TTaskInProgress.TTaskID,
        newTaskID: taskId
    }, updateHomeModel);
}

var stopTask = function (taskId: number) {
    /*alert("Stop " + taskId);*/
    requestData("/api/Tracktor/StopTask", "POST", {
        currentTaskID: taskId
    }, updateHomeModel);
}

var rootModel = function (data) {
    ko.mapping.fromJS(data, rootMapping, this)
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
        return viewModel;
    }
};

var viewModel;

var bindHomeModel = function (data) {
    viewModel = ko.mapping.fromJS(data, rootMapping);
    ko.applyBindings(viewModel);
    upup();
};

var updateHomeModel = function (data) {
    ko.mapping.fromJS(data, rootMapping, viewModel);
};

var refreshModel = function () {
    requestData("api/Tracktor/GetModel", "GET", {}, bindHomeModel);
};

var requestData = function (url: string, method: string, data: any, callback: (any) => void) {
    var tokenKey = "TokenKey";
    var token = sessionStorage.getItem(tokenKey);

    if (token === "") {
        alert("Authorization expired, please sign in.");
        window.location.assign("/Home/SignIn");
    }
    var headers: { [key: string]: any; } = {
        Authorization: 'Bearer ' + token
    };

    var settings: JQueryAjaxSettings = {
        type: method,
        url: url,
        data: data,
        headers: headers
    };

    $.ajax(settings).done(callback);
}

var upup = function () {
    var today = viewModel.Projects()[0].TTasks()[0].Contrib.Today();
    var thisWeek = viewModel.Projects()[0].TTasks()[0].Contrib.ThisWeek();
    var thisMonth = viewModel.Projects()[0].TTasks()[0].Contrib.ThisMonth();
    viewModel.Projects()[0].TTasks()[0].Contrib.Today(today + 60);
    viewModel.Projects()[0].TTasks()[0].Contrib.ThisWeek(thisWeek + 60);
    viewModel.Projects()[0].TTasks()[0].Contrib.ThisMonth(thisMonth + 60);
    setTimeout(upup, 1000);
}
