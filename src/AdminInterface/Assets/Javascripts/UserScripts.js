function ResetAFVersion(userId, userName) {
	var dialogText = "Вы уверены, что хотите произвести  сброс параметра, отвечающего за версию АФ, ниже кот. пользователю не разрешаем обновляться до 999? (Пользователь: " + userName + ")";
	YesNoDialog("Сбросить версию АФ", dialogText, function () {
		$.post("ResetAFVersion", { userId: userId }, function (data) {
			window.location = "Settings";
		});
	});
}

jQuery.fn.center = function() {
	this.css("position", "absolute");
	this.css("top", (($(window).height() - this.outerHeight()) / 2) + $(window).scrollTop() + "px");
	this.css("left", (($(window).width() - this.outerWidth()) / 2) + $(window).scrollLeft() + "px");
	return this;
};


function AjaxLoader() {
	$('body').append('<div id="loadingDiv"></div>');

	$('#loadingDiv')
		.append('<p id="loadingText"></p>')
		.center()
		.hide();
}


function DeleteItemInInboundList(hashCode, priceName) {
	var dialogText = "Вы уверены, что хотите удалить прайс лист: '" + priceName + "' из очереди на формализацию";
	YesNoDialog("удалить прайс лист", dialogText, function () {
		$('#loadingText').text("Удаление плайс листа '" + priceName + "'");
		$('#loadingDiv').show();
		$.post("DeleteItemInInboundList", { hashCode: hashCode }, function (data) {
			window.location = "InboundPriceItemsList";
		});
	});
}