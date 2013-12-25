var name = /([^/]+)\.html/.exec(window.document.URL)[1]
configPaths = {
	"jquery": "packages/jQuery.1.6.4/Content/Scripts/jquery-1.6.4",
	"jquery-ui": "packages/jQuery.UI.Combined.1.10.2/Content/Scripts/jquery-ui-1.10.2",
	"jquery-validate": "packages/jQuery.Validation.1.8.1/Content/Scripts/jquery.validate",
	"knockout": "packages/knockoutjs.2.1.0/Content/Scripts/knockout-2.1.0.debug",
	"underscore": "packages/underscore.js.1.1.7/Content/Scripts/underscore",
	"confirmation": "src/AdminInterface/Assets/Javascripts/confirmation",
	"search-editor": "src/Common.Web.UI/Common.Web.UI/Assets/Content/Javascripts/search.editor.v2",
	"smart-order-settings": "src/AdminInterface/Assets/Javascripts/smart.order.settings",
	"edit-payment": "src/AdminInterface/Assets/Javascripts/EditPayment",
	"filter": "src/AdminInterface/Assets/Javascripts/filter",
	"payer-search": "src/AdminInterface/Javascript/payer.search",
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
	},
	paths: configPaths
});

require([name + "-test"], function () {
	QUnit.start();
});
