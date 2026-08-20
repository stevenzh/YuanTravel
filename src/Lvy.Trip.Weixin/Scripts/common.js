//wx.config({
//    debug: true, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
//    appId: '', // 必填，公众号的唯一标识
//    timestamp: '', // 必填，生成签名的时间戳
//    nonceStr: '', // 必填，生成签名的随机串
//    signature: '',// 必填，签名，见附录1
//    jsApiList: [] // 必填，需要使用的JS接口列表，所有JS接口列表见附录2
//});



//////////CheckBox全选
function SelectAll(tempControl) {
    //将除头模板中的其它所有的CheckBox取反

    var theBox = tempControl;
    xState = theBox.checked;

    elem = theBox.form.elements;
    for (i = 0; i < elem.length; i++) {
        if (elem[i].type == "checkbox") {
            if (elem[i].checked != xState)
                elem[i].click();
        }

    }
}
// checkbox 组全选 。   需要在元素中加入  group 属性
function SelectGroup(theBox) {

    var xState = theBox.checked;
    var gName = theBox.getAttribute('group');

    $("#theForm input[group='" + gName + "']").each(function () {
        if (this.checked != xState)
            this.click();
    });
}

function IsPositiveInteger(value) {
    var pattern = /^[1-9]\d*$/;
    return pattern.test(value);
}

function SelectAllWeek(tempControl) {
    //将除头模板中的其它所有的CheckBox取反 

    var theBox = tempControl;
    xState = theBox.checked;

    elem = theBox.form.elements;
    for (i = 0; i < elem.length; i++)
        if (elem[i].type == "checkbox" && elem[i].id != theBox.id && elem[i].id != "cbx_showsearch") {
            if (elem[i].id == "mon" || elem[i].id == "tue" || elem[i].id == "wed" || elem[i].id == "thurs" || elem[i].id == "fri" || elem[i].id == "satur" || elem[i].id == "sun") {
                if (elem[i].checked != xState)
                    elem[i].click();
            }
        }

}

/*
日期增加
interval:  间隔
num:       增加量
dateValue: 日期值
*/
function DateAdd(interval, num, dateValue) {
    var date = new Date(dateValue);
    switch (String(interval).toLowerCase()) {
        case "y": case "year": date.setYear(date.getYear() + num); break;
        case "n": case "month": date.setMonth(date.getMonth() + num); break;
        case "d": case "day": date.setDate(date.getDate() + num); break;
        case "h": case "hour": date.setHours(date.getHours() + num); break;
        case "m": case "minute": date.setMinutes(date.getMinutes() + num); break;
        case "s": case "second": date.setSeconds(date.getSeconds() + num); break;
        case "ms": case "msecond": date.setMilliseconds(date.getMilliseconds() + num); break;
        case "w": case "week": date.setDate(date.getDate() + num * 7); break;
        default: return ("invalid");
    }
    //var now = newCom.year + "/" + newCom.month + "/" + newCom.day + " " + newCom.hour + ":" + newCom.minute + ":" + newCom.second;
    //return (new Date(now));
    return date;
}

//-----------------全局设置------------------------------------------
//
$(document).ready(function () {
    hoverChangeStyle();
    // 当 <input >中含有 input-limit='number'，只允许输入number
    // demo :<input type="text"  input-limit="number" />
    $("form input[input-limit='number']").keypress(function () {
        InputLimitNumber();
    });
});

// hover的场合改变tablelist的样式
function hoverChangeStyle() {
    $("input[type='text'], password,textarea,select,input[type='checkbox'],input[type='radio'] ").focus(function () {
        $(this).css('border', '1px solid #5BB9EC');
    });

    $("input[type='text'], password,textarea,select,input[type='checkbox'],input[type='radio'] ").focusout(function () {
        $(this).css('border', '1px solid #CCCCCC');
    });

    $(".listtbl tbody tr:odd").addClass("tbllist_odd");
    $(".listtbl tbody tr:odd").hover(
        function () {
            $(this).addClass("tbllist_hover");
            $(this).removeClass("tbllist_odd");
        }, function () {
            $(this).removeClass("tbllist_hover");
            $(this).addClass("tbllist_odd");
        }
    );
    $(".listtbl tbody tr:even").hover(
        function () {
            $(this).addClass("tbllist_hover");
        }, function () {
            $(this).removeClass("tbllist_hover");
        }
    );
}

