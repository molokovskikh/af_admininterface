﻿$(function () {
	$("#filter_ClientName").autocomplete({
		source: "GetClientForAutoComplite",
		minLength: 2,
		select: function (event, ui) {
			$('#filter_ClientId').val(ui.item.id);
		}
	});

	$.ui.autocomplete.prototype._renderItem = function (ul, item) {
		return $("<li></li>")
			.data("item.autocomplete", item)
			.append("<a href='#'>" + '[' + item.id + '] ' + item.label + ' - ' + item.region + "</a>")
			.appendTo(ul);
	};

	HiddenIfAddressPage($('#filter_FinderType'));

	$('#filter_FinderType').change(function() {
		HiddenIfAddressPage(this);
	});
});

function HiddenIfAddressPage(item) {
	if ($(item).val() == 0) {
		$('#hideBlock').css("display", "table-row");
	} else {
		$('#hideBlock').css("display", "none");
	}
}

function GetUserInfo(userId, item) {
	var thisBox = $('#toggledRow' + userId);
	if (thisBox.length == 0) {
		$(thisBox).empty();
		$.ajax({
			url: "GetUserInfo?userId=" + userId,
			type: "GET",
			async: false,
			success: function(data) {
				$(data).insertAfter($(item).parent().parent());
			}
		});
	}

	thisBox = $('#toggledRow' + userId);
	if (thisBox.is(":hidden")) {
		thisBox.slideDown("slow");
	} else {
		thisBox.slideUp("slow");
	}
}

function GetAnalysInfo(objectId, item, type) {
	var thisBox = $('#toggledRow' + objectId);
	if (thisBox.length == 0) {
		var params = $('#FilterForm').serializeArray();
		params.push({ name: 'filter.ObjectId', value: objectId });
		params.push({ name: 'filter.Type', value: type });
		params.push({ name: 'filter.ForSubQuery', value: true });
		$.ajax({
			url: "GetAnalysUserInfo?clientId=" + objectId,
			data: params,
			type: "POST",
			dataType: "html",
			async: false,
			success: function(data) {
				$(JSON.parse(data)).insertAfter($(item).parent().parent());
			}
		});
	}
	thisBox = $('#toggledRow' + objectId);
	if (thisBox.is(":hidden")) {
		thisBox.slideDown("slow");
	} else {
		thisBox.slideUp("slow");
	}
}