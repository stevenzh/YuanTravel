// 全局变量定义
var ApiServerUrl = "http://api.sh-cct.cn";


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
    return $.trim(str);
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
    var tt = 2000;
    if (t != null) tt = t;
    layer.msg(msg, { time: tt });
}

Utils.layerLoad = function () {
    return layer.load(1, { shade: [0.1, '#fff'] });
}

Utils.layerClose = function (obj) {
    layer.close(obj)
}

$.extend({
    ToHtmlStr: function (str) {
        if (str == null || str.length == 0) {
            return "";
        }
        return str.replace(/(\\|\")/g, "\\$1").replace(/\n|\r|\t/g,
            function () {
                var a = arguments[0];
                return (a == '\n') ? '\\n' :
                    (a == '\r') ? '\\r' :
                        (a == '\t') ? '\\t' : ""
            });
    },
    CovertJsonDate: function (cellval) {
        var date = new Date(parseInt(cellval.replace("/Date(", "").replace(")/", ""), 10));
        var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
        var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
        return date.getFullYear() + "-" + month + "-" + currentDate;
    },
    CovertJsonDateTime: function (cellval) {
        var date = new Date(parseInt(cellval.replace("/Date(", "").replace(")/", ""), 10));
        var y = date.getFullYear();
        var m = date.getMonth() + 1;
        var d = date.getDate();
        var h = date.getHours();
        var mi = date.getMinutes();
        return y + "-" + (m < 10 ? "0" + m.toString() : m.toString()) + "-" + (d < 10 ? "0" + d.toString() : d.toString()) + " " + (h < 10 ? "0" + h.toString() : h.toString()) + ":" + (mi < 10 ? "0" + mi.toString() : mi.toString());
    },
    GetCateGory: function (cateGory, subCateGory) {
        var result = "";
        if (cateGory != null && cateGory.length > 0) {
            result += cateGory;
        }
        if (subCateGory != null && subCateGory.length > 0 && subCateGory != '所有二级分类') {
            result += result.length > 0 ? "&gt" + subCateGory : subCateGory;
        }
        return result;
    },
    DateAdd: function (interval, number, date) {
        number = parseInt(number);
        if (typeof (date) == "string") {
            date = date.split(/D/);
            eval("var date = new Date(" + date.join(",") + ")");
        }
        if (typeof (date) == "object") {
            date = date
        }
        switch (interval) {
            case "y": date.setFullYear(date.getFullYear() + number); break;
            case "m": date.setMonth(date.getMonth() + number); break;
            case "d": date.setDate(date.getDate() + number); break;
            case "w": date.setDate(date.getDate() + 7 * number); break;
            case "h": date.setHours(date.getHours() + number); break;
            case "n": date.setMinutes(date.getMinutes() + number); break;
        }
        return date;
    },
    FormatDateByDate: function (date) {
        var y = date.getFullYear();
        var m = date.getMonth() + 1;
        var d = date.getDate();
        var h = date.getHours();
        var mi = date.getMinutes();
        return y + "-" + (m < 10 ? "0" + m.toString() : m.toString()) + "-" + (d < 10 ? "0" + d.toString() : d.toString()) + " " + (h < 10 ? "0" + h.toString() : h.toString()) + ":" + (mi < 10 ? "0" + mi.toString() : mi.toString());
    },
    FormatDateByP: function (y, m, d, h, mi) {
        return y + "-" + (m < 10 ? "0" + m.toString() : m.toString()) + "-" + (d < 10 ? "0" + d.toString() : d.toString()) + " " + (h < 10 ? "0" + h.toString() : h.toString()) + ":" + (mi < 10 ? "0" + mi.toString() : mi.toString());
    },
    FormatNum: function (num)   //将数字转换成三位逗号分隔的样式   
    {
        if (!/^(\+|-)?(\d+)(\.\d+)?$/.test(num)) { return num; }
        var a = RegExp.$1, b = RegExp.$2, c = RegExp.$3;
        var re = new RegExp().compile("(\\d)(\\d{3})(,|$)");
        while (re.test(b)) b = b.replace(re, "$1,$2$3");
        return a + "" + b + "" + c;
    },
    openWindow: function (url, winname, width, height, top, left) {
        if (!width) width = document.body.clientWidth + (screen.width - document.body.clientWidth) / 2;
        if (!height) height = document.body.clientHeight + (screen.height - document.body.clientHeight) / 2;
        if (!top) top = (screen.height - height) / 2;
        if (!left) left = (screen.width - width) / 2;;
        window.open(url, winname, "width=" + width + ",height=" + height + ",top=" + top + ",left=" + left + ",'toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,resizable=yes,alwaysRaised=yes,depended=yes,titlebar=no,model=yes,channelmode=no");

    },
    closeWindow: function (noConfirm) {
        if (noConfirm) { window.opener = null; window.open("", "_self"); }
        window.close();
    },
    nowTicks: function () { return +new Date },
    noCacheURL: function (url) { var ts = this.nowTicks(); var ret = url.replace(/(\?|&)_=.*?(&|$)/, "$1_=" + ts + "$2"); return ret + ((ret == url) ? (url.match(/\?/) ? "&" : "?") + "_=" + ts : ""); },
    isHex: function (num) { return /^[0-9a-f]+$/i.test(num) },
    isMail: function (mail) { return /^[a-z0-9](?:[a-z0-9]*[-._]?[a-z0-9]+)*@(?:[a-z0-9]*[-_]?[a-z0-9]+)+(?:\.[a-z0-9]{2,3})+$/i.test(mail) },
    isMobile: function (mo) { return /^1[345789][0-9]{9}$/.test(mo) },
    FormatCurrency: function (num) {
        num = num.toString().replace(/\$|\,/g, '');
        if (isNaN(num))
            num = "0";
        sign = (num == (num = Math.abs(num)));
        num = Math.floor(num * 100 + 0.50000000001);
        cents = num % 100;
        num = Math.floor(num / 100).toString();
        if (cents < 10)
            cents = "0" + cents;
        for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3); i++)
            num = num.substring(0, num.length - (4 * i + 3)) + ',' +
                num.substring(num.length - (4 * i + 3));
        return (((sign) ? '' : '-') + num + '.' + cents);
    },
    IsNumber: function (Obj) {
        var reg = /^(0|([1-9]\d*))(\.\d+)?$/;
        if (reg.test(Obj)) {
            return true;
        } else {
            return false;
        }
    },
    CheckCharacter: function (value) {
        return !/[@#\$%\^&\*]+/g.test(value);
    },
    AddSelectOption: function (select, optionValue) {
        $(select).find('option').remove().end().append(optionValue);
    },
    ArraySingleCheck: function (array) {
        return /(\x0f[^\x0f]+)\x0f[\s\S]*\1/.test("\x0f " + array.join("\x0f\x0f ") + "\x0f ");
    },
    ClearBlankLine: function (_sHtml) {
        var vReturn = _sHtml;
        var vPatterns = [
            '<p(>|\\s+[^>]*>)(&nbsp|&nbsp;|\\s|　|<br\\s*(\/)?>)*<\/p(>|\\s+[^>]*>)',
            '(<br\\s*(\/)?>((\\s|&nbsp;|&nbsp|　)*)){2,}',
            '(<p(>|\\s+[^>]*>))((&nbsp|&nbsp;|\\s)*<br\\s*(\/)?>)*((.|\n|\r)*?<\/p(>|\\s+[^>]*>))'
        ];
        var vReplaces = [
            '',
            '<br>$3',
            '$1$6'
        ];
        for (var i = 0; i < vPatterns.length; i++) {
            var vRegExp = new RegExp(vPatterns[i], 'img');
            vReturn = vReturn.replace(vRegExp, vReplaces[i]);
        }
        return vReturn;
    },
    subStr: function (str, length) {
        if (str != null && str.length > 0) {
            str = str.length > length ? str.substring(0, length) + '...' : str;
        }
        return str;
    },
    DateTimeCompare: function (start, end) {
        return Date.parse(start.replace(/-/g, "/")) - Date.parse(end.replace(/-/g, "/"));
    },
    Trim: function (str) {
        //return str.replace(/(^\s*)|(\s*$)/g, "");
        return str.replace(/^\s+|\s+$/gm, '');
    },
    IsEmpty : function (obj) {
        if ($(obj).val() == null || $(obj).val().length == 0) {
            return true;
        }
        return false;
    }
});
