(function ($) {

    $.suggestCustomer = function (input, options) {

        var $input = $(input).attr("autocomplete", "off");
        var $results;

        var timeout = false; 	// hold timeout ID for suggestion results to appear	
        var prevLength = 0; 		// last recorded length of $input.val()
        var cache = []; 			// cache MRU list
        var cacheSize = 0; 		// size of cache in chars (bytes?)

        if ($.trim($input.val()) == '' || $.trim($input.val()) == '') $input.val('').css('color', '#aaa');
        if (!options.attachObject)
            options.attachObject = $(document.createElement("ul")).appendTo('body');

        $results = $(options.attachObject);
        $results.addClass(options.resultsClass);

        resetPosition();
        $(window)
            .load(resetPosition)		// just in case user is changing size of page while loading
            .resize(resetPosition);

        $input.blur(function () {
            setTimeout(function () { $results.hide() }, 200);
        });

        $input.change(function () {
            var $containder = $(options.dataContainer);
            if ($(this).val().length == 0 && $containder.length != 0) {
                $containder.val('');
            } else {
                var has = false;
                for (var m = 0; m < options.source.length; m++) {
                    if (options.source[m]["Name"] == $(this).val()) {
                        has = true;
                        break;
                    }
                }
                if (!has && !options.anyInput) {
                    $(this).val('');
                    $containder.val('');
                }
            }

            //// 回调函数
            //if (options.onSelect) {
            //    $currentResult = getCurrentResult();
            //    options.onSelect($(this));
            //}
        });

        $input.focus(function () {
            //            if ($.trim($(this).val()) == '选择您的目的地') {
            //                $(this).val('').css('color', '#000');
            //            }
            if ($.trim($(this).val()) == '') {
                displayItems(''); //显示热门城市列表
            }
        });

        $input.click(function () {
            var q = $.trim($(this).val());
            displayItems(q);
            $(this).select();
        });

        // help IE users if possible
        try {
            $results.bgiframe();
        } catch (e) { }

        $input.keyup(processKey); //

        function resetPosition() {
            // requires jquery.dimension plugin
            var offset = $input.position();
            $results.css({
                top: (offset.top + input.offsetHeight) + 'px',
                left: offset.left + 'px'
            });
        }

        function processKey(e) {

            // handling up/down/escape requires results to be visible
            // handling enter/tab requires that AND a result to be selected
            if ((/27$|38$|40$/.test(e.keyCode) && $results.is(':visible')) ||
                (/^13$|^9$/.test(e.keyCode) && getCurrentResult())) {

                if (e.preventDefault)
                    e.preventDefault();
                if (e.stopPropagation)
                    e.stopPropagation();

                e.cancelBubble = true;
                e.returnValue = false;

                switch (e.keyCode) {

                    case 38: // up
                        prevResult();
                        break;

                    case 40: // down
                        nextResult();
                        break;
                    case 13: // return
                        selectCurrentResult();
                        break;
                    case 27: //	escape
                        $results.hide();
                        break;
                }

            } else if ($input.val().length != prevLength) {

                if (timeout)
                    clearTimeout(timeout);
                timeout = setTimeout(suggest, options.delay);
                prevLength = $input.val().length;
            }
        }

        function suggest() {

            var q = $.trim($input.val());
            displayItems(q);
        }
        function displayItems(items) {
            var par = { 'fromCity': '', 'keyword': items, 'hasChild': options.hasChild };
            $.post(options.requestUrl, par, function (data) {
                options.source = data;
                dd = /^[u4e00-u9fa5]+$/;
                var html = '';
                if (items == '' || !dd.test(items)) {
                    var s = 0;
                    for (var m = 0; m < options.source.length; m++) {
                        if (s < 8) {
                            //html += '<li rel="' + options.source[m]["Code"] + '"><a href="#' + m + '"><span>' + options.source[m]["FastCode"] + '</span>' + options.source[m]["Name"] + '</a></li>';
                            html += '<li rel="' + options.source[m]["Code"] + '"><a href="#' + m + '">' + options.source[m]["Name"] + '</a></li>';
                            s += 1;
                        }
                    }
                    html = '<div class="gray mudidi_tip">请输入拼音</div><ul>' + html + '</ul>';
                }
                else {
                    for (var i = 0; i < options.source.length; i++) {//国内城市匹配
                        var reg = new RegExp('^' + items + '.*$', 'im');
                        //html += '<li rel="' + options.source[i]["Code"] + '"><a href="#' + i + '"><span>' + options.source[i]["FastCode"] + '</span>' + options.source[i]["Name"] + '</a></li>';
                        html += '<li rel="' + options.source[i]["Code"] + '"><a href="#' + i + '">' + options.source[i]["Name"] + '</a></li>';
                    }
                    if (html == '') {
                        suggest_tip = '<div class="gray mudidi_tip">对不起，找不到：' + items + '</div>';
                    }
                    else {
                        suggest_tip = '<div class="gray mudidi_tip">' + items + '，按拼音排序</div>';
                    }
                    html = suggest_tip + '<ul>' + html + '</ul>';
                }

                $results.html(html).show();
                $results.children('ul').children('li:first-child').addClass(options.selectClass);

                $results.children('ul')
                    .children('li')
                    .mouseover(function () {
                        $results.children('ul').children('li').removeClass(options.selectClass);
                        $(this).addClass(options.selectClass);
                    })
                    .click(function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        selectCurrentResult();
                    });
            });

        }

        function getCurrentResult() {

            if (!$results.is(':visible'))
                return false;

            var $currentResult = $results.children('ul').children('li.' + options.selectClass);
            if (!$currentResult.length)
                $currentResult = false;

            return $currentResult;

        }
        //获取选择的值然后赋给表单
        function selectCurrentResult() {
            $currentResult = getCurrentResult();
            if ($currentResult) {
                $input.val($currentResult.children('a').html().replace(/<span>.*<\/span>/i, ''));

                $results.hide();
                if ($(options.dataContainer)) {
                    $(options.dataContainer).val($currentResult.attr('rel'));
                }

                if (options.onSelect) {
                    options.onSelect.apply($input[0]);
                }
            }
            //$("#theForm").submit();
        }

        function nextResult() {

            $currentResult = getCurrentResult();

            if ($currentResult)
                $currentResult
                    .removeClass(options.selectClass)
                    .next()
                    .addClass(options.selectClass);
            else
                $results.children('ul').children('li:first-child').addClass(options.selectClass);

        }

        function prevResult() {

            $currentResult = getCurrentResult();

            if ($currentResult)
                $currentResult
                    .removeClass(options.selectClass)
                    .prev()
                    .addClass(options.selectClass);
            else
                $results.children('ul').children('li:last-child').addClass(options.selectClass);

        }

    }

    $.fn.suggestCustomer = function (options) {
        options = options || {};
        options.delay = options.delay || 0;
        options.resultsClass = options.resultsClass || 'ac_results';
        options.selectClass = options.selectClass || 'ac_over';
        options.matchClass = options.matchClass || 'ac_match';
        options.minchars = options.minchars || 1;
        options.delimiter = options.delimiter || '\n';
        options.onSelect = options.onSelect || false;
        options.dataDelimiter = options.dataDelimiter || '\t';
        options.dataContainer = options.dataContainer || '#SuggestResult';
        options.attachObject = options.attachObject || null;
        options.requestUrl = options.requestUrl || null;
        options.hasChild = options.hasChild || false;
        options.anyInput = options.anyInput || false;

        this.each(function () {
            new $.suggestCustomer(this, options);
        });

        return this;
    };

})(jQuery);