
var token = "oh, it's ok";
var redirect = "http://gbsaas.12301.cn/html/cors/result.html";
function fileUploadFuc(file_btn,file_wrap,_index) {
    var _html ='<div class="file_progress"><p class="profont">0%</p> <div class="plupload_progress_container"><p class="progress"></p></div></div>';
    $(file_wrap).eq(_index).after(_html);
    var urls=[];
    $(file_btn).fileupload({
        url: "https://fileupload.12301.cn/upload",
        autoUpload: false,
        dataType: 'json',
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png|bmp)$/i,
        maxFileSize: 20000000,
        add: function (e, data) {
            var file = data.files[0];
            if (!/(\.|\/)(gif|jpe?g|png|bmp)$/i.test(file.name)) {
                alert('只能上传图片文件');
                return
            } else if (file.size > 10485760) {
                alert('上传的文件太大了，请上传10m以下的文件');
                return
            }
            data.formData = {token: token};
            if (window.redirect) {
                data.redirect = redirect;
            }
            data.submit();
        },
        progressall: function (e, data) {
            $(file_wrap).eq(_index).next('.file_progress').show();
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $(file_wrap).eq(_index).next('.file_progress').find('.progress').css('width',progress + '%');//控制进度条
            $(file_wrap).eq(_index).next('.file_progress').find('.profont').html('' + progress + "%");
            if(progress >= 100) {
                $(file_wrap).eq(_index).next('.file_progress').hide();
            }
        },
        done: function (e, data) {
            var uploadResult = data && data.result;
            var list=[];
            var file = uploadResult[0];
            var isImg = false;
            if (file.type.indexOf('image') > -1) {
                isImg = true;
            }

            list.push({
                src: file.url,
                isImg: isImg,
                filename: file.name
            });
            var renderData = {
                list: list
            };
            var tpl = templates.urllist;
            var html = Mustache.render(tpl, renderData);
            $(file_wrap).eq(_index).find('.upload-image').append(html);
            urls.push(file.url);
            for(var i in urls) {
                if(urls[i] == $(this).data('url')) {
                    urls.splice(i, 1)
                }
            }
            $(file_btn).parent('.upload-boxs').find('.upload-list').val(urls.toString());
        },
        fail: function (e, data) {
            //  console.log(JSON.stringify(arguments));
            //  console.log(arguments);
        }
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');

    $('body').on('click', '.del-img-btn', function(e) {
        var _this = $(this);
        e.preventDefault();
        _this.parent().remove();
        for(var i in urls) {
            if(urls[i] == $(this).data('url')) {
                urls.splice(i, 1)
            }
        }
        $(file_btn).parent('.upload-boxs').find('.upload-list').val(urls.toString())
    })

}


function bindEvent(browsebtn,rednerbox,submitbtn) {
    var _html ='<div class="file_progress"><p class="profont">0%</p> <div class="plupload_progress_container"><p class="progress"></p></div></div>';
    $(rednerbox).after(_html);
    var urls=[];
    var _uploadList =$(browsebtn).parent('.upload-boxs').find('.upload-list');

    if((typeof _uploadList.val() !='undefined') && (_uploadList.val()!='')){
        var apply = $(browsebtn).parent('.upload-boxs').find('.upload-list').val().split(',');
        urls=urls.concat(apply);

    }
    $(browsebtn).fileupload({
        url: "https://fileupload.12301.cn/upload",
        // forceIframeTransport : true,
        autoUpload: false,
        dataType: 'json',
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png|bmp)$/i,
        maxFileSize: 5000000,
        add: function (e, data) {
            var file = data.files[0];
            if (!/(\.|\/)(gif|jpe?g|png|bmp)$/i.test(file.name)) {
                layer.msg('只能上传图片文件');
                return
            } else if (file.size > 10485760) {
                layer.msg('上传的文件太大了，请上传10m以下的文件');
                return
            }
            data.formData = {token: token};
            if (window.redirect) {
                data.redirect = redirect;
            }
            data.submit();

            $.each(submitbtn,function(i,v){
                $(v).attr("disabled","false")
            })
        },
        progressall: function (e, data) {
            $(rednerbox).next('.file_progress').show();
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $(rednerbox).next('.file_progress').find('.progress').css('width',progress + '%');//控制进度条
            $(rednerbox).next('.file_progress').find('.profont').html('' + progress + "%");
            if(progress >= 100) {
                $(rednerbox).next('.file_progress').hide();
            }
        },
        done: function (e, data) {
            $.each(submitbtn,function(i,v){
                $(v).removeAttr("disabled")
            })
            var uploadResult = data && data.result;
            var list=[];
            var file = uploadResult[0];
            var isImg = false;
            if (file.type.indexOf('image') > -1) {
                isImg = true;
            }
            var image = new Image();
            image.src = file.url;

            list.push({
                src: file.url,
                isImg: isImg,
                filename: file.name,
                file:file

            });

            var renderData = {
                list: list
            };
            var tpl = templates.urllist;
            var html = Mustache.render(tpl, renderData);
            $(rednerbox).find('.upload-image').append(html);
            urls.push(file.url);
            for(var i in urls) {
                if(urls[i] == $(this).data('url')) {
                    urls.splice(i, 1)
                }
            }


            $(browsebtn).parent('.upload-boxs').find('.upload-list').val(urls.toString());
        },
        fail: function (e, data) {


            $.each(submitbtn,function(i,v){
                $(v).removeAttr("disabled")
            })
            //  console.log(JSON.stringify(arguments));
            //  console.log(arguments);
        }
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');

    $('body').on('click', '.del-img-btn', function(e) {
        var _this = $(this);
        e.preventDefault();
        _this.parent().remove();
        for(var i in urls) {
            if(urls[i] == $(this).data('url')) {
                console.log(urls);
                urls.splice(i, 1)
            }
        }
        $(browsebtn).parent('.upload-boxs').find('.upload-list').val(urls.toString())
    })

}
// var that = {};
// that.init = function (options) {
//     options = options || {};
//     if (options.base_url) {
//         baseURL = options.base_url;
//     }
//     bindEvent();
// };
//
// that.init({base_url:"http://fileupload.5212301.com"});

var url = "/upload";
var baseURL = "https://fileupload.12301.cn";
function initUpload(wrapper) {
    var _html ='<div class="file_progress"><p class="profont">0%</p> <div class="plupload_progress_container"><p class="progress"></p></div></div>';
    $('#'+wrapper+' .upload-boxs').before(_html);
    var urls=[];
    if($('#'+wrapper+' .upload-list').val()!=''){
        var apply = $('#'+wrapper+' .upload-list').val().split(',');
        var _html = '';
        urls=urls.concat(apply);
        for (var i in apply) {
            var _url = apply[i];
            if (_url == '') {
                break;
            }
            var _up_url = _url.toUpperCase();
            if (_up_url.indexOf('.JPG') >= 0 || _up_url.indexOf('.PNG') >= 0 || _up_url.indexOf('.JPEG') >= 0 || _up_url.indexOf('.BPM') >= 0) {
                _html += '<li>'+
                    '<div class="data-image"><img src="'+_url+'"></div>'+
                    '<a class="delBtn" data-url="'+_url+'">删除</a>'+
                    '</li>';
            }
            $('#'+wrapper+' .upload-image').html(_html);
        }
    }
    $('#'+wrapper+' input[type="file"]').fileupload({
        url: baseURL + url,
        autoUpload: true,
        // forceIframeTransport : true,
        dataType: 'text',
        // acceptFileTypes: 'jpg,gif,jpe?g,png,zip,rar,XLSX,DOC,JPG,AVI,RAR,WAV,LOG,TIF,WPS,WMA,XLS,MP4,TXT,FLAC,SWF,DOCX,MOV,CSV,OGG,AAC,AMR,EML,M4A,HTML,GIF,MP3,PIC,PDF',
        acceptFileTypes: /(\.|\/)(gif|jpe?g|png|bmp)$/i,
        maxFileSize: 20480000,
        add: function (e, data) {
            var file = data.files[0];
            if (file.size > 10485760) {
                layer.msg('上传的文件太大了，请上传10m以下的文件');
                return
            }
            data.formData = {token:token};
            if (window.redirect) {
                data.redirect = redirect;
            }
            // alert('添加文件成功');
            data.submit();
        },
        progressall: function (e, data) {
            $('#'+wrapper+' .file_progress').show();
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $('#'+wrapper+' .file_progress .progress').css('width',progress + '%');//控制进度条
            $('#'+wrapper+' .file_progress .profont').html('' + progress + "%");
            if(progress >= 100) {
                $('#'+wrapper+' .file_progress').hide();
            }
        },
        done: function (e, data) {
            var _data = data && data.result;
            _data = JSON.parse(_data);
            for(var i in _data) {
                var file = _data[i];
                _html='<li>'+
                    '<div class="data-image"><img src="'+file.url+'"></div>'+
                    '<a class="delBtn" data-url="'+file.url+'">删除</a>'+
                    '</li>';
                urls.push(file.url);
                $('#'+wrapper).find('.upload-image').append(_html);
                for(var i in urls) {
                    if(urls[i] == $(this).data('url')) {
                        urls.splice(i, 1)
                    }
                }
                $('#'+wrapper).find('.upload-list').val(urls.toString());
            }

            // alert('渲染成功');
        },
        fail: function (e, data) {
            // alert('上传失败');
            layer.msg('上传失败');
        }
    })
    $('body').on('click', '.delBtn', function(e) {
        var _this = $(this);
        e.preventDefault();
        _this.parent().remove();
        for(var i in urls) {
            if(urls[i] == $(this).data('url')) {
                console.log(urls);
                urls.splice(i, 1)
            }
        }
        $('#'+wrapper).find('.upload-list').val(urls.toString())
    })
}
//判断是否填参数,没填返回空字符串
function verdict(ele){return ele.val() == undefined ? undefined : ele.val() == ele.attr('placeholder') ? '' : ele.val()}

// $("body").on("click",".del-img-btn", function(value) {
//     var _this = $(this);
//     _this.parent("li").remove();
// });


