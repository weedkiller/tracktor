/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />

module Tracktor {
    export var summaryModel;
    export var statusModel;
    export var entriesModel;
    export var reportModel;
    export var editModel;

    export var viewOptions = {
        showObsolete: ko.observable(false)
    };

    export var projectMapping = function (data: any) {
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

    export var rootModel = function (data) {
        ko.mapping.fromJS(data, rootMapping, this);
    };

    export var rootMapping = {
        "Projects": {
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

    export var bindHomeModel = function (data) {
        summaryModel = ko.mapping.fromJS(data.SummaryModel, rootMapping);
        ko.applyBindings(summaryModel, document.getElementById("SummaryModel"));

        statusModel = ko.mapping.fromJS(data.StatusModel);
        ko.applyBindings(statusModel, document.getElementById("StatusModel"));

        entriesModel = ko.mapping.fromJS(data.EntriesModel);
        ko.applyBindings(entriesModel, document.getElementById("EntriesModel"));

        reportModel = ko.mapping.fromJS(data.ReportModel);
        ko.applyBindings(reportModel, document.getElementById("ReportModel"));

        editModel = ko.mapping.fromJS(data.EditModel);
        ko.applyBindings(editModel, document.getElementById("EditModel"));

        updateTitle();
        $(".curtain").removeClass("curtain");
        $(".nocurtain").remove();
        timerFunc();
    };

    export var updateHomeModel = function (data) {
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
            $("#EditStartDate").data("DateTimePicker").maxDate(new Date());
            $("#EditEndDate").data("DateTimePicker").minDate(new Date(0));
            $("#EditEndDate").data("DateTimePicker").date(null);
            ko.mapping.fromJS(data.EditModel, {}, editModel);
            // update datepickers
            $("#EditStartDate").data("DateTimePicker").date(moment(editModel.Entry.StartDate()));
            if (editModel.Entry.EndDate() != null) {
                $("#EditEndDate").data("DateTimePicker").date(moment(editModel.Entry.EndDate()));
            }
        }
    };

    export var refreshModel = function () {
        hideReport();
        requestData("viewmodel", "GET", {}, updateHomeModel);
    };

    export var initializeModel = function () {
        requestData("viewmodel", "GET", {}, bindHomeModel);
    };
};