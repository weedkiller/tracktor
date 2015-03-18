/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />

var _tokenKey: string = "TokenKey";
var _urlRoot: string;
var _timeTick: number = 1;

var initializeDefinitions = function (urlRoot: string) {
    _urlRoot = urlRoot;
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
