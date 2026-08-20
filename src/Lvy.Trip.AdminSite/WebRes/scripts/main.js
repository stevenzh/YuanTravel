var int0 = 0;
var int1 = 0;

$(document).ready(function () {
    //input border change

    $("input[type='text'], textarea,select,input[type='checkbox'],input[type='radio'] ").focus(function () {
        $(this).css('border', '1px solid #5BB9EC');
    });

    $("input[type='text'], textarea,select,input[type='checkbox'],input[type='radio'] ").focusout(function () {
        $(this).css('border', '1px solid #CCCCCC');
    });

    NavChange();

    $('.nav_left li[class!=bac_line]').hover(
        function () {
            var true_index = $(this).index();
            var index = true_index / 2;
            var lilength = $('.nav_left li[class!=bac_line]').length;

            //alert(index);
            //如果不是最后一个li，那么隐藏当前li的上面和下面的背景图片li（bac_line）

            if (true_index == 0) {
                $('.nav_left li').eq(1).attr('class', 'bac_line_hidden');
            } else if (true_index == $('.nav_left li').length - 1) {
                $('.nav_left li').eq(true_index - 1).attr('class', 'bac_line_hidden');
            } else {
                $('.nav_left li').eq(true_index + 1).attr('class', 'bac_line_hidden');
                $('.nav_left li').eq(true_index - 1).attr('class', 'bac_line_hidden');
            }



            if (index == lilength - 1 && index != 0) {

            } else if (index != 0) {
                $(this).css('border-bottom', '2px solid #5DB9EC');
                $(this).css('margin-bottom', '1px');
                //border-top:2px solid #5DB9EC;

            } else {
                $(this).find('#nav_left_border').attr('class', 'nav_left_border_0');
            }
            $(this).find('#hidden_arrow').attr('class', 'hidden_arrow_' + index);
            $(this).find('#hidden_arrow').css('display', 'block');
            $(this).find('#hidden_arrow').attr('style', 'border:2px solid #5DB9EC;');
            $(this).find('#nav_left_border').css('display', 'block');
            $(this).find('#nav_left_arrow').attr('class', 'nav_left_arrow_1');
            $(this).attr('class', 'hover_' + index);
            //$('#m_left_bac').attr('class','m_left_bac_'+index);

        },
        function () {

            var true_index = $(this).index();
            var index = true_index / 2;
            var lilength = $('.nav_left li[class!=bac_line]').length;
            //
            if (index == lilength - 1 && index != 0) {

            } else if (index != 0) {
                $(this).css('border-bottom', '');
                $(this).css('margin-bottom', '0px');
                //border-top:2px solid #5DB9EC;
            } else {

            }
            $(this).find('#hidden_arrow').attr('style', 'display:none');
            //$(this).find('#hidden_arrow').css('display','none');
            $(this).find('#nav_left_arrow').attr('class', 'nav_left_arrow');
            $(this).attr('class', '');
            $('.bac_line_hidden').attr('class', 'bac_line');
            $('#m_left_bac').attr('class', 'm_left_bac');
            $(this).find('#nav_left_border').css('display', 'none');
            //
        }
    );

    /*nav_left*/

    /*sms_Schedule.php*/
    $("#post_button").hover(
        function () {
            $(this).attr('class', 'post_button_hover');
        },
        function () {
            $(this).attr('class', 'post_button');
        }
    );


    /**
    *print.html
    */

    $(".top_bt a, .bottom_bt a").hover(
        function () {
            className = $(this).attr('class');
            $(this).attr('class', className + '_1');
        },
        function () {
            $(this).attr('class', className);
        }

    );
    /**
    *print.html
    */

    /**
    *listtour2.html
    */


    //the more function for index page 
    //length = $(".time_list a").length
    //alert(length);
    $(".time_list a").click(function () {
        if ($(this).attr('class') == 'time_list_3') {

            if ($(this).parents('div').eq(0).find("table").css('display') == 'none') {
                $('.time_list_3').attr('style', '');
                $('.more_list').hide();
                $(this).attr('style', 'position:relative; *position:absolute; height:25px; _height:26px; border:2px solid #339F8F; border-bottom:0px; margin-top:-1px; background-color:#fff;');
                $(this).parents('div').eq(0).find("table").show();
            } else {
                $(this).attr('style', '');
                $(this).parents('div').eq(0).find("table").hide();
            }

        }
    });
});

function diffTime() {
    var d = new Date();
    var minutes = d.getMinutes();
    var seconds = d.getSeconds();
    var ms = d.getMilliseconds();
    return minutes * 60 + seconds + '.' + ms;
}

// 导航效果
function NavChange() {
    var className = "";
    $('.nav li').mouseover(function () {
        className = $(this).attr('class');
        if (className == undefined) { return; }
        if (className.search('li_hr') < 0) {
            $(this).attr('class', '');
        }
    }).mouseout(function () {
        if (className == undefined) { return; }
        if (className.search('li_hr') < 0) {
            $(this).attr('class', 'noback');
        }
    });
}


