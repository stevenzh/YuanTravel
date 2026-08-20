define(['jquery', 'bootstrap', 'art_template', 'underscore', 'text!/template/itinerary_new_data_structure.hbs', 'text!/template/onBehalfOfTouristList.hbs', 'text!/template/create_contract_enclosure_shanghai.hbs', 'text!/template/onBehalfOfTourist.hbs', 'allnational', 'contract','laydate','wangEditor','datepicker', 'layer', 'validform', 'select2', 'jq_upload'], function ($, bootstrap, template, _, itineraryListHtml, touristListHtml, contractEnclosureHtml, behalfOfTouristHtml, allna, contract,laydate,wangEditor) {
    editor = new wangEditor('#container');

    //行程模块模块
    $('body').append(itineraryListHtml);
    //企业签署
    $('body').append(behalfOfTouristHtml);
    //游客模块
    $('body').append(touristListHtml);
    //合同附件模块
    $('body').append(contractEnclosureHtml);

    //接口地址配置
    var ajaxConfig = {
        //保存地址
        saveContract: '/econtract/contract/saveV2',
        //获取合同内容
        detail: '/econtract/contract/detailV2',
    };
    //状态
    var STATE = {copy: 1, change: 0, resign: 3, copyV2: 5};
    //合同上传数量
    var maxUploadNum = 3;
    var contractPage = {
        //页面参数
        pageOption: {},
        //步骤
        step: 1,
        initObj: {},
        //初始化页面
        init: function (paramsObj) {
            var _this = this;
            _this.initObj = paramsObj;
            _this.resizePage();

            var _hash = window.location.hash ? window.location.hash : '#1';
            _this.step = _hash.replace('#', '');


            _this.getDetail();

            //返回提醒
            $('body').on("click", "#backBtn", function (e) {
                e.preventDefault();
                var _thisLink = this;
                if (_this.initObj.sourceEcNumber != "") {
                    layer.confirm('确认返回并放弃本次操作吗？', {
                        btn: ['取消', '立即返回'],
                    }, function (index, layero) {
                        layer.close(index);
                    }, function (indx, layero) {
                        window.location.href = "/travel/electronicContract/3";
                    });

                } else {
                    layer.confirm('是否要保存合同数据后再返回?', {
                        btn: ['保存', '立即返回'],
                    }, function (index, layero) {
                        layer.close(index);
                        _this.saveContract()
                    }, function (indx, layero) {
                        window.location.href = $(_thisLink).attr('href');
                    });
                }

            });


            //上一步
            $('body').on('click', '.prev-btn', function (e) {
                // e.preventDefault();
                _this.step--;
                _this.showStepWrapper();
            })

            //下一步
            $('body').on('click', '.next-btn', function (e) {
                // e.preventDefault();
                _this.step++;
                _this.showStepWrapper();
            })
            //保存合同,重签时不展示保存按钮
            if (_this.initObj.sourceEcNumber != "") {
                $(".save-contract-btn").remove()
            } else {
                $('body').on('click', '.save-contract-btn', function (e) {
                    e.preventDefault();
                    _this.saveContract();
                })
            }
        },
        //自适应页面宽度
        resizePage: function () {
            $('.fixed-header').css({
                'display': 'block',
                'width': $('.contract-wrapper').innerWidth() - 30,
            })
        },
        //显示当前步骤页面
        showStepWrapper: function () {
            var _this = this;
            $('.step-wrapper').addClass('dn');
            $('#step' + _this.step).removeClass('dn');

            $('.stepIco').removeClass('active');
            $('.stepIco:eq(' + (_this.step - 1) + ')').addClass('active');
            $('#all-main').scrollTop(0, 0);
        },
        //获取页面信息
        getDetail: function () {
            $('.loader-box').show();
            var _this = this;
            var detailParamsData = {};
            detailParamsData[_this.initObj.type] = _this.initObj.number;
            detailParamsData['operation'] = STATE[_this.initObj.typeState];
            detailParamsData['viewId'] = _this.initObj.viewId;
            var btnIds = _this.initObj.btnIds.split(",");
            $.ajax({
                url: ajaxConfig.detail,
                type: 'post',
                cache: false,
                dataType: 'json',
                data: $.extend({}, detailParamsData),
                success: function (data) {
                    $('.loader-box').fadeOut();
                    if (data.errorCode == 0) {
                        if(data.data.contractId || _this.initObj.typeState.indexOf("copy") >= 0){
                            _this.pageOption = data.data;
                        }else{
                            var tempData = {
                                cardType:data.data.cardType,
                                travelAgency:data.data.travelAgency,
                                dispute:data.data.dispute
                            };
                            _this.pageOption = $.extend(contract.createNew(),tempData);
                        }

                        //渲染第一步页面
                        var _html = template('stepTemplate1', _this.pageOption);
                        $('#step1').html(_html);

                        initItineraryListModel(_this.initObj.type, _this.initObj.number, _this.pageOption);

                        _this.pageOption.nationalarray = allna.nationals;
                        // //初始化游客信息
                        initTouristListModel(_this.pageOption);
                        init_behakf_of_tourist(_this.pageOption)
                        // //渲染第三步页面
                        _html = template('stepTemplate3', _this.pageOption);
                        $('#step3').html(_html);
                        
                        if(_this.pageOption.cost.includeNote && _this.pageOption.cost.includeNote.length > 0){
                            _this.pageOption.cost.includeNote.map(function(val,i){
                                $("input[name='costIncludeNote']").eq(val-1).prop("checked",true);
                            })
                        }

                        $("input[name='groupAgreementMode']").eq(0).click(function(){
                            if($("#entrustedTravelAgencyAgencyName")){
                                $("#entrustedTravelAgencyAgencyName").val("");
                            }
                            if($("#groupAgreementMergeToCompanyName")){
                                $("#groupAgreementMergeToCompanyName").val("");
                            }
                            if($("#travelAgencyLicenseNumber")){
                                $("#travelAgencyLicenseNumber").val("");
                            }
                        })
                        $("input[name='groupAgreementMode']").eq(1).click(function(){
                            if($("#groupAgreementMergeToCompanyName")){
                                $("#groupAgreementMergeToCompanyName").val("");
                            }
                        })
                        $("input[name='groupAgreementMode']").eq(2).click(function(){
                            if($("#entrustedTravelAgencyAgencyName")){
                                $("#entrustedTravelAgencyAgencyName").val("");
                            }
                        })


                        allna.init_document(_this.pageOption);

                        $("body").on("change", "#solveWay", function (e) {
                            e.preventDefault();
                            if ($(this).val() != 1) {
                                $("#commissionAgencyName").val("")
                            }
                        })
                        var dom_date_id = ['#startDate', '#endDate',"#deadline"];
                        dom_date_id.map(function (o) {
                            laydate.render({
                                elem: o, //指定元素
                                trigger: 'click'
                            });
                        })
                        //init购物或者自费协议
                        _this.initBuyOrCostList("voluntaryBuyTemplate", _this.pageOption, "#addBuyTemplate", ".delBuyTemplate", 1, false)
                        _this.initBuyOrCostList("voluntaryCostTemplate", _this.pageOption, "#addCostTemplate", ".delCostTemplate", 2, false)
                        //其他约定事项请求函数
                        additionContentList();

                        //渲染附件模块
                        var _html = template('contractEnclosureTemplate', _this.pageOption);
                        $('#contractEnclosure').html(_html)

                        //设置附件数量
                        var fileNum = _this.pageOption.attachments ? _this.pageOption.attachments.length : 0;
                        maxUploadNum = maxUploadNum - fileNum;

                        //渲染附件模块
                        initEnclosureModel(maxUploadNum);

                        //显示模块
                        _this.showStepWrapper();

                        //新创建入口
                        if (_this.initObj.type == 'expId') {
                            if (btnIds.indexOf('1') < 0) {
                                $('.save-contract-btn').attr('disabled', 'disabled')
                            }
                        }
                        //复制入口
                        if (_this.initObj.typeState == 'copy') {
                            if (btnIds.indexOf('3') < 0) {
                                $('.save-contract-btn').attr('disabled', 'disabled')
                            }
                        }
                        //修改入口
                        if (_this.initObj.type == 'ecNumber' && _this.initObj.typeState == 'change') {
                            if (btnIds.indexOf('7') < 0) {
                                $('.save-contract-btn').attr('disabled', 'disabled')
                            }
                        }

                        $('#contractForm').Validform({
                            btnSubmit: '.create-btn',
                            ajaxPost: true,
                            tipSweep: true,
                            datatype: {
                                "money": /(^[1-9]([0-9]+)?(\.[0-9]{1,2})?$)|(^(0){1}$)|(^[0-9]\.[0-9]([0-9])?$)/
                            },
                            tiptype: function (msg, o, cssctl) {
                                if (o.type == 3) {
                                    var _index = $(o.obj).parents('.step-wrapper').index();
                                    _this.step = _index + 1;
                                    _this.showStepWrapper()
                                    layer.msg(msg);
                                    $(o.obj).addClass("redBorder");
                                    $(o.obj).blur(function () {
                                        $(o.obj).removeClass("redBorder")
                                    });
                                }
                            },
                            beforeSubmit: function () {
                                var submitOption = _this.getSubmitOption();
                                var contractObject = contract.createNew();
                                var optionObject = $.extend(true,contractObject,submitOption);

                                $("input[name='costIncludeNote']").each(function(i,ele){
                                    if($(ele).prop("checked")){
                                        optionObject.cost.includeNote.push($(ele).val());
                                    }
                                });
                                $("input[name='groupAgreementMode']").each(function(idx,ele){
                                    if($(ele).prop("checked")){
                                        optionObject.itinerary.groupMode = $(ele).data("mode");
                                    }
                                });
                                optionObject.itinerary.inputType = route.mode;

                                if($("input[name='costIncludeNote']").eq(9).prop("checked") && $("#otherCost").val() == ""){
                                    _this.step = 1;
                                    _this.showStepWrapper();
                                    layer.msg("请填写其他旅游费用!");
                                    $('#otherCost').focus();
                                    $('#otherCost').addClass("redBorder");
                                    $('#otherCost').blur(function () {
                                        $('#otherCost').removeClass("redBorder")
                                    });
                                    return false;
                                }

                                

                                var phoneReg = /^1[3-9][0-9]{9}$/;
                                var checkedMode = 0;
                                $("input[name='groupAgreementMode']").each(function(idx,ele){
                                    if($(ele).prop("checked")){
                                        checkedMode ++;
                                    }
                                })
                                if(checkedMode  == 0){
                                    _this.step = 1;
                                    _this.showStepWrapper();
                                    layer.msg("请选择组团方式!");
                                    return false;
                                }

                                if($("input[name='groupAgreementMode']").eq(1).prop("checked")){
                                    if(optionObject.entrustedTravelAgency.agencyName ==""){
                                        _this.step = 1;
                                        _this.showStepWrapper();
                                        layer.msg($('#entrustedTravelAgencyAgencyName').attr('nullmsg'));
                                        $('#entrustedTravelAgencyAgencyName').focus();
                                        $('#entrustedTravelAgencyAgencyName').addClass("redBorder");
                                        $('#entrustedTravelAgencyAgencyName').blur(function () {
                                            $('#entrustedTravelAgencyAgencyName').removeClass("redBorder")
                                        });
                                        return false;
                                    }   
                                }

                                if (parseInt(optionObject.cost.totalCost) > 5000000) {
                                    layer.msg('合同总金额不能大于500万！');
                                    _this.step = 1;
                                    _this.showStepWrapper();
                                    $('#totalCost').focus();
                                    $('#totalCost').addClass("redBorder");
                                    $('#totalCost').blur(function () {
                                        $('#totalCost').removeClass("redBorder")
                                    });
                                    return false;
                                }

                                if($("input[name='groupAgreementMode']").eq(1).prop("checked")){
                                    if($("#travelAgencyLicenseNumber") && optionObject.entrustedTravelAgency.travelAgencyLicenseNumber ==""){
                                        _this.step = 1;
                                        _this.showStepWrapper();
                                        layer.msg($('#travelAgencyLicenseNumber').attr('nullmsg'));
                                        $('#travelAgencyLicenseNumber').focus();
                                        $('#travelAgencyLicenseNumber').addClass("redBorder");
                                        $('#travelAgencyLicenseNumber').blur(function () {
                                            $('#travelAgencyLicenseNumber').removeClass("redBorder")
                                        });
                                        return false;
                                    }   
                                }
                                if($("input[name='groupAgreementMode']").eq(2).prop("checked")){
                                    if(optionObject.groupAgreement.mergeToCompanyName ==""){
                                        _this.step = 1;
                                        _this.showStepWrapper();
                                        layer.msg($('#groupAgreementMergeToCompanyName').attr('nullmsg'));
                                        $('#groupAgreementMergeToCompanyName').focus();
                                        $('#groupAgreementMergeToCompanyName').addClass("redBorder");
                                        $('#groupAgreementMergeToCompanyName').blur(function () {
                                            $('#groupAgreementMergeToCompanyName').removeClass("redBorder")
                                        });
                                        return false;
                                    }
                                }
                                
                                if(new Date(optionObject.itinerary.startDate) > new Date(optionObject.itinerary.endDate)){
                                    _this.step = 1;
                                    _this.showStepWrapper();
                                    layer.msg('结束日期不能小于出发日期');
                                    $('#endDate').focus();
                                    $('#endDate').addClass("redBorder");
                                    $('#endDate').blur(function () {
                                        $('#endDate').removeClass("redBorder")
                                    });
                                    return false;
                                }
                                if(_this.initObj.viewId != 6){
                                    if(optionObject.localTravelAgencies[0].contactPhone == "" ){
                                        _this.step = 1;
                                        _this.showStepWrapper();
                                        layer.msg($('#localTravelAgenciesContactPhone').attr('errorMsg'));
                                        $('#localTravelAgenciesContactPhone').focus();
                                        $('#localTravelAgenciesContactPhone').addClass("redBorder");
                                        $('#localTravelAgenciesContactPhone').blur(function () {
                                            $('#localTravelAgenciesContactPhone').removeClass("redBorder")
                                        });
                                        return false;
                                    }
                                }

                                if(optionObject.paymentMethod == 4 && optionObject.cost.paymentMethodDescription == ""){
                                    _this.step = 1;
                                    _this.showStepWrapper();
                                    layer.msg($('#paymentOtherDescription').attr('nullmsg'));
                                    $('#paymentOtherDescription').focus();
                                    $('#paymentOtherDescription').addClass("redBorder");
                                    $('#paymentOtherDescription').blur(function () {
                                        $('#paymentOtherDescription').removeClass("redBorder")
                                    });
                                    return false;
                                }
                                if(optionObject.groupAgreement.resolution == 1 && optionObject.groupAgreement.leastCustomerNumber == ""){
                                    _this.step = 3;
                                    _this.showStepWrapper();
                                    layer.msg($('#leastCustomerNumber').attr('nullmsg'));
                                    $('#leastCustomerNumber').focus();
                                    $('#leastCustomerNumber').addClass("redBorder");
                                    $('#leastCustomerNumber').blur(function () {
                                        $('#leastCustomerNumber').removeClass("redBorder")
                                    });
                                    return false;
                                }
                                var agreeFail = 0;
                                if(optionObject.insurance.agreeToBuy == 1){

                                    if(optionObject.insurance.company == ""){
                                        _this.step = 3;
                                        _this.showStepWrapper();
                                        layer.msg($('#insuranceCompany').attr('nullmsg'));
                                        $('#insuranceCompany').focus();
                                        $('#insuranceCompany').addClass("redBorder");
                                        $('#insuranceCompany').blur(function () {
                                            $('#insuranceCompany').removeClass("redBorder")
                                        });
                                        agreeFail++;
                                    }
                                    if(optionObject.insurance.productName == ""){
                                        _this.step = 3;
                                        _this.showStepWrapper();
                                        layer.msg($('#insuranceProduct').attr('nullmsg'));
                                        $('#insuranceProduct').focus();
                                        $('#insuranceProduct').addClass("redBorder");
                                        $('#insuranceProduct').blur(function () {
                                            $('#insuranceProduct').removeClass("redBorder")
                                        });
                                        agreeFail++;
                                    }
                                    if(optionObject.insurance.premium == ""){
                                        _this.step = 3;
                                        _this.showStepWrapper();
                                        layer.msg($('#premium').attr('nullmsg'));
                                        $('#premium').focus();
                                        $('#premium').addClass("redBorder");
                                        $('#premium').blur(function () {
                                            $('#premium').removeClass("redBorder")
                                        });
                                        agreeFail++;
                                    }
                                    if(optionObject.insurance.coverage == ""){
                                        _this.step = 3;
                                        _this.showStepWrapper();
                                        layer.msg($('#coverage').attr('nullmsg'));
                                        $('#coverage').focus();
                                        $('#coverage').addClass("redBorder");
                                        $('#coverage').blur(function () {
                                            $('#coverage').removeClass("redBorder")
                                        });
                                        agreeFail++;
                                    }
                                }
                                if(agreeFail > 0){
                                    return false;
                                }
                                
                                if (optionObject.touristsInfo.tourists.length == 1) {
                                    optionObject.touristsInfo.tourists.map(function (o) {
                                        if (o.name == "" && o.phone == "" && o.ID.IDNumber == "") {
                                            optionObject.touristsInfo.tourists.splice(0, 1);
                                        }
                                    });
                                }

                                if (optionObject.signatory.isJoin == 0 && optionObject.touristsInfo.tourists.length == 0) {
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    layer.msg("请填写游客信息!");
                                    $('#name').focus();
                                    $('#name').addClass("redBorder");
                                    $('#name').blur(function () {
                                        $('#name').removeClass("redBorder")
                                    })
                                    return false;
                                }
                                if (optionObject.signatory.ID.IDType == 1 && !idCardVerify(optionObject.signatory.ID.IDNumber)) {
                                    layer.msg($('#beIDNumber').attr('errormsg'));
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    $('#beIDNumber').focus();
                                    $('#beIDNumber').addClass("redBorder");
                                    $('#beIDNumber').blur(function () {
                                        $('#beIDNumber').removeClass("redBorder")
                                    });
                                    return false;
                                }

                                var re1 = /^([a-zA-Z0-9-]*)$/
                                if(optionObject.signatory.ID.IDType == 2 && !re1.test(optionObject.signatory.ID.IDNumber)){
                                    layer.msg("游客代表护照号码格式错误！");
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    $('#beIDNumber').focus();
                                    $('#beIDNumber').addClass("redBorder");
                                    $('#beIDNumber').blur(function () {
                                        $('#beIDNumber').removeClass("redBorder")
                                    });
                                    return false;
                                }


                                if(optionObject.signatory.address.zip != '' && !zipcodeReg.test(optionObject.signatory.address.zip)){
                                    layer.msg($('#beZip').attr('errormsg'));
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    $('#beZip').focus();
                                    $('#beZip').addClass("redBorder");
                                    $('#beZip').blur(function () {
                                        $('#beZip').removeClass("redBorder")
                                    });
                                    return false;
                                }


                                if (submitOption.signatory.isComp == 1) {
                                    if (submitOption.signatory.loa.url == "" && submitOption.signatory.loa.fileName == "") {
                                        layer.msg("单位及机构用户请上传企业授权书！")
                                        _this.step = 2;
                                        _this.showStepWrapper();
                                        $("#license").addClass("redBorder");
                                        return false;
                                    }
                                }

                                var emailReg = /^(\w)+(\.\w+)*@(\w)+((\.\w{2,3}){1,3})$/;
                                if(optionObject.signatory.email != "" && !emailReg.test(optionObject.signatory.email)){
                                    layer.msg($("#beEmail").attr("errormsg"));
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    $('#beEmail').focus();
                                    $('#beEmail').addClass("redBorder");
                                    $('#beEmail').blur(function () {
                                        $('#beEmail').removeClass("redBorder")
                                    });
                                    return false;
                                }
                                
                               
                                //游客代表证件类型为身份证
                                var _isRepeat = false;
                                $.each(optionObject.touristsInfo.tourists, function (i, o) {
                                    if (optionObject.signatory.ID.IDType == o.ID.IDType && o.ID.IDNumber == optionObject.signatory.ID.IDNumber) {
                                        _isRepeat = true;
                                    }
                                })
                                if (_isRepeat) {
                                    layer.msg('游客代表与游客证件号码重复');
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    $('#beIDNumber').focus();
                                    $('#beIDNumber').addClass("redBorder");
                                    $('#beIDNumber').blur(function () {
                                        $('#beIDNumber').removeClass("redBorder")
                                    });
                                    return false;
                                }

                                var _flag = false;
                                $.each(optionObject.touristsInfo.tourists, function (i, o) {
                                    if (o.isSigner == 1 && o.phone == optionObject.signatory.phone) {
                                        _flag = true;
                                    }
                                })
                                if (_flag) {
                                    layer.msg('多人签署下，游客代表与游客的手机号不能重复');
                                    _this.step = 2;
                                    _this.showStepWrapper();
                                    $('#bePhone').focus();
                                    $('#bePhone').addClass("redBorder");
                                    $('#bePhone').blur(function () {
                                        $('#bePhone').removeClass("redBorder")
                                    });
                                    return false;
                                }
                                //游客信息
                                if ($('.saveTourist').length > 0 && optionObject.signatory.isJoin == 0) {
                                    //有未保存的游客
                                    if (!saveTourist(optionObject)) {
                                        _this.step = 2;
                                        _this.showStepWrapper();
                                        return false;
                                    }
                                }
                                if (_this.checkRepeatTourist(optionObject)) {
                                    return false;
                                }

                                $('.loader-box').show();

                                $.ajax({
                                    url: ajaxConfig.saveContract,
                                    type: 'POST',
                                    data: optionObject,
                                    dataType: 'json',
                                    success: function (data) {
                                        $('.loader-box').fadeOut();
                                        if (data.errorCode == 0) {
                                            window.location.href = "/travel/electronicContract/create/predetail?ecNumber=" + data.data.ecNumber + "&viewId=2&type=0"
                                        } else {
                                            layer.msg('操作失败!错误信息:' + data.errorMessage);
                                            $('.loader-box').hide();
                                            //重新刷新其他约定事项列表
                                            additionContentList();
                                        }
                                    },
                                    error: function (jqXHR) {
                                        $('.loader-box').fadeOut();
                                        layer.msg('登录异常，请重新登录。');
                                    }
                                })
                                return false;
                            }
                        });

            //             //创建权限
                        if (btnIds.indexOf('5') >= 0) {
                            $('.create-btn').attr('disabled', false);
                        }
                    } else {
                        layer.msg('操作失败!错误信息:' + data.errorMessage);
                        $('.loader-box').hide();
                    }

                }
            });
        },
        //获取要提交数据
        getSubmitOption: function () {
            var _this = this;
            //数据
            var ecNumber = '';
            var expId = '';
            var contractName = '';
            if (_this.initObj.contractName == 'undefined') {
                contractName = verdict($('#contractName'));
            } else {
                contractName = _this.initObj.contractName;
            }

            if (verdict($('#expId')) != undefined) {
                expId = verdict($('#expId'));
            }

            //修改入口
            if (_this.initObj.type == 'ecNumber' && _this.initObj.typeState == 'change') {
                ecNumber = _this.initObj.number;
            }

            //新创建入口
            if (_this.initObj.type == 'expId') {
                expId = _this.initObj.number;
                ecNumber = verdict($('#ecNumber'));
            }
            //复制入口
            if (_this.initObj.typeState == 'copy') {
                expId = verdict($('#expId'));
                ecNumber = verdict($('#ecNumber'));
            }

            //修改的时候给ec_number的值,不是修改ec_number为空
            var enclosure = [];

            $('.file-name').each(function (i, o) {
                var file = {
                    url: $(o).attr('href'),
                    name: $(o).html()
                }

                enclosure.push(file);
            })

            //如果合同附件为空,传空字符串
            if (enclosure.length == 0) {
                enclosure = ""
            }

            var myItinerarys;
            if (saveSite()) {
                if (route.mode == 0) {
                    //标准模式
                    myItinerarys = route.dayList;
                } else {
                    //简易模式
                    myItinerarys = [];
                    var mySite = {
                        siteList: [
                            {
                                description: editor.txt.html()
                            }
                        ]
                    };
                    myItinerarys.push(mySite);
                }
            }
            return {
                templateId: expId,
                ecNumber: ecNumber,
                contractName: contractName,
                businessType: _this.initObj.businessType || verdict($('input[name=businessType]')),
                //行程模式
                itinerary:{
                    inputType: route.mode
                },
                //行程信息
                dayList: myItinerarys,
                //附件
                attachments:enclosure,
                shoppings: _this.eachBuyOrCostList("voluntaryBuyTemplate", 1),//购物;
                activities: _this.eachBuyOrCostList("voluntaryCostTemplate", 2),//自费
                //游客信息
                touristsInfo:{
                    tourists: _this.pageOption.tourists,
                },
                //游客代表信息
                signatory: {
                    isComp: $("input[name='isComp']:checked").val(),
                    name: verdict($("#beName")),
                    ID: {
                        IDType: verdict($("#beIDType")),
                        IDNumber: verdict($("#beIDNumber"))
                    },
                    phone: verdict($("#bePhone")),
                    email: verdict($("#beEmail")),
                    mode: "",
                    companyName: verdict($("#beCompanyName")),
                    businessLicenseNumber: verdict($("#beBusinessLicenseNumber")),
                    address: {
                        zip: "",
                        description: verdict($("#beDestinationAddress"))
                    },
                    gender: $("input[name='beGender']:checked").val(),
                    race: $("#beRace").val(),
                    isJoin: $("input[name='beIsJoin']:checked").val(),
                    loa: {
                        url: $("#signatoryLoadedFile").val(),
                        fileName: verdict($("#signatoryLoadedFileName"))
                    },
                },
                travelAgency: _this.pageOption.travelAgency,
            }
        },
        //保存合同
        saveContract: function () {
            var _this = this;

            clearEmptySite();

            if (verdict($('#routeName')) == '') {
                _this.step = 1;
                _this.showStepWrapper();
                layer.msg('请输入旅游线路名称');
                $('#routeName').focus();
                $('#routeName').addClass("redBorder");
                $('#routeName').blur(function () {
                    $("#routeName").removeClass("redBorder")
                });
                return false;
            }


            var submitOption = _this.getSubmitOption();
            var contractObject = contract.createNew();
            var optionObject = $.extend(true, contractObject,submitOption);
            $("input[name='groupAgreementMode']").each(function(idx,ele){
                if($(ele).prop("checked")){
                    optionObject.itinerary.groupMode = $(ele).data("mode");
                }
            });
            $("input[name='costIncludeNote']").each(function(i,ele){
                if($(ele).prop("checked")){
                    optionObject.cost.includeNote.push($(ele).val());
                }
            });

            if (optionObject.touristsInfo.tourists.length == 1) {
                optionObject.touristsInfo.tourists.map(function (o) {
                    if (o.name == "" && o.phone == "" && o.ID.IDNumber == "") {
                        optionObject.touristsInfo.tourists.splice(0, 1);
                    }
                });
            }
            $('.loader-box').show();
            $('.loader').html("保存中");
            $.ajax({
                url: ajaxConfig.saveContract,
                type: 'POST',
                data: optionObject,
                dataType: 'json',
                success: function (data) {
                    $('.loader-box').fadeOut();

                    if (data.errorCode == 0) {
                        layer.msg("保存成功");
                        //重新刷新其他约定事项列表
                        additionContentList();
                        // window.location.reload();
                        //window.location.href = '/travel/electronicContract/1';
                    } else {
                        layer.msg('操作失败!错误信息:' + data.errorMessage);
                        $('.loader-box').hide();
                    }
                    if (data.data && data.data.ecNumber) {
                        $('#ecNumber').val(data.data.ecNumber);
                    }
                },
                error: function (jqXHR) {
                    $('.loader-box').fadeOut();
                    layer.msg('登录异常，请重新登录。');
                }
            })
        },
        //检测重复游客
        checkRepeatTourist: function (optionObject) {
            var _this = this;
            var isRepeat = false;
            $.each(optionObject.touristsInfo.tourists, function (i, obj) {
                if (verdict($('#beIDType')) == obj.ID.IDType && verdict($('#beIDNumber')) == obj.ID.IDNumber) {
                    layer.msg('游客代表和游客证件号重复!');
                    $('#beIDNumber').focus();
                    $('#beIDNumber').addClass("redBorder")
                    $('#beIDNumber').blur(function () {
                        $('#beIDNumber').removeClass("redBorder")
                    });
                    _this.step = 2;
                    _this.showStepWrapper();
                    isRepeat = true;
                    return;
                }
                if (obj.isSigner == 1 && obj.phone == "") {
                    layer.msg('请输入签署游客的手机号码!');
                    _this.step = 2;
                    _this.showStepWrapper();
                    isRepeat = true;
                    return;
                }
            })
            return isRepeat;
        },
        //初始自愿购物活动/自愿付费项目
        initBuyOrCostList: function (id, data, add, del, num, isproduct) {

            var _this = this;
            var domId = "#a" + id;
            data = {};
            
            var _html = template(id, _this.pageOption);
            $(domId).html(_html);


            $(document).off("click", add);
            $(document).off("click", del);

            var myBuyOrCost = _this.eachBuyOrCostList(id, num);
            var addList = {};
            //添加天数购物
            $(document).on("click", add, function () {
                if (num == 1) {
                    addList = {
                        date: "",
                        place: "",
                        shoppingPlace: "",
                        goods: "",
                        memo: "",
                        stayDuration: "",
                    }
                } else {
                    addList = {
                        date: "",
                        place: "",
                        item: "",
                        fee: "",
                        memo: "",
                        stayDuration: "",
                    }
                }
                myBuyOrCost = _this.eachBuyOrCostList(id, num);
                myBuyOrCost.push(addList);
                var template_data = {};
                if(num == 1){
                    template_data.shoppings = myBuyOrCost
                }else{
                    template_data.activities = myBuyOrCost
                }
                var _html = template(id,template_data);
                $("#a" + id).html(_html);
            });
            //删除天数购物
            $(document).on("click", del, function () {
                $(this).parent().parent().remove();
                myBuyOrCost = _this.eachBuyOrCostList(id, num);
                var template_data = {};
                if(num == 1){
                    template_data.shoppings = myBuyOrCost
                }else{
                    template_data.activities = myBuyOrCost
                }
                var _html = template(id,template_data);
                $("#a" + id).html(_html);
            })

        },
        //遍历行程单自愿购物活动/自愿付费项目
        eachBuyOrCostList: function (id, num) {
            var myBuyOrCost = [];
            var id = "#a" + id + " tbody tr";

            $(id).each(function (i, v) {
                var mySite = {};


                if (num == "1") {//购物
                    mySite = {
                        date: verdict($(v).find("td input[name=date]")),
                        place: verdict($(v).find("td input[name=place]")),
                        shoppingPlace: verdict($(v).find("td input[name=shoppingPlace]")),
                        goods: verdict($(v).find("td input[name=mainGoods]")),
                        memo: verdict($(v).find("td input[name=others]")),
                        stayDuration: verdict($(v).find("td input[name=lstayTime]")),   

                    };

                } else {//付费
                    mySite = {
                        date: verdict($(v).find("td input[name=date]")),
                        place: verdict($(v).find("td input[name=place]")),
                        item: verdict($(v).find("td input[name=item]")),
                        fee: verdict($(v).find("td input[name=price]")),
                        memo: verdict($(v).find("td input[name=others]")),
                        stayDuration: verdict($(v).find("td input[name=time]")),
                    };

                }

                myBuyOrCost.push(mySite);
            })

            return myBuyOrCost;
        }
    }
    return contractPage;
})