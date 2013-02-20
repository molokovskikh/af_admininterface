function AndJurdicalOrganization(element) {
	$('#andJurdicalOrganization').val(element.checked);
	if (element.checked) {
		$("#selectJurdicalOrganizationBlock").removeClass("display").addClass("noDisplay");
	} else {
		$("#selectJurdicalOrganizationBlock").removeClass("noDisplay").addClass("display");
	}
}