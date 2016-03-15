function deleteListItem(elem, id, type, name) {
    if (window.confirm("Do you really want to remove \"" + name + "\"?") == false) {
        $(elem).blur();
        return false;
    }
    

    var result = false;
    $.ajax({
        url: "/Home/deleteListItem",
        data: "{'id': '" + id + "', 'type' : '" + type + "' }",
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        async: false,
        success: function (data) {
            $(elem).closest("tr").remove();
            result = true;
        },
        error: function(errss){
            result = false;
        },

        //Options to tell JQuery not to process data or worry about content-type
        cache: false,
        processData: false
    });

    return result;
}


function validateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}