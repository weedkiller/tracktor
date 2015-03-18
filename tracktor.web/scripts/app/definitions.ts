/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />

module Tracktor {
    export var _tokenKey: string = "TokenKey";
    export var _urlRoot: string;
    export var _timeTick: number = 1;

    export var initializeDefinitions = function (urlRoot: string) {
        _urlRoot = urlRoot;
    };
};