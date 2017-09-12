var table;
var weekday = new Array(7);
weekday[0] = "Sunday";
weekday[1] = "Monday";
weekday[2] = "Tuesday";
weekday[3] = "Wednesday";
weekday[4] = "Thursday";
weekday[5] = "Friday";
weekday[6] = "Saturday";
$(document).ready(function () {
    table = $('#attendance').DataTable(
        {
            stateSave: true
        });

    // this is strictly a visual on the datatables, so it's ok if someone fucks with it
    $('#attendance tbody').on('click', 'input', function () {
        var rowDelete = $(this);
        var password = $("#password").val();
        var submit = '{password: "' + password + '" }';
        $.ajax({
            type: "POST",
            url: 'Home/ValidatePasswordSalt',
            data: submit,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                var parsed = JSON.parse(response);
                var valid = parsed["isValid"];
                if (valid === "false") {
                    var div = $(".validation");
                    div.empty();
                    div.append("INVALID PASSWORD OR MISSING DATA");
                }
                // validation passed, successful.
                else {
                    var div = $(".validation");
                    div.empty();
                    table
                        .row($(rowDelete).parents('tr'))
                        .remove()
                        .draw();
                }
            }
        })
    });
});
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
            if (response.indexOf("ERROR") != -1) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD OR MISSING DATA");
            }
            // validation passed, successful.
            else {
                var div = $(".validation");
                div.empty();
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
            if (response.indexOf("ERROR") != -1) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD OR MISSING DATA");
            }
            // validation passed, successful.
            else {
                var div = $(".validation");
                div.empty();
                var parsed = JSON.parse(response);
                var nameAdd = parsed["Name"];
                var date = new Date(parsed["Date"]);
                var dayAdd = parsed["Day"];
                var idAdd = parsed["Id"];
                table.row.add([
                    nameAdd,
                    date.toLocaleString().split(',')[0],
                    weekday[date.getDay()],
                    '<input type="button" class="btn btn-danger" onclick=\'DeleteMethod("' + idAdd + '");\' value="Delete" />'
                ]).draw(false);
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
            if (response.indexOf("ERROR") != -1) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD OR MISSING DATA");
            }
            // validation passed, successful.
            else {
                var div = $(".validation");
                div.empty();
                var parsed = JSON.parse(response);
                var nameAdd = parsed["Name"];
                var idAdd = parsed["Id"];
                table.row.add([
                    nameAdd,
                    '<input type="button" class="btn btn-danger" onclick=\'DeletePlayerRoster("' + idAdd + '");\' value="Delete" />'
                ]).draw(false);
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
            if (response.indexOf("ERROR") != -1) {

                var div = $(".validation");
                div.empty();
                div.append("INVALID PASSWORD OR MISSING DATA");
            }
            // validation passed, successful.
            else {
                var div = $(".validation");
                div.empty();
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