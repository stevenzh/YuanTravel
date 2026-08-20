
//window.onscroll = scrollDiv;
//void function onResize() {
//    scrollDiv();
//    setTimeout(onResize, 100);

//}();

//function scrollDiv() {
//    var myDiv = document.getElementById("mm_right");
//    var mm_left = document.getElementById("mm_left");

//    var leftWidth = getPosition(mm_left).left;
//    var myDivTop = getPosition(myDiv).top;
//    var mm_left_top = getPosition(mm_left).top;

//    if (document.body == null) {
//        return;
//    }
//    var scrollTop = document.documentElement.scrollTop || document.body.scrollTop;

//    if (scrollTop > myDivTop) {
//        myDiv.className = "mm_right active";
//        myDiv.style.left = leftWidth + 500 + 'px';
//    }
//    if (scrollTop < mm_left_top) {
//        myDiv.className = "mm_right";
//    }
//}

//function getPosition(obj) {
//    if (obj == undefined) {
//        return { "top": 0, "left": 0, "width": 0, "height": 0 };
//    }
//    var top = 0;
//    var left = 0;
//    var width = obj.offsetWidth;
//    var height = obj.offsetHeight;
//    while (obj.offsetParent) {
//        top += obj.offsetTop;
//        left += obj.offsetLeft;
//        obj = obj.offsetParent;
//    }
//    return { "top": top, "left": left, "width": width, "height": height };
//}
