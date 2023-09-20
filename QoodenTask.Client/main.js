const loginForm = document.getElementById("login-form");
const signUpForm = document.getElementById("sign-up-form");
const users = document.getElementById('users');
const balances = document.getElementById('balances');
const toggleButton = document.getElementById('toggle-form-button');
const baseUrl = 'https://localhost:44390'

$(users).hide();
$(balances).hide();
$(signUpForm).hide();


loginForm.addEventListener("submit", onLogin);
signUpForm.addEventListener("submit", onSignUp);

function toggleForm(){
    if($(signUpForm).is(":visible")){
        $(loginForm).show();
        $(signUpForm).hide();
        $(toggleButton).text('Sign up');
    } else if($(loginForm).is(":visible")){
        $(loginForm).hide();
        $(signUpForm).show();
        $(toggleButton).text('Sign in');
    }
}

function getBalances() {
    $.get({
        url: `${baseUrl}/wallet/balance`,
        success: (data) => {
            console.log(data);
            let balanceTable = document.getElementById("balance-table");
            
            for (var key in data) {
                
                const row = balanceTable.insertRow();

                const cellId = row.insertCell();
                $(cellId).text(key)

                const cellBalance = row.insertCell();
                $(cellBalance).text(data[key].balance)

                const cellUsdAmount = row.insertCell();
                $(cellUsdAmount).text(data[key].usdAmount)
            }
            $(balances).show();
        },
        contentType: "application/json",
        crossDomain: true,

        // This will use the cookie
        xhrFields: {
            withCredentials: true
        }
    });
}

function getUsers() {
    $.get({
        url: `${baseUrl}/admin/users`,
        success: (data) => {
            console.log(data);
            let userTable = document.getElementById("user-table");

            data.forEach((item, i) => {
                const row = userTable.insertRow();
                
                const cellId = row.insertCell();
                $(cellId).text(item.id)

                const cellUsername = row.insertCell();
                $(cellUsername).text(item.userName)

                const cellPass = row.insertCell();
                $(cellPass).text(item.password);

                const cellRole = row.insertCell();
                $(cellRole).text(item.role)

                const cellIsActive = row.insertCell();
                $(cellIsActive).text(item.isActive)
            });
            $(users).show();
        },
        contentType: "application/json",
        crossDomain: true,

        // This will use the cookie
        xhrFields: {
            withCredentials: true
        }
    });
}

function onLogin(e) {
    // Don't refresh the page
    e.preventDefault();
    
    let formData = JSON.stringify(Object.fromEntries(new FormData(e.target)))
    console.log(formData);
    
    $.ajax({
        type : "POST",
        url: `${baseUrl}/auth/login`,
        data: formData,
        statusCode: {
            401: function () {
                alert("Wrong login or password");
            }
        },
        success: () => {
            $(loginForm).hide();
            $(toggleButton).hide();
            getUsers();
            getBalances();
            alert("Login successful");
        },
        error: () => {
            alert("Login failed");
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

function onSignUp(e) {
    // Don't refresh the page
    e.preventDefault();

    let formData = JSON.stringify(Object.fromEntries(new FormData(e.target)))
    console.log(formData);

    $.ajax({
        type : "POST",
        url: `${baseUrl}/auth/sign-up`,
        data: formData,
        success: () => {
            $(signUpForm).hide();
            $(toggleButton).hide();
            getUsers();
            getBalances();
            alert("Sign-up successful");
        },
        error: () => {
            alert("Sign-up failed");
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
