class LoginHelpers {
    // The object that will be used to invoke C# methods
    static dotNetHelper;

    static setDotNetHelper(value) {
        LoginHelpers.dotNetHelper = value;
    }

    static async onLoginSuccess() {
        await LoginHelpers.dotNetHelper.invokeMethodAsync('OnLoginSuccess');
    }

    static async onLoginFailure(error) {
        await LoginHelpers.dotNetHelper.invokeMethodAsync('OnLoginFailure', error);
    }
}

window.LoginHelpers = LoginHelpers;

function onLoginSubmit(form) {
    // Make a JSON object from the form data
    console.log(form);
    console.log(new FormData(form))
    console.log(Object.fromEntries(new FormData(form)));
    console.log(JSON.stringify(Object.fromEntries(new FormData(form))));
    
    var formData = JSON.stringify(Object.fromEntries(new FormData(form)))
    $.post({
        url: `auth/login`,
        data: formData,
        success: () => {
            LoginHelpers.onLoginSuccess();
        },
        error: (e) => {
            LoginHelpers.onLoginFailure(JSON.parse(e.responseText).message);
        },
        dataType: 'json',
        crossDomain: true,

        // This will save the cookie
        xhrFields: {
            withCredentials: true
        },

        // Will be sent as "Content-Type: application/json"
        contentType : "application/json"
    });
}