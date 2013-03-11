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

	$('.deleteGroupBox').change(
		function() {
			$('.deleteGroupBox').parent().parent().removeClass('markedForDeleteMail');
			$(".deleteGroupBox").each(function() {
				if (this.checked) {
					$(this).parent().parent().addClass('markedForDeleteMail');
				}
			});
		});

	$('#deleteSelectedButton').click(function() {
		var dialogText = "Вы действительно хотите удалить выбранные письма?'";
		YesNoDialog("Удалить письмо", dialogText, function () {
			var items = new Array();
			$(".deleteGroupBox").each(function () {
				if (this.checked) {
					items.push($(this).val());
				}
			});
			$.post("DeleteGroup", { 'ids[]' : items }, function () {
				location.reload();
			});
		});
	});
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