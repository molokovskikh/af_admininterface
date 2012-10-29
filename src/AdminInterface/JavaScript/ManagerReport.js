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