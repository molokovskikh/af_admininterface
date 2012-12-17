function ResetAFVersion(userId, userName) {
	var dialogText = "Вы уверены, что хотите произвести  сброс параметра, отвечающего за версию АФ, ниже кот. пользователю не разрешаем обновляться до 999? (Пользователь: " + userName + ")";
	YesNoDialog("Сбросить версию АФ", dialogText, function () {
		$.post("ResetAFVersion", { userId: userId }, function (data) {
			window.location = "Settings";
		});
	});
}