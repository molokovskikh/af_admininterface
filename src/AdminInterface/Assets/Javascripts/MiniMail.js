$(function () {
	$("#filter_SupplierName").autocomplete({
		source: "MailsModering/GetSullierList",
		minLength: 2,
		select: function (event, ui) {
			$('#filter_SupplierId').val(ui.item.id);
		}
	});

	$.ui.autocomplete.prototype._renderItem = function (ul, item) {
		return $("<li></li>")
			.data("item.autocomplete", item)
			.append("<a href='Suppliers/" + item.id + "'>" + item.label + "</a>")
			.appendTo(ul);
	};
});

function ShowMiNiMail(mailId) {
	$.ajax({
		url: "ShowMail",
		type: "GET",
		cache: false,
		data: { mailId: mailId },
		success: function (data) {
			$(data).dialog({
				modal: true,
				position: ['center', 'center'],
				width : 500,
				heigth : 500
			});
		}
	});
}