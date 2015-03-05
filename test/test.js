var name = /([^/]+)\.html/.exec(window.document.URL)[1]
configPaths = {
	"jquery": "packages/jQuery.1.9.1/Content/Scripts/jquery-1.9.1",
	"jquery-ui": "packages/jQuery.UI.Combined.1.10.2/Content/Scripts/jquery-ui-1.10.2",
	"jquery-validate": "packages/jQuery.Validation.1.11.1/Content/Scripts/jquery.validate",
	"knockout": "packages/knockoutjs.3.2.0/Content/Scripts/knockout-3.2.0.debug",
	"underscore": "packages/underscore.js.1.1.7/Content/Scripts/underscore",
	"confirmation": "src/AdminInterface/Assets/Javascripts/confirmation",
	"search-editor": "src/Common.Web.UI/Common.Web.UI/Assets/Content/Javascripts/search.editor.v2",
	"smart-order-settings": "src/AdminInterface/Assets/Javascripts/smart.order.settings",
	"edit-payment": "src/AdminInterface/Assets/Javascripts/EditPayment",
	"filter": "src/AdminInterface/Assets/Javascripts/filter",
	"payer-search": "src/AdminInterface/Javascript/payer.search",
	"region-editor": "src/Common.Web.UI/Common.Web.UI/Assets/Content/Javascripts/region-editor",
}
configPaths[name + "-test"] = "test/" + name;

require.config({
	baseUrl: "..",
	shim: {
		"jquery": {
			exports: "jQuery"
		},
		"knockout": {
			exports: "ko",
		},
		"jquery-validate": {
			deps: ["jquery"],
		},
		"jquery-ui": {
			deps: ["jquery"],
		},
		"confirmation": {
			deps: ["jquery", "jquery-validate", "jquery-ui"],
		},
		"search-editor": {
			deps: ["jquery", "jquery-validate", "knockout", "underscore"],
		},
		"smart-order-settings": {
			deps: ["jquery", "jquery-validate", "knockout", "underscore"],
		},
		"edit-payment": {
			deps: ["jquery", "jquery-validate", "jquery-ui"],
		},
		"filter": {
			deps: ["jquery"],
		},
		"payer-search": {
			deps: ["jquery"],
		},
		"region-editor": {
			deps: ["jquery"]
		},
	},
	paths: configPaths
});

require([name + "-test"], function () {
	QUnit.start();
});
