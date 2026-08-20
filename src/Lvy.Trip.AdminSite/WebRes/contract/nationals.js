define(['jquery', 'datepicker'], function ($) {
    var model = {
        nationals: [{
            value: '汉族',
            title: '汉族'
        },
            {
                value: '壮族',
                title: '壮族'
            },
            {
                value: '满族',
                title: '满族'
            },
            {
                value: '回族',
                title: '回族'
            },
            {
                value: '苗族',
                title: '苗族'
            },
            {
                value: '维吾尔族',
                title: '维吾尔族'
            },
            {
                value: '土家族',
                title: '土家族'
            },
            {
                value: '彝族',
                title: '彝族'
            },
            {
                value: '蒙古族',
                title: '蒙古族'
            },
            {
                value: '藏族',
                title: '藏族'
            },
            {
                value: '布依族',
                title: '布依族'
            },
            {
                value: '侗族',
                title: '侗族'
            },
            {
                value: '瑶族',
                title: '瑶族'
            },
            {
                value: '朝鲜族',
                title: '朝鲜族'
            },
            {
                value: '白族',
                title: '白族'
            },
            {
                value: '哈尼族',
                title: '哈尼族'
            },
            {
                value: '哈萨克族',
                title: '哈萨克族'
            },
            {
                value: '黎族',
                title: '黎族'
            },
            {
                value: '傣族',
                title: '傣族'
            },
            {
                value: '畲族',
                title: '畲族'
            },
            {
                value: '傈僳族',
                title: '傈僳族'
            },
            {
                value: '仡佬族',
                title: '仡佬族'
            },
            {
                value: '东乡族',
                title: '东乡族'
            },
            {
                value: '高山族',
                title: '高山族'
            },
            {
                value: '拉祜族',
                title: '拉祜族'
            },
            {
                value: '水族',
                title: '水族'
            },
            {
                value: '佤族',
                title: '佤族'
            },
            {
                value: '纳西族',
                title: '纳西族'
            },
            {
                value: '羌族',
                title: '羌族'
            },
            {
                value: '土族',
                title: '土族'
            },
            {
                value: '仫佬族',
                title: '仫佬族'
            },
            {
                value: '锡伯族',
                title: '锡伯族'
            },
            {
                value: '柯尔克孜族',
                title: '柯尔克孜族'
            },
            {
                value: '达斡尔族',
                title: '达斡尔族'
            },
            {
                value: '景颇族',
                title: '景颇族'
            },
            {
                value: '毛南族',
                title: '毛南族'
            },
            {
                value: '撒拉族',
                title: '撒拉族'
            },
            {
                value: '布朗族',
                title: '布朗族'
            },
            {
                value: '塔吉克族',
                title: '塔吉克族'
            },
            {
                value: '阿昌族',
                title: '阿昌族'
            },
            {
                value: '普米族',
                title: '普米族'
            },
            {
                value: '鄂温克族',
                title: '鄂温克族'
            },
            {
                value: '怒族',
                title: '怒族'
            },
            {
                value: '京族',
                title: '京族'
            },
            {
                value: '基诺族',
                title: '基诺族'
            },
            {
                value: '德昂族',
                title: '德昂族'
            },
            {
                value: '保安族',
                title: '保安族'
            },
            {
                value: '俄罗斯族',
                title: '俄罗斯族'
            },
            {
                value: '裕固族',
                title: '裕固族'
            },
            {
                value: '乌孜别克族',
                title: '乌孜别克族'
            },
            {
                value: '门巴族',
                title: '门巴族'
            },
            {
                value: '鄂伦春族',
                title: '鄂伦春族'
            },
            {
                value: '独龙族',
                title: '独龙族'
            },
            {
                value: '塔塔尔族',
                title: '塔塔尔族'
            },
            {
                value: '赫哲族',
                title: '赫哲族'
            },
            {
                value: '珞巴族',
                title: '珞巴族'
            }
        ],
        DateDiff: function (d1, d2) {
            var day = 24 * 60 * 60 * 1000;
            try {
                var dateArr = d1.split("-");
                var checkDate = new Date();
                checkDate.setFullYear(dateArr[0], dateArr[1] - 1, dateArr[2]);
                var checkTime = checkDate.getTime();

                var dateArr2 = d2.split("-");
                var checkDate2 = new Date();
                checkDate2.setFullYear(dateArr2[0], dateArr2[1] - 1, dateArr2[2]);
                var checkTime2 = checkDate2.getTime();

                var cha = (checkTime - checkTime2) / day;
                return Math.round(cha);
            } catch (e) {
                return false;
            }
        },
        init_document: function (data) {
            var _this = this;
            if ($("#exitDate")) {
                if (!$(this).hasClass("asDatepicker")) {
                    $("#exitDate").datepicker({
                        onceClick: true,
                        onChange: function (o) {
                            if ($("#entryDate").val() != "") {
                                var nights = _this.DateDiff($("#entryDate").val(), o[0]);
                                var day = nights + 1;
                                if (nights < 0) {
                                    layer.msg('出发时间不能大于结束时间');
                                    $('#exitDate').focus();
                                    $('#exitDate').addClass("redBorder");
                                    return false;
                                }
                                if ($("#entryDate").hasClass("redBorder")) {
                                    $('#entryDate').removeClass("redBorder")
                                }
                                if ($("#exitDate").hasClass("redBorder")) {
                                    $('#exitDate').removeClass("redBorder")
                                }
                                $("#days").val(day);
                                $("#nights").val(nights);
                                if ($("#dayNum")) {
                                    $("#dayNum").val(day)
                                }
                            }
                        }
                    })
                }
            }
            if ($("#entryDate")) {
                if (!$(this).hasClass("asDatepicker")) {
                    $("#entryDate").datepicker({
                        onceClick: true,
                        onChange: function (o) {
                            if ($("#exitDate").val() != "") {
                                var nights = _this.DateDiff(o[0], $("#exitDate").val());
                                var day = nights + 1;
                                if (nights < 0) {
                                    layer.msg('结束时间不能小于出发时间');
                                    $('#entryDate').focus();
                                    $('#entryDate').addClass("redBorder");
                                    return false;
                                }
                                if ($("#entryDate").hasClass("redBorder")) {
                                    $('#entryDate').removeClass("redBorder")
                                }
                                if ($("#exitDate").hasClass("redBorder")) {
                                    $('#exitDate').removeClass("redBorder")
                                }
                                $("#days").val(day);
                                $("#nights").val(nights);
                                if ($("#dayNum")) {
                                    $("#dayNum").val(day)
                                }
                            }
                        }
                    })
                }
            }
            if ($("#payDate")) {
                $("#payDate").datepicker({
                    onceClick: true,
                    date: 'today',
                    initValue: true,
                });
            }
            if ($(".asDatepicker")) {
                $('.asDatepicker').datepicker({
                    onceClick: true,
                });
            }
            if ($("#payWay") && data.payWay == undefined) {
                $("#payWay").find("option[value='2']").attr("selected", true);
            }

            $(".js-travel-agency-data-array").select2({
                width: '100%',
                placeholder: "请选择旅行社",
                ajax: {
                    url: function (params) {
                        return "/econtract/agencylist?name=" + encodeURI(params.term)
                    },
                    async: false,
                    dataType: 'json',
                    delay: 500,
                    data: function (params) {
                        return '';
                    },
                    processResults: function (data, params) {
                        if (data.errorCode == '4000') {
                            layer.msg("登录超时或此账号在其他位置登录，请重新登录。");
                            setTimeout(function () {
                                window.location.reload();
                            }, 2000)
                        } else if (data.errorCode == '12301522') {
                            layer.msg("请至少输入四位中文字符。");
                            return {};
                        } else {
                            return {
                                results: data.data,
                            };
                        }
                    },
                    cache: true
                },
                escapeMarkup: function (markup) {
                    return markup;
                },
                minimumInputLength: 4,
                templateResult: function (repo) {
                    if (repo.loading) return repo.text;
                    var markup = "<div class='select2-result-repository clearfix'>" + repo.name + "</div>";
                    return markup;
                },
                templateSelection: function (repo) {
                    if (repo.name) {
                        _this.getInfo(repo.id,repo.type);
                        return '<span>' + repo.name + '</span>';
                    } else {
                        return repo.text;
                    }
                }
            });
            $('.js-travel-agency-data-array').each(function(_i, _o) {
                if($(_o).attr('data-init')) {
                    $(_o).parent().find('.select2-selection__rendered').html($(_o).attr('data-init'));
                }
            });


            var valueZeroDocuments = ["childrenCost", "childrenNum", "bedChildrenCost", "noBedChildrenCost"];

            valueZeroDocuments.map(function (value) {
                if ($("#" + value) && data[value] == undefined) {
                    $("#" + value).val(0);
                }
            })
        },
        getInfo: function (id, type) {
            $('.loader-box').show();
            $.ajax({
                url: "/econtract/agencyinfo?id=" + id + "&type=" + type,
                type: 'get',
                cache: false,
                dataType: 'json',
                success: function (data) {
                    $("#delegateAgencyAddress").val(data.data.agencyAddress.description);
                    $("#delegateAgencyLicense").val(data.data.travelAgencyLicenseNumber);
                    $("#delegateAgencyBusinesstype").val(data.data.businessScope);
                    $('.loader-box').fadeOut();
                }
            })
        }
    }
    return model;
})