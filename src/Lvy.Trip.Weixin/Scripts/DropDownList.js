var tid = '';
function findDiv() {
    $('div[id^=div]').slideUp(200);
}
function popkeyup(obj, id, controllerName, actionName) {
    tid = id;
    var url = '../' + controllerName + '/' + actionName;
    var value = $(obj).val();
    $.post(url, { 'key': value }, function (data) {
        var html = "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'','" + id +
                     "')>&nbsp;&nbsp;</div>";
        for (var i = 1; i <= data.length; i++) {
            html += "<div style='background-color:#cdcdcd;height:1px;font-size:0;'>&nbsp;</div>";
            html += "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'" + data[i - 1]["Key"] +
                        "','" + id + "')>" + data[i - 1]["Value"] + "</div>";
        }
        $('#div' + id).html(html);
    });
}
function popdiv(obj, id, controllerName, actionName) {
    tid = id;
    findDiv();
    var url = '../' + controllerName + '/' + actionName;
    var value = $(obj).val();
    $.post(url, { 'key': value }, function (data) {
        var html = "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'','" + id +
                     "')>&nbsp;&nbsp;</div>";
        for (var i = 1; i <= data.length; i++) {
            html += "<div style='background-color:#cdcdcd;height:1px;font-size:0;'>&nbsp;</div>";
            html += "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'" + data[i - 1]["Key"] +
                        "','" + id + "')>" + data[i - 1]["Value"] + "</div>";
        }
        $('#div' + id).html(html);
    });
    var x = $('#' + id).offset().left;
    var y = $('#' + id).offset().top + $('#' + id).height() + 2;
    $('#div' + id).css("top", y + "px").css("left", x + "px");
    $('#div' + id).slideDown(200);
}
function popDropDownList(obj, id, controllerName, actionName) {
    tid = id;
    findDiv();
    var url = '../' + controllerName + '/' + actionName;
    $.post(url, '', function (data) {
        var html = "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'','" + id +
                     "')>&nbsp;&nbsp;</div>";
        for (var i = 1; i <= data.length; i++) {
            html += "<div style='background-color:#cdcdcd;height:1px;font-size:0;'>&nbsp;</div>";
            html += "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'" + data[i - 1]["Key"] +
                        "','" + id + "')>" + data[i - 1]["Value"] + "</div>";
        }
        $('#div' + id).html(html);
    });
    var x = $('#' + id).offset().left;
    var y = $('#' + id).offset().top + $('#' + id).height() + 2;
    $('#div' + id).css("top", y + "px").css("left", x + "px");
    $('#div' + id).slideDown(200);
}
function select(obj, key, id) {
    if (obj.innerHTML == "&nbsp;&nbsp;") {
        $('#' + id).val('');
        $('#h' + id).val('');
        $('#div' + id).slideUp(200);
        return;
    }
    $('#' + id).val(obj.innerHTML);
    $('#h' + id).val(key);
    $('#div' + id).slideUp(200);
}
function addListener(element, e, fn) {
    if (element.addEventListener) {
        element.addEventListener(e, fn, false);
    } else {
        element.attachEvent("on" + e, fn);
    }
}
addListener(document, "click", function (evt) {
    var evt = window.event ? window.event : evt, target = evt.srcElement || evt.target;
    if (target.id != tid) {
        $('#div' + tid).slideUp(200);
    }
})
function getValue(name) {
    var hidden = $('input[type=hidden][id^=h]');
    for (var i = 0; i < hidden.length; i++) {
        if (hidden[i].id.indexOf(name) > -1) {
            return $(hidden[i]).val();
        }
    }
}
function popDropDownListEx(obj, id, controllerName, actionName, nodeName) {
    tid = id;
    findDiv();
    var url = '../' + controllerName + '/' + actionName;
    $.post(url, { 'nodeName': nodeName }, function (data) {
        var html = "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'','" + id +
                     "')>&nbsp;&nbsp;</div>";
        for (var i = 1; i <= data.length; i++) {
            html += "<div style='background-color:#cdcdcd;height:1px;font-size:0;'>&nbsp;</div>";
            html += "<div style='font-size: 9pt;color:#ff6600;cursor:pointer;padding-top:5px;padding-left:5px;' onclick=select(this,'" + data[i - 1]["Key"] +
                        "','" + id + "')>" + data[i - 1]["Value"] + "</div>";
        }
        $('#div' + id).html(html);
    });
    var x = $('#' + id).offset().left;
    var y = $('#' + id).offset().top + $('#' + id).height() + 2;
    $('#div' + id).css("top", y + "px").css("left", x + "px");
    $('#div' + id).slideDown(200);
}