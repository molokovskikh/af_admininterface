$(function () {
	$('#WaybillSourceSettingsForm').validate();

	EditorChange();
	$('#source_SourceType').change(function () {
		EditorChange();
	});

	$('#AddEmailButton').click(function() {
		var html = "<div><input type='text' id='EmailBox_" + EmailCount + "' class='EmailFieldBox' name='Emails[" + EmailCount + "]' /><a href='javescript:void(0);' onclick='DeleteEmail(this);'>Удалить</a></div>";
		$("#emailBox").append(html);
		EmailValidation($("#EmailBox_" + EmailCount));
		EmailCount++;
	});

	$('.EmailFieldBox').each(function () {
		EmailValidation(this);
	});
});

function EditorChange() {
	var val = $('#source_SourceType').val();
	if (val == 5) {
		GetFTPEditor();
	} else {
		if (val == 4) {
			GetAnalitFTPEditor();
		} else {
			$("#selectorBlock").empty();
		}
	}
}

function DeleteEmail(item) {
	$(item).parent().remove();
}

function EmailValidation(item) {
	$(item).rules("add", {
		required: true,
		email: true,
		messages: {
			required: "Поле не может быть пустым",
			email: "Ошибка ввода емейла",
		}
	});
}

function SelectorBlockEmtyValidation() {
	$("#selectorBlock input[type=text]").each(function () {
		$(this).rules("add", {
			required: true,
			messages: {
				required: "Поле не может быть пустым",
			}
		});
	});
}

function GetClassFieldEditor() {
	var html = "<div class='block'><h3>Настройка данных для доступа к FTP</h3></div>";
	html += "<table><tbody><tr><td>";
	html += "<label for='source_WaybillUrl'>Имя класса</label></td>";
	if (ReaderClassName == "")
		ReaderClassName = "SupplierFtpReader";
	html += "<td><input type='text' name='source.ReaderClassName' id='source_ReaderClassName' value='" + ReaderClassName + "'/></td></tr>";
	return html;
}

function GetAnalitFTPEditor() {
	var html = GetClassFieldEditor();

	html += "</tbody></table>";

	$("#selectorBlock").empty();
	$("#selectorBlock").append(html);

	SelectorBlockEmtyValidation();
}

function GetFTPEditor() {
	var html = GetClassFieldEditor();

	html += "<tr><td><label for='source_WaybillUrl'>Источник накладных</label></td>";
	html += "<td><input type='text' name='source.WaybillUrl' id='source_WaybillUrl' value='" + WaybillUrl + "'/></td></tr>";

	html += "<tr><td><label for='source_RejectUrl'>Источник отказов</label></td>";
	html += "<td><input type='text' name='source.RejectUrl' id='source_RejectUrl' value='" + RejectUrl + "'/></td></tr>";

	html += "<tr><td><label for='userName'>Пользователь</label></td>";
	html += "<td><input type='text' name='source.UserName' id='userName' value='" + UserName + "'/></td></tr>";

	html += "<tr><td><label for='source_Password'>Пароль</label></td>";
	html += "<td><input type='password' name='source.Password' id='source_Password' value='" + Password + "'/></td></tr>";

	html += "<tr><td><label for='downloadInterval'>Интервал загрузки</label></td>";
	html += "<td><input type='text' name='source.DownloadInterval' id='source_downloadInterval' value='" + DownloadInterval + "'/></td></tr>";

	html += "<tr><td><label for='ActiveMode'>Active Mode</label></td>";
	if (ActiveMode) {
		html += "<td><input type='checkbox' name='source.FtpActiveMode' id='ActiveMode' checked/></td></tr>";
	} else {
		html += "<td><input type='checkbox' name='source.FtpActiveMode' id='ActiveMode'/></td></tr>";
	}
	html += "</tbody></table>";
	$("#selectorBlock").empty();
	$("#selectorBlock").append(html);

	SelectorBlockEmtyValidation();

	$('#source_Password').rules("add", {
		required: true,
		messages: {
			required: "Поле не может быть пустым",
		}
	});

	$('#source_downloadInterval').rules("add", {
		digits: true,
		messages: {
			digits: "Должно быть введено число",
		}
	});
}