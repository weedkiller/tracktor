/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />
var Tracktor;
(function (Tracktor) {
    Tracktor.viewOptions = {
        showObsolete: ko.observable(false)
    };
    Tracktor.projectMapping = function (data) {
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
    Tracktor.rootModel = function (data) {
        ko.mapping.fromJS(data, Tracktor.rootMapping, this);
    };
    Tracktor.rootMapping = {
        "Projects": {
            create: function (options) {
                return new Tracktor.projectMapping(options.data);
            }
        },
        create: function (options) {
            var viewModel = new Tracktor.rootModel(options.data);
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
    Tracktor.bindHomeModel = function (data) {
        Tracktor.summaryModel = ko.mapping.fromJS(data.SummaryModel, Tracktor.rootMapping);
        ko.applyBindings(Tracktor.summaryModel, document.getElementById("SummaryModel"));
        Tracktor.statusModel = ko.mapping.fromJS(data.StatusModel);
        ko.applyBindings(Tracktor.statusModel, document.getElementById("StatusModel"));
        Tracktor.entriesModel = ko.mapping.fromJS(data.EntriesModel);
        ko.applyBindings(Tracktor.entriesModel, document.getElementById("EntriesModel"));
        Tracktor.reportModel = ko.mapping.fromJS(data.ReportModel);
        ko.applyBindings(Tracktor.reportModel, document.getElementById("ReportModel"));
        Tracktor.editModel = ko.mapping.fromJS(data.EditModel);
        ko.applyBindings(Tracktor.editModel, document.getElementById("EditModel"));
        Tracktor.updateTitle();
        $(".curtain").removeClass("curtain");
        $(".nocurtain").remove();
        Tracktor.timerFunc();
    };
    Tracktor.updateHomeModel = function (data) {
        if (data.SummaryModel) {
            ko.mapping.fromJS(data.SummaryModel, Tracktor.rootMapping, Tracktor.summaryModel);
        }
        if (data.StatusModel) {
            ko.mapping.fromJS(data.StatusModel, {}, Tracktor.statusModel);
            Tracktor.updateTitle();
        }
        if (data.EntriesModel) {
            ko.mapping.fromJS(data.EntriesModel, {}, Tracktor.entriesModel);
        }
        if (data.ReportModel) {
            ko.mapping.fromJS(data.ReportModel, {}, Tracktor.reportModel);
        }
        if (data.EditModel) {
            $("#EditStartDate").data("DateTimePicker").maxDate(null);
            $("#EditEndDate").data("DateTimePicker").minDate(null);
            $("#EditEndDate").data("DateTimePicker").date(null);
            ko.mapping.fromJS(data.EditModel, {}, Tracktor.editModel);
            // update datepickers
            $("#EditStartDate").data("DateTimePicker").date(moment(Tracktor.editModel.Entry.StartDate()));
            if (Tracktor.editModel.Entry.EndDate() != null) {
                $("#EditEndDate").data("DateTimePicker").date(moment(Tracktor.editModel.Entry.EndDate()));
            }
        }
    };
    Tracktor.refreshModel = function () {
        Tracktor.hideReport();
        Tracktor.requestData("viewmodel", "GET", {}, Tracktor.updateHomeModel);
    };
    Tracktor.initializeModel = function () {
        Tracktor.requestData("viewmodel", "GET", {}, Tracktor.bindHomeModel);
    };
})(Tracktor || (Tracktor = {}));
;
//# sourceMappingURL=model.js.map