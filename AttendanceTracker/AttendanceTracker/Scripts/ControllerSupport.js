
// delete player from attendance list
function DeleteMethod(id) {
    var password = $("#password").val();
    var submit = '{id: "' + id + '", password: "' + password + '" }';
    $.ajax({
        type: "POST",
        url: 'Home/DeletePlayer',
        data: submit,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.length > 0) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD");
            }
            else {
                var div = $(".validation");
                div.empty();
                $('#messagesDiv').load(document.URL + ' #messagesDiv');
            }
        },
        error: function (response) {
            alert("error occurred. Tell Chiaki");
        },
        failure: function (response) {
            alert("failure occurred. Tell Chiaki");
        }
    })
}

// Add a player's attendace to the attendance list
function CallMethod() {
    var name = $("#nameInput").val();
    var date = $("#messageInput").val();
    var password = $("#password").val();

    var submit = '{name: "' + name + '", date: "' + date + '", password: "' + password + '" }';
    $.ajax({
        type: "POST",
        url: 'Home/SubmitMethod',
        data: submit,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.length > 0) {
                
                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD OR MISSING DATA");
            }
            else {
                var div = $(".validation");
                div.empty();
                $('#messagesDiv').load(document.URL + ' #messagesDiv');
            }
        },
        error: function (response) {
            alert("error occurred. Tell Chiaki");
        },
        failure: function (response) {
            alert("failure occurred. Tell Chiaki");
        }
    })
}

// Add a player to the roster
function AddPlayerRoster() {
    var name = $("#nameInput").val();
    var password = $("#password").val();

    var submit = '{name: "' + name + '", password: "' + password + '" }';
    $.ajax({
        type: "POST",
        url: 'AddPlayerRoster',
        data: submit,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.length > 0) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD OR MISSING DATA");
            }
            else {
                var div = $(".validation");
                div.empty();
                $('#messagesDiv').load(document.URL + ' #messagesDiv');
            }
        },
        error: function (response) {
            alert("error occurred. Tell Chiaki");
        },
        failure: function (response) {
            alert("failure occurred. Tell Chiaki");
        }
    })
}

// delete player from the roster
function DeletePlayerRoster(id) {
    var password = $("#password").val();
    var submit = '{id: "' + id + '", password: "' + password + '" }';
    $.ajax({
        type: "POST",
        url: 'DeletePlayerRoster',
        data: submit,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.length > 0) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD");
            }
            else {
                var div = $(".validation");
                div.empty();
                $('#messagesDiv').load(document.URL + ' #messagesDiv');
            }
        },
        error: function (response) {
            alert("error occurred. Tell Chiaki");
        },
        failure: function (response) {
            alert("failure occurred. Tell Chiaki");
        }
    })
}
