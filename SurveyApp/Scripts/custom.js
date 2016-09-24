function deleteListItem(elem, id, type, name, callback) {
    var result = false;

    bootbox.confirm({
        message: "Do you really want to remove \"" + name + "\"?", callback: function (res) {
            if (res == false) {
                $(elem).blur();
                bootbox.hideAll();
                return false;
            }
            else {
                $.ajax({
                    url: "../Home/deleteListItem",
                    data: "{'id': '" + id + "', 'type' : '" + type + "' }",
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    success: function (data) {
                        if (data.success == true) {
                            if (type == "crintervention" || type == "adversereaction" || type == "lifeenvent") {
                                $(elem).closest(".item").remove();
                            }
                            else {
                                if (type != "consent") {
                                    $(elem).closest("tr").remove();
                                }
                            }
                            result = true;
                            if (callback != null && callback != undefined && callback != "undefined") {
                                callback();
                            }
                        }
                        else {
                            boobox.alert("Unable to perform the action, please try again or contact our support team.");
                            result = false;
                        }
                    },
                    error: function (errss) {
                        result = false;
                    },

                    //Options to tell JQuery not to process data or worry about content-type
                    cache: false,
                    processData: false
                });
            }
        }
    });
    
    return result;
}


function validateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}