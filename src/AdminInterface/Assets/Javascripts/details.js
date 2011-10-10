function ShowDownloadResult(element, id, url, callerDescription) {
	var row = $(element).parents("tr").get(0)
	var isCallerVisible = IsVisible(id, callerDescription);
	HideAll(id, function () {
		if (!isCallerVisible)
			Download(url, row);
	});
}

function Hide(id, description, afterFinish) {
	if ($("#" + description + id).length > 0) {
		$("#" + description + id).slideUp(500, function () {
			$("#" + description + "Row" + id).remove();
			afterFinish();
		});
	}
	else {
		afterFinish();
	}
}

function HideAll(id, afterFinish) {
	if (IsVisible(id, "DownloadLog")) {
		Hide(id, "DownloadLog", afterFinish);
	}
	else if (IsVisible(id, "UpdateDetails")) {
		Hide(id, "UpdateDetails", afterFinish);
	}
	else if (IsVisible(id, "DocumentDetails")) {
		Hide(id, "DocumentDetails", afterFinish);
	}
	else {
		afterFinish();
	}
}

function IsVisible(id, description) {
	return $("#" + description + id).length > 0;
}

function Download(url, row) {
	$.ajax({
		url: url,
		success: function (data) {
			var result = $(data);
			result.insertAfter(row);
			//не работает
			//row.slideDown(500);
			result.css("display", "table-row");
		}
	});
}