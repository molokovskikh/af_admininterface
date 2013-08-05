$(function () {
	$("form").validate();

	$("#payment_Payer_Name").autocomplete({
		source: "SearchPayer",
		minLength: 2,
		select: function (event, ui) {
			$("#payment_Payer_Id").val(ui.item.id);
		}
	});

	$.ui.autocomplete.prototype._renderItem = function (ul, item) {
		var uri = (window.siteroot || "") + "/Billing/Edit?billingcode=" + item.id;
		var cls = "";
		if ($("#payment_Recipient_Id").val() == item.recipient) {
			cls = "Recipient";
		}
		return $("<li></li>")
			.data("item.autocomplete", item)
			.append("<a href='" + uri + "' class='" + cls + "'>" + item.label + "</a>")
			.appendTo(ul);
	};
});
