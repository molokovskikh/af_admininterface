$(function () {
	$("#filter_SupplierName").autocomplete({
		source: "GetSullierList",
		minLength: 2,
		select: function (event, ui) {
			$('#filter_SupplierId').val(ui.item.id);
		}
	});

	$.ui.autocomplete.prototype._renderItem = function (ul, item) {
		return $("<li></li>")
			.data("item.autocomplete", item)
			.append("<a href='Suppliers/" + item.id + "'>[" + item.id + "] " + item.label + "</a>")
			.appendTo(ul);
	};
});

function ShowMiNiMail(mailId) {
	$.ajax({
		url: "GetMail",
		type: "GET",
		cache: false,
		data: { mailId: mailId },
		success: function (data) {
			$(data).dialog({
				modal: true,
				async: true,
				position: ['center', 'center'],
				width : 600,
				heigth : 500
			});
		}
	});
}

function DeleteMiNiMail(mailId, supplier, date) {
	var dialogText = "Вы действительно хотите удалить письмо поставщика '" + supplier + "' от " + date;
	YesNoDialog("Удалить письмо", dialogText, function () {
		$.post("Delete", { id: mailId }, function () {
			location.reload();
		});
	});
}