//  只允许输入number
function InputLimitNumber() {

    if (((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105) && event.keyCode != 46 && event.keyCode != 8))
        event.returnValue = false;
    //if (!(event.keyCode == 46) && !(event.keyCode == 8) && !(event.keyCode == 37) && !(event.keyCode == 39))
    //    if (!((event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105)))
    //        event.returnValue = false;
}

function InputLimitMoney() {
    if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105) && event.keyCode != 45 && event.keyCode != 46 && event.keyCode != 8)
        event.returnValue = false;
}


//-----------------------------------------------------------
var Utils = new Object();
Utils.Check = new Object();
Utils.Convert = new Object();

Utils.Check.IsMoney = function (val) {
    //var reg = /\d+/;
    //return reg.test(val);
    var reg1 = /^(-)?(([1-9]\d*)|\d)(\.\d{1,2})?$/;

    return reg1.test(val);
};

Utils.Check.IsInt = function (val) {
    //var reg = /\d+/;
    //return reg.test(val);
    if (val.length == 0) {
        return true;
    }
    for (var i = 0; i < val.length; ++i) {
        DataChar = val.charCodeAt(i);
        if (!(DataChar >= 48 && DataChar <= 57))		//0～9
        {
            return false;
        }
    }
    return true;
};

Utils.Check.IsEmail = function (email) {
    var reg1 = /([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)/;

    return reg1.test(email);
};
Utils.Check.IsID = function (passNo) {
    var reg = /^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$/;
    return reg.test(Utils.Trim(passNo));
};


Utils.QueryString = function (name) {

    var AllVars = window.location.search.toLowerCase().substring(1);
    var Vars = AllVars.split("&");
    for (i = 0; i < Vars.length; i++) {
        var Var = Vars[i].split("=");
        if (Var[0] == name) return Var[1];
    }
    return "";
};

Utils.Trim = function (str) {
    return str.replace(/(^\s*)|(\s*$)/g, "");
};
Utils.LTrim = function (str) {
    return str.replace(/(^\s*)/g, "");
};
Utils.RTrim = function (str) {
    return str.replace(/(\s*$)/g, "");
};
Utils.Print = function () {
    window.print();
};

Utils.Convert.Int = function (value) {
    return Number(value);
};

Utils.Convert.Decimal = function (value) {
    return parseFloat(value);
};

Utils.alert = function (msg, t) {
    var tt = 2;
    if (t != null) tt = t;
    layer.open({
        content: msg
        , skin: 'msg'
        , time: tt    //秒后自动关闭
    });
}

Utils.layerLoad = function () {
    return layer.open({ type: 2 });
}

Utils.layerClose = function (obj) {
    layer.close(obj)
}

//function MsgPoll(opts) {
//    var _this = this;
//    _this.handler = null;
//    _this.requestUrl = opts.url;
//    _this.interval = opts.interval;
//    _this.callback = opts.callback;
//    _this.poll = function () {
//        $.ajax({
//            type: "post",
//            url: _this.requestUrl,
//            dataType: "json",
//            success: function (data) {
//                _this.callback(data);
//                if (null != _this.handler)
//                    clearTimeout(_this.handler);
//                _this.handler = setTimeout(_this.poll, _this.interval);
//            },
//            complete: function (xhr) {
//                xhr = null;
//            }
//        });
//    };
//    _this.handler = setTimeout(_this.poll, 4000);
//}

