
window.onload = function () {

    $('.searchMain').hide();

}

function login(address) {
    var userId = $("#loginUserId");
    var userPass = $("#loginUserPassword");
    if (address == null) {
        address = window.location.href;
    }

    LoginUserPassHref(userId, userPass, address);

}

$(document).ready(function () {

    $("#btnUserRegister").on("click", function () {
        return RegisterUser();
    });

});

function LoginUserPassHref(pUserId, pUserPass, address) {
    pUserId.css("border", '');
    pUserPass.css("border", '');
    $("#loginFormDivLoginMessage").text('');

    var isValid = true;

    if (pUserId.val() == "") {
        pUserId.css("border", "2px solid red");
        isValid = false;
    }

    if (pUserPass.val() == "") {
        pUserPass.css("border", "2px solid red");
        isValid = false;
    }

    if (isValid) {
        $.ajax({
            url: "/Account/AsyncUserLogin",
            type: "POST",
            data: { userId: pUserId.val(), userPass: pUserPass.val() },
            success: function (xhr) {
                if (xhr.response == 1) {
                    window.location.href = address;
                } else {
                    $("#loginFormDivLoginMessage").text('Combinația utilizator-parolă este greșită!');
                    pUserId.css("border", "2px solid red");
                    pUserPass.css("border", "2px solid red");
                }
            }
        })
    }
}

function LoginUser(pUserId, pUserPass) {
    pUserId.css("border", '');
    pUserPass.css("border", '');
    $("#loginFormDivLoginMessage").text('');

    var isValid = true;

    if (pUserId.val() == "") {
        pUserId.css("border", "2px solid red");
        isValid = false;
    }

    if (pUserPass.val() == "") {
        pUserPass.css("border", "2px solid red");
        isValid = false;
    }

    if (isValid) {
        $.ajax({
            url: "/Account/AsyncUserLogin",
            type: "POST",
            data: { userId: pUserId.val(), userPass: pUserPass.val() },
            success: function (xhr) {
                if (xhr.response == 1) {
                    window.location.href = "/Home/Index";
                } else {
                    $("#loginFormDivLoginMessage").text('Combinația utilizator-parolă este greșită!');
                    pUserId.css("border", "2px solid red");
                    pUserPass.css("border", "2px solid red");
                }
            }
        })
    }
}

function RegisterUser() {
    $("#loginFormDivCreateMessage").text('');
    if (RegisterUserValidate()) {
        var userName = $("#txtUserName");
        var userSurame = $("#txtUserSurame");
        var email = $("#registerUserEmail");
        var password = $("#registerUserPassword");


        $.ajax({
            url: "/Cont/AsyncUserRegister",
            type: "POST",
            data: { userSurame: userSurame.val(), userName: userName.val(), userEmail: email.val(), userPass: password.val() },
            success: function (xhr) {
                if (xhr.response == 1) {
                    window.location.href = "/Home/Index?newRegister=true";
                }
                else if (xhr.response == 4) {
                    $("#loginFormDivCreateMessage").text('Acest email a fost deja folosit!');
                    email.css("border", "2px solid red");
                }
                else {
                    $("#loginFormDivCreateMessage").text('Eroare internă, te rugâm contactează-ne și descrie-ne problema!');
                }
            }
        })
    }
}

function RegisterUserValidate() {
    var errors = [];
    var userName = $("#txtUserName");
    var userSurame = $("#txtUserSurame");
    var email = $("#registerUserEmail");
    var password = $("#registerUserPassword");
    var passwordVerify = $("#registerUserPasswordVerify");

    userName.css("border", '');
    userSurame.css("border", '');
    email.css("border", '');
    password.css("border", '');
    passwordVerify.css("border", '');

    if (userName.val() == "") {
        userName.css("border", "2px solid red");
        errors.push("userName");
    }

    if (userSurame.val() == "") {
        userSurame.css("border", "2px solid red");
        errors.push("userSurame");
    }

    if (email.val() == "") {
        email.css("border", "2px solid red");
        errors.push("email");
    }

    if (password.val() == "") {
        password.css("border", "2px solid red");
        errors.push("password");
    }
    if (passwordVerify.val() == "") {
        passwordVerify.css("border", "2px solid red");
        errors.push("passwordVerify");
    }

    if (password.val() != passwordVerify.val()) {
        password.css("border", "2px solid red");
        passwordVerify.css("border", "2px solid red");
        errors.push("passwordVerifyDiffer");
    }

    //result
    if (errors.length > 0) {
        return false;
    }

    return true;
}