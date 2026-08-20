$(function () {
    $("#preloader").fadeOut();
    $(".ui.sidebar-left").sidebar("attach events", ".navbar-toggle-left").sidebar("setting", "transition", "scale down");
    $(".ui.search").sidebar("attach events", ".navbar-toggle-search").sidebar("setting", "transition", "scale down");
    //$(".ui.cart").sidebar("attach events", ".navbar-toggle-cart").sidebar("setting", "transition", "scale down");
    $(".slider-show").owlCarousel({ items: 1, navigation: true, slideSpeed: 1000, dots: true, paginationSpeed: 400, singleItem: true, autoplay: true, loop: true });
    $(".slide-product").owlCarousel({ items: 1, navigation: true, slideSpeed: 1000, nav: true, paginationSpeed: 400, singleItem: true });
    $(".testimonial").owlCarousel({ items: 1, navigation: true, slideSpeed: 1000, dots: true, paginationSpeed: 400, singleItem: true, autoplay: true, loop: true });
    $(".menu .item").tab();
    $(".ui.accordion").accordion()
});