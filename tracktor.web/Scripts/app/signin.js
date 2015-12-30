/// <reference path="../typings/jquery/jquery.d.ts" />
/// <reference path="../typings/knockout.mapping/knockout.mapping.d.ts" />
/// <reference path="../typings/moment/moment.d.ts" />
/// <reference path="../typings/bootstrap/bootstrap.d.ts" />
/// <reference path="../typings/bootstrap.v3.datetimepicker/bootstrap.v3.datetimepicker.d.ts" />
/// <reference path="../typings/bootbox/bootbox.d.ts" />
/// <reference path="definitions.ts" />
var Tracktor;
(function (Tracktor) {
    Tracktor.initializeControls = function () {
        $("[data-toggle='tooltip']").tooltip();
        $("#loginform").keyup(function (event) {
            if (event.keyCode === 13) {
                $("#signinbutton").click();
            }
        });
    };
    Tracktor.internalSignIn = function (username, password) {
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
            url: Tracktor._urlRoot + "token",
            data: loginData,
        }).done(function (data, status, xhr) {
            sessionStorage.setItem(Tracktor._tokenKey, data.access_token);
            window.location.assign(Tracktor._urlRoot);
        }).fail(function (data) {
            Tracktor.authFailed(data);
        });
    };
    Tracktor.authFailed = function (data) {
        if (data) {
            bootbox.alert({ message: data.responseJSON.error_description });
        }
        else {
            bootbox.alert("Authentication failed.");
        }
        $("#signinbutton").prop("disabled", false);
    };
    Tracktor.register = function () {
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
            url: Tracktor._urlRoot + "account/register",
            data: registerData
        }).done(function (data) {
            Tracktor.internalSignIn($("#RegisterEmail").val(), $("#RegisterPassword").val());
        }).fail(function (data) {
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
            }
            else {
                bootbox.alert(data.responseJSON.Message);
                $("#registerbutton").prop("disabled", false);
            }
        });
    };
    Tracktor.signIn = function () {
        Tracktor.internalSignIn($("#username").val(), $("#password").val());
    };
})(Tracktor || (Tracktor = {}));
;
//# sourceMappingURL=signin.js.map