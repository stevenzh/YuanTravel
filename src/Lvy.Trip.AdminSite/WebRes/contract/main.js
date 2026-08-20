var $pageNum = 20;

//上传图片
var urls=[];
var url = "/upload";
var baseURL = "https://fileupload.12301.cn";
var token = "oh, it's ok";
var redirect = "http://gbsaas.12301.cn/html/cors/result.html";
function initUpload(wrapper) {
    var i=0;
    $('#'+wrapper+' input[type="file"]').on('click', function(e) {
        e.stopPropagation();
    })
    $('#'+wrapper+' input[type="file"]').hide();
    $('#'+wrapper).find('.file').on('click', function(e){
        e.preventDefault();
        $('#'+wrapper+' input[type="file"]').trigger('click');
        var html = '<li class="file_progress"><p class="profont">0%</p> <div class="plupload_progress_container"><p class="progress"></p></div></li>';
        $('#'+wrapper).find('#dataimage').append(html);
        $('.file_progress').hide();
    });

    // alert('初始化成功');
    
    $('#'+wrapper+' input[type="file"]').fileupload({
        url: baseURL + url,
        autoUpload: true,
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
        	$('.file_progress').show();
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $('.file_progress .progress').css('width',progress + '%');//控制进度条
            $('.file_progress .profont').html('' + progress + "%");
            console.log(progress);
            if(progress >= 100) {
                $('.file_progress').remove();
            }
        },
        done: function (e, data) {
            // alert('上传成功');
            var _data = data && data.result;
            // alert(_data);
            _data = JSON.parse(_data)
            // console.log(_data)
            for(var i in _data) {
                var file = _data[i];
                _html = '<li><img src="'+file.url+'"><p>'+file.name+'</p><div class="mask"></div><a class="delBtn" data-url="'+file.url+'">删除</a></li>';
                urls.push(file.url);
                $('#'+wrapper).find('#dataimage').append(_html);
                for(var i in urls) {
                    if(urls[i] == $(this).data('url')) {
                        urls.splice(i, 1)
                    }
                }
                $('#'+wrapper).find('input[name="urllist"]').val(urls.toString());
            }
            $('body').on('click', '.delBtn', function(e) {
                e.preventDefault();
                $(this).parent().remove();
                for(var i in urls) {
                    if(urls[i] == $(this).data('url')) {
                        urls.splice(i, 1)
                    }
                }
                $('#'+wrapper).find('input[name="urllist"]').val(urls.toString())
            })
            // alert('渲染成功');
        },
        fail: function (e, data) {
            // alert('上传失败');
            layer.msg('上传失败');
        }
    })
}
