
function getModel(event, text, funcPre, funcPost) {
	event.preventDefault();
	var buttons = {};
	var link = $(this);
	if (funcPre != undefined) {
		funcPre(this);
	}
	buttons["Да"] = function () {
		if (funcPre != undefined) {
			funcPost();
		}
	};
	buttons["Нет"] = function() {
		$(this).dialog("close");
	};
	$('<div></div>')
		.html($("<div>" + text + "</div>"))
		.appendTo('body')
		.dialog({
			modal: true,
			title: "Внимание",
			zIndex: 10000,
			autoOpen: true,
			width: '350',
			resizable: false,
			buttons: buttons,
			close: function(event, ui) {
				$(this).remove();
			}
		});
}