var insertId = 0;
function setContactTypeClass(rowId, className) {
	var textBox = document.getElementById("contacts[" + rowId + "].ContactText");
	textBox.setAttribute("class", className);
}

function setContactType(rowId) {
	var comboBox = document.getElementById("contacts[" + rowId + "].Type");
	var index = comboBox.selectedIndex;
	if (index == 0) {
		setContactTypeClass(rowId, "email");
		var text = $("#contactActionLink" + rowId).attr("name");
		$("#contactActionLink" + rowId).attr("href", "mailto:" + text);
		$("#contactActionLink" + rowId).attr("class", "contact-email-icon");
	}
	else if (index == 1) {
		setContactTypeClass(rowId, "phone");
		var text = $("#contactActionLink" + rowId).attr("name");
		$("#contactActionLink" + rowId).attr("href", "sip:8" + text.replace('-', ''));
		$("#contactActionLink" + rowId).attr("class", "contact-phone-icon");
	}
}

function deleteContactRow(rowId) {
	$("#row" + rowId).remove();
	if (rowId > 0) {
		$("#ContactHolder").append("<input type='hidden' name='deletedContacts[" + rowId + "].Id' id='contacts[" + rowId + "].Id' value='" + rowId + "' />");
	}
}

function addContactRow(rowId, text, comment, contactType) {
	var imageLink = "<a id='contactActionLink" + rowId + "' href='' name='" + text + "' class='contact-email-icon'></a>";

	$("#ContactHolder").append("<tr id='row" + rowId + "' name='row" + rowId + "' valign='top'>" +
		"<td align='right'>" + imageLink + "</td>" +
		"<td class='ContactInfoCell' align='right'>" +
			"<input type='hidden' name='contacts[" + rowId + "].Id' id='contacts[" + rowId + "].Id' value='" + rowId + "'/>" +
			"<select style='width: 100%;' name='contacts[" + rowId + "].Type' id='contacts[" + rowId + "].Type' onchange='setContactType(" + rowId + ")'>" +
				"<option value='" + window.EmailContactType + "' selected>Email</option>" +
				"<option value='" + window.PhoneContactType + "'>Телефон</option>" +
			"</select>" +
			"<p>Примечание:</p>" +
		"</td>" +
		"<td style = 'width: 50%;padding-left: 10px;'>" +
			"<input type='text' style='width: 100%' class='email' name='contacts[" + rowId + "].ContactText' id='contacts[" + rowId + "].ContactText' value='" + text + "'/>" +
			"<input type='text' style='width: 100%' name='contacts[" + rowId + "].Comment' id='contacts[" + rowId + "].Comment' value='" + comment + "'/>" +
		"</td>" +
		"<td style='vertical-align: top;padding-left: 10px'>" +
			"<input type='button' value='Удалить' name='contacts[" + rowId + "].Delete' onclick='deleteContactRow(" + rowId + ")' />" +
		"</td>" +
	"</tr>");

	if (contactType == window.EmailContactType) {
		var comboBox = document.getElementById("contacts[" + rowId + "].Type");
		comboBox.selectedIndex = 0;
		setContactType(rowId);
	}
	else if (contactType == window.PhoneContactType) {
		var comboBox = document.getElementById("contacts[" + rowId + "].Type");
		comboBox.selectedIndex = 1;
		setContactType(rowId);
	}
}