///<reference path="../JsLibs/index.d.ts"/>
//
//   Gunucco
//     webclient.ts(.js)
//
//     this source is licenced AGPL 3.0
//
$("textarea.dynamic-height").on("input", function (evt) {
    if (evt.target.scrollHeight > (evt.target).offsetHeight) {
        $(evt.target).height(evt.target.scrollHeight);
    }
});
//# sourceMappingURL=webclient.js.map