window.skipFirstDefaultRoute = true;

function AjaxRequest(link, action) {
	AjaxRequest(link, action, null);
}

function AjaxRequest(link, action, errorAction) {
	hideMessage();
	$.ajax({
		url: link,
		cache: false,
		success: function (html) {
			action(html);
		},
		error: function (xhr, textStatus, error) {
			showErrorMessage("При выполнении операции возникла ошибка. Попробуйте повторить позднее.");
			if (errorAction)
				errorAction(xhr, textStatus, error);
		}
	});
}

function showErrorMessage(text) {
	showMessage(text, "err");
}

function showSuccessMessage(message) {
	showMessage(message, "notice");
}

function showNotificationMessage(message) {
	showMessage(message, "notice");
}

function showMessage(text, type) {
	$("#ErrorMessageDiv").show();
	$("#ErrorMessageDiv").removeClass("notice").removeClass("err").addClass(type);
	$("#ErrorMessageDiv").html(text);
}

function hideMessage() {
	$("#ErrorMessageDiv").hide();
}

function RemoveMessage(userId) {
	showNotificationMessage("Удаление...");
	AjaxRequest("CancelMessage.rails?userId=" + userId,
			function () {
				$("#CurrentMessageForUser" + userId).remove();
				showSuccessMessage("Сообщение удалено");
			});
}

function RemoveMailEntity(id) {
	showNotificationMessage("Удаление...");
	AjaxRequest("DeleteMail.rails?id=" + id,
			function () {
				$('#mail' + id).remove();
				showSuccessMessage("Сообщение удалено");
			});
}

function ShowMessage(userId) {
	showNotificationMessage("Загрузка...", "Notification");
	AjaxRequest("ShowMessageForUser.rails?userId=" + userId,
			function (html) {
				hideMessage();
				$("#CurrentMessageForUser" + userId).children().replaceWith(html);
			}
		);
}
