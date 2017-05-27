///<reference path="../JsLibs/index.d.ts"/>
//
//   Gunucco
//     webclient.top.ts(.js)
//
//     this source is licenced AGPL 3.0
//
$(function () {
    var $top_container = $('#top-container');
    var $top_logo = $('#gunucco-top-large-logo');
    var $header = $('#navigation-bar');
    var $footer = $('#footer');
    // move gunucco large logo to page center
    var top_container_height = $top_container.outerHeight();
    var page_height = $(window).height();
    var header_height = $header.height();
    var footer_height = $footer.height();
    var top_container_margin_size = (page_height - header_height - footer_height - top_container_height) / 2;
    $top_container.css({
        'margin-top': top_container_margin_size + 'px'
    });
});
//# sourceMappingURL=webclient.top.js.map