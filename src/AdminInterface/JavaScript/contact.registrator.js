function addPhone(namePrefix, idPrefix) {
	if (!idPrefix)
		idPrefix = namePrefix;
	var index = getIndex(1, "#" + idPrefix + "addEmailLink");
	var id = "phone" + index + idPrefix;
	var name = namePrefix + "Contacts[" + index + "]";
	var html = "<tr id='" + id + "'><td><table width='100%' cellpadding='0' cellspacing='0' border='0'><tr valign='top'>" +
			"<td width='30%'>Номер телефона:</td>" +
			"<td width='53%'>" +
			"<input type='text' name='" + name + ".ContactText' class='phone' style='width: 100%' />" +
			"<input type='hidden' name='" + name + ".Type' value='1' /></td>" +
			"<td align='right'><input type='button' onclick=removeElement('" + id + "') value='Удалить' /></td>" +
			"</tr><tr valign='top'><td>Примечание:</td><td><input type='text' name='" + name + ".Comment' style='width: 100%' /></td>" +
			"</tr></table></td></tr>";
	jQuery("#" + idPrefix + "Phones").append(html);
}

function addEmail(namePrefix, idPrefix) {
	if (!idPrefix)
		idPrefix = namePrefix;
	var index = getIndex(1, "#" + idPrefix + "addEmailLink");
	var id = "email" + index + idPrefix;
	var name = namePrefix + "Contacts[" + index + "]";
	var html = "<tr id='"+ id +"'><td><table width='100%' cellpadding='0' cellspacing='0' border='0'><tr valign='top'>" +
			"<td width='30%'>Email:</td>" +
			"<td width='53%'>" +
			"<input type='text' name='" + name + ".ContactText' class='email' style='width: 100%' />" +
			"<input type='hidden' name='" + name + ".Type' value='0' /></td>" +
			"<td align='right'><input type='button' onclick=removeElement('" + id + "') value='Удалить' /></td>" +
			"</tr><tr valign='top'><td>Примечание:</td><td><input type='text' name='" + name + ".Comment' style='width: 100%' /></td>" +
			"</tr></table></td></tr>";
	jQuery("#" + idPrefix + "Emails").append(html);
}

function addPerson(namePrefix, idPrefix) {
	if (!idPrefix)
		idPrefix = namePrefix;
	var index = getIndex(0, "#" + idPrefix + "addPersonLink");
	var id = "person" + index + idPrefix;
	var name = namePrefix + "Persons[" + index + "]";
	var html = "<tr id='" + id + "'><td><table width='100%' cellpadding='0' cellspacing='0' border='0'><tr valign='top'>" +
			"<td width='30%'>Ф.И.О.:</td>" +
			"<td width='53%'>" +
			"<input type='text' name='" + name + ".Name' style='width: 100%' /></td>" +
			"<td align='right'><input type='button' onclick=removeElement('" + id + "') value='Удалить' /></td>" +
			"</tr></table></td></tr>";
	jQuery("#" + idPrefix + "Persons").append(html);
}

function getIndex(begin, selector) {
	var index = parseInt(jQuery(selector).data('index'));
	if (!index)
		index = begin;
	jQuery(selector).data('index', ++index);
	return index;
}

function removeElement(id) {
	jQuery("#" + id).remove();
}