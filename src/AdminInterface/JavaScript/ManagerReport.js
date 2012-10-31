$(function () {
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
			.append("<a href='#'>" + item.label + "</a>")
			.appendTo(ul);
	};
});

function GetUserInfo(userId, item) {
	var thisBox = $(item).parent().children(".toggled:first");
	$(thisBox).empty();
	if (thisBox.is(":hidden")) {
		$.ajax({
			url: "GetUserInfo?userId=" + userId,
			type: "GET",
			async: false,
			success: function(data) {
				$(thisBox).append(data);
			}
		});
		thisBox.slideDown("slow");
	} else {
		thisBox.slideUp("slow");
	}
}