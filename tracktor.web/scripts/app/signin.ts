/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />

/// <reference path="definitions.ts" />

module Tracktor {
    export var initializeControls = function () {
        $("[data-toggle='tooltip']").tooltip();

        $("#loginform").keyup(function (event: any) {
            if (event.keyCode === 13) {
                $("#signinbutton").click();
            }
        });
    };

    export var internalSignIn = function (username: string, password: string) {
        var loginData = {
            grant_type: "password",
            username: username,
            password: password
        };

        if (!loginData.username || !loginData.password) {
            bootbox.alert("Username and password cannot be empty.");
            return;
        }

        $("#signinbutton").prop("disabled", true);
        $.ajax({
            type: "POST",
            url: _urlRoot + "token",
            data: loginData
        }).done(function (data: any) {
            sessionStorage.setItem(_tokenKey, data.access_token);
            window.location.assign(_urlRoot);
        }).fail(function (data: any) {
            bootbox.alert(data.responseJSON.error_description);
            $("#signinbutton").prop("disabled", false);
        });
    };

    export var register = function () {
        var registerData = {
            Authorization: $("#Authorization").val(),
            Email: $("#RegisterEmail").val(),
            Password: $("#RegisterPassword").val(),
            ConfirmPassword: $("#ConfirmPassword").val(),
            TimeZone: $("#TimeZone").val()
        };

        if (!registerData.Authorization || !registerData.Email || !registerData.Password || !registerData.ConfirmPassword) {
            bootbox.alert("Please fill in all fields on the registration form.");
            return;
        }
        if (registerData.Password !== registerData.ConfirmPassword) {
            bootbox.alert("Password and confirmation don't match!");
            return;
        }

        $("#registerbutton").prop("disabled", true);
        $.ajax({
            type: "POST",
            url: _urlRoot + "account/register",
            data: registerData
        }).done(function (data: any) {
            internalSignIn($("#RegisterEmail").val(), $("#RegisterPassword").val());
        }).fail(function (data: any) {
            var errors = "";
            var modelState = data.responseJSON.ModelState;
            if (modelState) {
                for (var key in modelState) {
                    if (key) {
                        for (var i = 0; i < modelState[key].length; i++) {
                            errors = errors + modelState[key][i] + "<br/>";
                        }
                    }
                }
                bootbox.alert(errors);
                $("#registerbutton").prop("disabled", false);
            } else {
                bootbox.alert(data.responseJSON.Message);
                $("#registerbutton").prop("disabled", false);
            }
        });
    };

    export var signIn = function () {
        internalSignIn($("#username").val(), $("#password").val());
    };
};