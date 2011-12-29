function ShowDownloadResult(element, id, url, callerDescription) {
	var row = $(element).parents("tr").get(0);
	var isCallerVisible = IsVisible(id, callerDescription);
	var detailsRow = $(row).next(".details");
	hideDetails(detailsRow, function () {
		if (!isCallerVisible)
			Download(url, row);
	});
}

function hide(element, afterFinish) {
	if (element.length > 0) {
		element.find("div").slideUp(500, function () {
			element.remove();
			afterFinish();
		});
	}
	else {
		afterFinish();
	}
}

function hideDetails(element, afterFinish) {
	if (element.length > 0) {
		hide(element, afterFinish);
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