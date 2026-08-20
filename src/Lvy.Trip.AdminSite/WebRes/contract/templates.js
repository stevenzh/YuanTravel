
(function(sp) {
    var that = {};

    //游客名单列表



    that.tuoristlist = '{{#list}}<tr id="touristlist_{{index}}" class="tourist">' +
        '<td >' +
        '<input class="form-control btn-circle input-icon   padding-left-5" name="name" value="{{name}}" datatype="*" nullmsg="请输入游客姓名" placeholder="游客姓名"  type="text">' +
        '</td>' +
        '<td >' +
        '<div class="col-md-4">'+
        '<select class="form-control input-icon   padding-left-5 cardType"  name="cardType"  nullmsg="请选择游客证件类型" placeholder="" value="<%=cardType%>"   >'+


        '</select>'+
        '</div>'+
        '<div class="col-md-8">'+
        '   <input class="form-control btn-circle input-icon   padding-left-5" name="cardNo" value="{{cardNo}}" datatype="*" nullmsg="请输入游客证件号码" placeholder="请输入证件号码"  type="text">' +
        '</div>'+

        '</td>'+
        '<td >'+
         ' <select class="gender form-control input-icon   padding-left-5"  name="gender"  nullmsg="请选择游客性别" placeholder="" value="{{gender}}" >'+
         ' <option {{#boy}} selected {{/boy}} value="0" >男</option>'+
         '<option {{#girl}} selected {{/girl}} value="1"> 女</option>'+
        '</select>'+
        '</td>' +
        '<td >' +
        '<input class="form-control btn-circle input-icon   padding-left-5" name="age" value="{{age}}" datatype="*" nullmsg="请输入年龄" placeholder="请输入年龄"  type="text">' +
        '</td>' +
        '<td >' +
        '<input class="form-control btn-circle input-icon   padding-left-5" name="nation" value="{{nation }}"  nullmsg="请输入民族" placeholder="请输入民族"  type="text">' +
        '</td>' +

        '<td >' +
        '<input class="form-control btn-circle input-icon   padding-left-5" name="mobile" value="{{mobile}}" datatype="*" nullmsg="请输入手机号码" placeholder="请输入手机号码"  type="text">' +
        '</td>' +
        '<td >' +
        '</td>' +
        '<td >' +
        '<input class="form-control btn-circle input-icon   padding-left-5" name="health" value="{{health}}"  nullmsg="请输入健康状况" placeholder="请输入健康状况"  type="text">' +
        '</td>' +
        '<td class="text-center line-height-30">' +
        '<a class="delete" data-value="touristlist_{{index}}">删除</a>' +
        '</td>' +
       '</tr>{{/list}}';








     //获取图片显示
    that.urllist = '{{#list}} ' +
        '<li>' +
        '<div class="data-image">' +
        '<img src="{{src}}"></div><a href="javascript:void(0);" data-url="{{src}}" class="del-img-btn">删除</a></li>{{/list}}';



    //资源列表
    that.optionlist = '{{#list}}<option value="{{id}}" data-viewId="{{viewId}}">{{name}}</option>{{/list}}';
    that.optionlist2 = '{{#list}}<option value="{{viewId}}" data-viewId="{{viewId}}">{{name}}</option>{{/list}}';
    //数据统计分析资源列表
    that.dataoptionlist = '{{#list}}<option value="{{areaID}}">{{areaName}}</option>{{/list}}';

    //资源列表
    that.usermanagerlist = '{{#list}}<tr><td>{{idx}}</td><td>{{userName}}</td><td>{{roleName}}</td><td data-record="{{id}}"> <a  href="javascript:;" class="set-btns">修改</a><a class="del-btns">删除</a></td></tr>{{/list}}';

    //
    that.approvallist = '{{#list}}<tr><td>{{idx}}</td><td>{{orderId}}</td><td>{{travelName}}</td><td>{{govName}}</td><td>{{stateDesc}}</td><td data-record="{{orderId}}"> <a  href="javascript:;" class="set-btns">修改</a></td></tr>{{/list}}';

    //
    that.basicinfo = '{{#list}}<tr><td>{{orderId}}</td><td>{{travelName}}</td><td>{{govName}}</td><td>{{stateDesc}}</td><td>{{create_time}}</td></tr>{{/list}}';




    //数据统计分析-旅行社资质概况统计查询
    /*
     ** @旅行社信息库-列表
     */
    that.qualification_profile_list = '{{#list}}<tr>'
    that.qualification_profile_list += '	<td class="text-align-center">{{region}}</td>';
    that.qualification_profile_list += '	<td class="text-align-center">{{travel_number}}</td>';
    that.qualification_profile_list += '	<td class="text-align-center">{{exit_number}}</td>';
    that.qualification_profile_list += '	<td class="text-align-center">{{branch_number}}</td>';
    that.qualification_profile_list += '	<td class="text-align-center">{{website_number}}</td>';
    that.qualification_profile_list += '	<td class="text-align-center">{{wait_create_number}}</td>';
    that.qualification_profile_list += '</tr>{{/list}}';
    that.qualification_profile_list += '<tr>{{^list}}<td colspan="9">暂无记录</td>{{/list}}</tr>';

    /*旅游产品列表*/
    that.addProductList = '<ul >{{#list}}'
    that.addProductList += '	<li data-val="{{product_id}}">{{line_name}}</li>';
    that.addProductList += '{{/list}}</ul>';
    that.addProductList += '{{^list}}<ul ><div style="cursor: pointer;">暂无旅游产品记录</div></ul>{{/list}}';

    /*旅游产品列表*/
    that.additionContentList = '<ul >{{#list}}'
    that.additionContentList += '	<li class="mt-checkbox mt-checkbox-outline" data-val="" title="{{content}}">';
    that.additionContentList += ' <label class="mt-checkbox mt-checkbox-outline margin-none padding-none padding-10" style="padding: 5px!important;">';
    that.additionContentList += ' <input type="checkbox" value="0" >';
    that.additionContentList += '  {{head_content}}<span style="position: absolute;left: -20px;top:7px"></span></label></li>';
    that.additionContentList += '{{/list}}</ul>';
    that.additionContentList += '{{^list}}<ul ><div style="cursor: pointer;">暂无约定事项记录</div></ul>{{/list}}';



    window.templates = that;


})(window)
