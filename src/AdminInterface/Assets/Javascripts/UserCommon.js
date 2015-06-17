
function YesNoDialog(headerText, dialogText, successFunction) {
	var buttonsOpts = {};
	buttonsOpts["Да"] = function () {
		successFunction();
		$(this).dialog("close");
	};
	buttonsOpts["Нет"] = function () {
		$(this).dialog("close");
	};

	$('<div></div>').appendTo('body')
	.html('<div class="shureDeleteAdress">' + dialogText + '</div>')
	.dialog({
		modal: true, title: headerText, zIndex: 10000, autoOpen: true,
		width: '350', resizable: false,
		buttons: buttonsOpts,
		close: function (event, ui) {
			$(this).remove();
		}
	});
}

$(window).on("load", function () {
	var link = $("a.sendMailForNewSupplier");
	link.on("click", function () {
		var address = prompt("Введите адрес электронной почты клиента");
		if (address.trim() == "")
			return;

		var href = link.attr("href");
		href += "?name=" + address;
		window.location.href = href;
		return false;
	})
})
