///<reference path="../JsLibs/index.d.ts"/>
//
//   Gunucco
//     webclient.top.ts(.js)
//
//     this source is licenced AGPL 3.0
//

var $top_container = $('#top-container');
var $top_logo = $('#gunucco-top-large-logo');
var $header = $('#navigation-bar');
var $footer = $('#footer');

$(function () {
});

$(window).on('load resize', function () {
    on_window_resized();
});

function on_window_resized() {
    // move gunucco large logo to page center
    var top_container_height = $top_container.outerHeight();
    var page_height = $(window).height();
    var header_height = $header.outerHeight();
    var footer_height = $footer.outerHeight() + 24;
    var top_container_margin_size = (page_height - header_height - footer_height - top_container_height) * 0.47;    // 0.5
    $top_container.css({
        'margin-top': top_container_margin_size + 'px'
    });

    $top_container.css('opacity', 1);
    $top_container.css('-moz-opacity', 1);
    $top_container.css('filter', 'alpha(opacity=100)');
}
