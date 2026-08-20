(function ($) {
    function ExfForms() {
        this._target = null;
        this.render = null; this.json = {};
        this._default = {
            url: null,
            rules: {},
            exp: null,
            errorTag: "label",
            errorAlert: false,
            errorPlacement: null,
            errorClass: null,
            validClass: null,
            validMsg: "",
            keyCode: -1,
            showmsg: null,
            isfocus: true,
            iskeyup: false,
            renderTo: null,
            dataType: null,
            isname: false,
            submit: null,
            keydown: null,
            onsubmit: false,
            success: null
        };
        this.focusinput = null;
        this._iscreate = false;
        this._options = {};
        this._rules = {}
    }
    $.exfforms = new ExfForms();
    $.exfforms.format = function (source, params) {
        if (arguments.length == 1) {
            return function () {
                var args = $.makeArray(arguments); args.unshift(source);
                return $.exfforms.format.apply(this, args)
            }
        }
        if (arguments.length > 2 && params.constructor != Array) {
            params = $.makeArray(arguments).slice(1)
        }
        if (params.constructor != Array) {
            params = [params]
        }
        $.each(params, function (i, n) {
            source = source.replace(new RegExp("\\{" + i + "\\}", "g"), n)
        });
        return source
    };
    $.extend(ExfForms.prototype, {
        messages: {
            regex: "\u8bf7\u4fee\u6b63\u8be5\u5b57\u6bb5",
            required: "\u5fc5\u586b",
            remote: "\u8bf7\u4fee\u6b63\u8be5\u5b57\u6bb5",
            email: "\u8bf7\u8f93\u5165\u5408\u6cd5\u90ae\u4ef6\u683c\u5f0f",
            url: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u7f51\u5740",
            date: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u65e5\u671f",
            dateISO: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u65e5\u671f (ISO).",
            time: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u65f6\u95f4",
            number: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u6570\u5b57",
            digits: "\u8bf7\u8f93\u5165\u6574\u6570",
            creditcard: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u4fe1\u7528\u5361\u53f7",
            equalTo: "\u8bf7\u518d\u6b21\u8f93\u5165\u76f8\u540c\u7684\u503c",
            accept: "\u8bf7\u8f93\u5165\u62e5\u6709\u5408\u6cd5\u540e\u7f00\u540d\u7684\u5b57\u7b26\u4e32",
            minlength: $.exfforms.format("\u5b57\u6570\u4e0d\u80fd\u5c0f\u4e8e{0}\u4e2a\u5b57\u7b26"),
            maxlength: $.exfforms.format("\u5b57\u6570\u4e0d\u80fd\u5927\u4e8e{0}\u4e2a\u5b57\u7b26"),
            min: $.exfforms.format("\u503c\u4e0d\u80fd\u5c0f\u4e8e{0}"),
            max: $.exfforms.format("\u503c\u4e0d\u80fd\u5927\u4e8e{0}"),
            range: $.exfforms.format("\u8bf7\u8f93\u5165\u4e00\u4e2a\u4ecb\u4e8e {0} \u548c {1} \u4e4b\u95f4\u7684\u503c"),
            rangelength: $.exfforms.format("\u8bf7\u8f93\u5165\u4e00\u4e2a\u957f\u5ea6\u4ecb\u4e8e {0} \u548c {1} \u4e4b\u95f4\u7684\u5b57\u7b26\u4e32"),
            zipcode: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u90ae\u653f\u7f16\u7801",
            user: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u7528\u6237\u540d",
            pass: "\u8bf7\u8f93\u5165\u5408\u6cd5\u7684\u5bc6\u7801",
            ip4: "\u8bf7\u8f93\u5165\u6b63\u786e\u7684IP\u5730\u5740",
            ip6: "\u8bf7\u8f93\u5165\u6b63\u786e\u7684IP6\u5730\u5740",
            mac: "\u8bf7\u8f93\u5165\u6b63\u786e\u7684MAC\u5730\u5740"
        },
        setOptions: function (settings) {
            var config = $.extend({}, $.exfforms._default);
            $.extend(config, settings);
            $.extend($.exfforms._options, config)
        },
        exfmonths: {
            regex: function (value, param) {
                if (typeof param == "object") {
                    return param.test(value)
                } else {
                    return new RegExp(param).test(value)
                }
            },
            required: function (value) {
                return value.replace(/(^ *)|( *$)/g, "").length > 0
            },
            haha: function (value) {
                return value.replace(/(^ *)|( *$)/g, "").length > 0 && value.split('.')[1].toString() == "doc"
            }, digits: function (value) {
                return /^\d+$/.test(value)
            },
            phone: function (value) {
                return /^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$/.test(value)
            },
            number: function (value) {
                return /^-?(?:\d+|\d{1,3}(?:,\d{3})+)(?:\.\d+)?$/.test(value)
            }, dateISO: function (value) {
                return /^\d{4}[\/-]\d{1,2}[\/-]\d{1,2}$/.test(value)
            }, date: function (value) {
                return !/Invalid|NaN/.test(new Date(value))
            }, time: function (value) {
                return /^\d{1,2}:\d{1,2}(:\d{1,2})?$/.test(value)
            }, url: function (value) {
                return /^(https?|ftp):\/\/(((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:)*@)?(((\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5])\.(\d|[1-9]\d|1\d\d|2[0-4]\d|25[0-5]))|((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?)(:\d*)?)(\/((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)+(\/(([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)*)*)?)?(\?((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|[\uE000-\uF8FF]|\/|\?)*)?(\#((([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(%[\da-f]{2})|[!\$&'\(\)\*\+,;=]|:|@)|\/|\?)*)?$/i.test(value)
            }, email: function (value) {
                return /^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/i.test(value)
            },
            min: function (value, param) {
                return value >= param
            }, max: function (value, param) {
                return value <= param
            }, minlength: function (value, param) {
                return value.length >= param
            }, maxlength: function (value, param) {
                return value.length <= param
            }, range: function (value, param) {
                return value >= param[0] && value <= param[1]
            }, rangelength: function (value, param) {
                var length = $.trim(value).length; return length >= param[0] && length <= param[1]
            }, accept: function (value, param) {
                param = typeof param == "string" ? param.replace(/,/g, "|") : "png|jpe?g|gif"; return value.match(new RegExp(".(" + param + ")$", "i")) != null
            }, equalTo: function (value, param) {
                if (param.constructor == String && (param.substr(0, 1) == "#" || param.substr(0, 1) == ".")) { return value == $(param).val() } else { return value == param.toString() }
            }, creditcard: function (value) {
                if (/[^0-9-]+/.test(value)) { return false } var nCheck = 0, nDigit = 0, bEven = false; value = value.replace(/\D/g, ""); for (var n = value.length - 1; n >= 0; n--) { var cDigit = value.charAt(n); var nDigit = parseInt(cDigit, 10); if (bEven) { if ((nDigit *= 2) > 9) { nDigit -= 9 } } nCheck += nDigit; bEven = !bEven } return (nCheck % 10) == 0
            }, zipcode: function (value) {
                return /^[\d]{6}$/.test(value)
            }, user: function (value) {
                return /^[a-zA-Z]+[\w-]{2,20}$/.test(value)
            }, pass: function (value) {
                return /^[\w@$*#_]{6,20}$/.test(value)
            }, ip4: function (value) {
                return /^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$/.test(value)
            }, ip6: function (value) {
                return /^([\dA-Fa-f]{4})::([\dA-Fa-f]{4}):([\dA-Fa-f]{4}):([\dA-Fa-f]{4}):([\dA-Fa-f]{4})\%([\dA-Fa-f]{2})$/.test(value)
            }, mac: function (value) {
                return /^([\dA-Fa-f]{1,2})-([\dA-Fa-f]{1,2})-([\dA-Fa-f]{1,2})-([\dA-Fa-f]{1,2})-([\dA-Fa-f]{1,2})-([\dA-Fa-f]{1,2})$/.test(value)
            }, remote: function (value, param) { }
        }, _getName: function (input) {
            if (input.attr("type") == "radio") { return input.attr("name") } if (this._options.isname && input.attr("name") != "") { return input.attr("name") } else { return input.attr("id") }
        }, _getTarget: function (name) {
            if (this._options.isname) { return this._target.find(":input[name='" + name + "']") } else { return this._target.find("#" + name) }
        }, _getElement: function (exfor, msg, css, targetElement) {
            var element = this._target.find(this._options.errorTag + "[exfor='" + exfor + "']"); if (element.length == 0) { element = $("<" + this._options.errorTag + ">"); element.attr("exfor", exfor); if (!$.isFunction(this._options.errorPlacement)) { element.insertAfter(targetElement); targetElement.after("&nbsp;") } } if (css != null) { element.addClass(css) } element.html(msg); element.show(); if ($.isFunction(this._options.errorPlacement)) { var isexit = this._options.errorPlacement(element, targetElement, exfor); return isexit } return true
        }, _setval: function (input) {
            var _name = this._getName(input); if (_name == "") { return } if (input.attr("type") == "checkbox") { if (input.val() == "on") { this.json[_name] = input.checked() } else { if (input.checked()) { this.json[_name] = input.val() } else { this.json[_name] = "" } } } else { this.json[_name] = input.val() } if (!(input.attr("regex") === undefined)) { eval("var obj = " + input.attr("regex")); this._rules[_name] = obj }
        }, _checking: function (key, rule, value) {
            this._target.find(this._options.errorTag + "[exfor='" + key + "']").hide();
            var $this = this;
            var msg = null;
            var result = true;
            var resultAll = true;
            var isexit = false;
            if (rule.hasOwnProperty("required") && rule.required == false && !$this.exfmonths.required(value)) {
                return [true, false]
            }
            if ($.isEmptyObject(rule)) {
                rule = { required: true }
            }
            $.each(rule, function (prop, _val) {
                var _msg = null;
                var val = _val;
                if (_val.constructor == Object) {
                    if (_val.hasOwnProperty("value")) {
                        val = _val.value
                    }
                    if (_val.hasOwnProperty("msg")) {
                        _msg = _val.msg
                    }
                }
                if (val == null) { return }
                if (val === false) { return }
                if ($.isFunction(val)) {
                    var _re = val(value);
                    if (_re.constructor == Array) { result = _re[0]; msg = _re[1] }
                    else { result = _re; msg = "\u8bf7\u4fee\u6b63\u8be5\u5b57\u6bb5\uff01" }
                } else {
                    result = $this.exfmonths[prop](value, val);
                    msg = $this.messages[prop];
                    if (!result && $.isFunction($this.messages[prop])) { msg = $this.messages[prop](val) }
                }
                var input = $this._getTarget(key);
                if (result == false) {
                    var errorMsg = _msg == null ? msg : _msg;
                    if ($this.focusinput == null) {
                        $this.focusinput = input
                    }
                    if ($this._options.errorAlert) {
                        alert(errorMsg);
                        isexit = true
                    } else {
                        if ($.isFunction($this._options.showmsg)) {
                            $this._options.showmsg(errorMsg, input);
                            isexit = true
                        } else {
                            var isrun = $this._getElement(key, errorMsg, $this._options.errorClass, input);
                            if (isrun == false) { isexit = true } else { isexit = false }
                        }
                    } return false
                } else {
                    if ($this._options.validClass != null || $this._options.validMsg != "") {
                        $this._getElement(key, $this._options.validMsg, $this._options.validClass, input)
                    }
                }
            });
            return [result, isexit]
        }, _checkval: function (rules) {
            var $this = this; var result = true; var resultAll = true; var _result = 1; $.each(rules, function (key, rule) { var value = $this.json[key]; if (value == null) { value = "" } var resultArr = $this._checking(key, rule, value); if (resultArr[0] == false) { _result = 0 } if (resultArr[1] == true) { _result = 2; return false } }); return _result
        }, _getJson: function () { var $this = this; if (this._options.exp != null) { forms = $this._target.find(this._options.exp) } else { forms = $this._target.find(":text,:password,:checkbox,:file,input:hidden,select,textarea") } forms.each(function () { $this._setval($(this)) }); if (this._options.exp == null) { this._target.find(":radio[checked]").each(function () { $this._setval($(this)) }) } return this.json }, submit: function () {
            this.focusinput = null; this._rules = {}; this._getJson(); var result = 0, result1 = 0; result = this._checkval(this._options.rules); if (result != 2) { result1 = this._checkval(this._rules) } var isok = (result == 1) && (result1 == 1); if (isok) { if ($.isFunction(this._options.submit)) { var isAlgin = this._options.submit(this.json, this); if (isAlgin == false) { return isok } } if (this._options.onsubmit == false) { if ($.isFunction(this._options.success) && this._options.url != null) { this._post() } } else { if (this._target.get(0).tagName == "FORM") { this._target.submit() } else { this._target.parents("form").submit() } } } if (this.focusinput != null) { $.exfforms.focusinput.focus() } return isok
        }, _post: function () { var $this = this; if (this._target.data("exfformerrorbind") == null) { this._target.data("exfformerrorbind", true); this._target.ajaxError(function (event, request, settings) { var html = request.responseText; if (html != "") { var reg = /\s*<title>(.*)?<\/title>\s*/igm; var t = reg.exec(html); $this._options.success("\u7cfb\u7edf\u9519\u8bef\uff1a" + t[1] + "URL\uff1a" + settings.url) } else { $this._options.success("\u7cfb\u7edf\u9519\u8bef\uff1a\u65e0\u6cd5\u627e\u5230\u8d44\u6e90\u3002URL\uff1a" + settings.url) } }) } $.post(this._options.url, this.json, this._options.success, this._options.dataType) }
    }); $.fn.exfforms = function (cfg) {
        if (!$.exfforms._target != this) { $.exfforms._target = this; $.exfforms.setOptions(cfg) } if ($.exfforms._iscreate == false) {
            $.exfforms._iscreate = true; if ($.exfforms._options.keyCode != -1) { $(document).keyup(function (event) { if (event.keyCode == $.exfforms._options.keyCode) { if ($.isFunction($.exfforms._options.keydown)) { var result = $.exfforms._options.keydown(); if (result == false) { return } } $.exfforms.submit() } }) } if ($.exfforms._options.isfocus || $.exfforms._options.iskeyup) {
                var forms = this.find(":text, :password, :file, select, textarea"); if ($.exfforms._options.isfocus) { forms.blur(function () { $.exfforms.focusinput = null; var name = $.exfforms._getName($(this)); var rule = null; if (!($(this).attr("regex") === undefined)) { eval("rule = " + $(this).attr("regex")) } else { rule = $.exfforms._options.rules[name] } if (rule != null) { $.exfforms._checking(name, rule, $(this).val()) } }) } if ($.exfforms._options.iskeyup) { forms.keyup(function () { $.exfforms.focusinput = null; var name = $.exfforms._getName($(this)); var rule = null; if (!($(this).attr("regex") === undefined)) { eval("rule = " + $(this).attr("regex")) } else { rule = $.exfforms._options.rules[name] } if (rule != null) { $.exfforms._checking(name, rule, $(this).val()) } }) } var checkbox = this.find(":radio, :checkbox"); checkbox.change(function () {
                    $.exfforms.focusinput = null; var name = $.exfforms._getName($(this)); var rule = null;
                    if (!($(this).attr("regex") === undefined)) { eval("rule = " + $(this).attr("regex")) }
                    else { rule = $.exfforms._options.rules[name] }
                    if (rule != null) {
                        if (this.type == "radio") { $.exfforms._checking(name, rule, $.exfforms._target.find(":radio[name='" + this.name + "'][checked]").val()) }
                        else { $.exfforms._checking(name, rule, $(this).attr("checked")) }
                    }
                })
            }
            if ($.exfforms._options.renderTo != null) {
                $.exfforms.render = $($.exfforms._options.renderTo);
                $.exfforms.render.click(function () {
                    $.exfforms.submit()
                });
                return null
            }
        }
        if ($.exfforms._options.renderTo == null) {
            return $.exfforms.submit()
        }
    }; $.extend($.fn, { formToArray: function () { $.exfforms._target = this; return $.exfforms._getJson() }, formSerialize: function () { return $.param(this.exfFormToArray()) }, clearForm: function () { var forms = this.find(":input"); return forms.each(function () { $(this).clearFields() }) }, clearFields: function () { return this.each(function () { var t = this.type, tag = this.tagName.toLowerCase(); if (t == "text" || t == "password" || tag == "textarea") { this.value = "" } else { if (t == "checkbox" || t == "radio") { this.checked = false } else { if (tag == "select") { this.selectedIndex = -1 } } } }) }, resetForm: function () { return this.each(function () { if (typeof this.reset == "function" || (typeof this.reset == "object" && !this.reset.nodeType)) { this.reset() } }) }, enable: function (b) { if (b == undefined) { b = true } return this.each(function () { this.disabled = !b }) }, checked: function (b) { if (b == null) { return this.attr("checked") } else { this.attr("checked", b) } }, selected: function (select) { if (select == undefined) { select = true } return this.each(function () { var t = this.type; if (t == "checkbox" || t == "radio") { this.checked = select } else { if (this.tagName.toLowerCase() == "option") { var $sel = $(this).parent("select"); if (select && $sel[0] && $sel[0].type == "select-one") { $sel.find("option").selected(false) } this.selected = select } } }) } }); $.extend({ cookie: function (name, value, options) { if (typeof value != "undefined") { options = options || {}; if (value === null) { value = ""; options.expires = -1 } var expires = ""; if (options.expires && (typeof options.expires == "number" || options.expires.toUTCString)) { var date; if (typeof options.expires == "number") { date = new Date(); date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000)) } else { date = options.expires } expires = "; expires=" + date.toUTCString() } var path = options.path ? "; path=" + options.path : ""; var domain = options.domain ? "; domain=" + options.domain : ""; var secure = options.secure ? "; secure" : ""; document.cookie = [name, "=", encodeURIComponent(value), expires, path, domain, secure].join("") } else { var cookieValue = null; if (document.cookie && document.cookie != "") { var cookies = document.cookie.split(";"); for (var i = 0; i < cookies.length; i++) { var cookie = jQuery.trim(cookies[i]); if (cookie.substring(0, name.length + 1) == (name + "=")) { cookieValue = decodeURIComponent(cookie.substring(name.length + 1)); break } } } return cookieValue } } })
})(jQuery);
