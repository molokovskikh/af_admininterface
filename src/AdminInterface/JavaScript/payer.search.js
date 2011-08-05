function OnPayerExistsChanged(selectedValue) {
	$("#SearchPayerDiv").hide();
	$("#SelectPayerDiv").hide();
	var disabled = '';
	var checked = 'checked';
	if (selectedValue) {
		ShowSearchPayerDiv();
		disabled = 'disabled';
		checked = '';
	}
	jQuery("#FillBillingInfo").attr('disabled', disabled);
	jQuery("#FillBillingInfo").attr('checked', checked);
	jQuery("#PayerExistsValue").val(selectedValue);
	jQuery("#MessageForPayer").html("");
}

function ShowSearchPayerDiv() {
	jQuery('#PayerComboBox').find('option').remove().end();
	jQuery("#MessageForPayer").html("");
	jQuery("#SearchPayerTextPattern").removeAttr("disabled");
	jQuery("#SearchPayerButton").removeAttr("disabled");
	$('#SelectPayerDiv').hide();
	$("#SearchPayerDiv").show();
	jQuery("#SearchPayerTextPattern").focus();
}

function ShowSelectPayerDiv() {
	var searchText = jQuery("#SearchPayerTextPattern").val();
	if (searchText.length == 0)
		return;
	jQuery.get("SearchPayers",
			{ "searchPattern": searchText },
			function (htmlOptions) {
				jQuery('#PayerComboBox').find('option').remove().end();
				if (htmlOptions.length == 0) {
					jQuery("#SearchPayerTextPattern").removeAttr("disabled");
					jQuery("#SearchPayerButton").attr("disabled", "");
					jQuery("#MessageForPayer").html("<i>Ничего не найдено</i>");
					return;
				}
				jQuery("#PayerComboBox").append(htmlOptions);
				$("#SearchPayerDiv").hide();
				$("#SelectPayerDiv").show();
				jQuery("#MessageForPayer").html("");
			});
	jQuery("#MessageForPayer").html("<i>Выполняется поиск</i>");
	jQuery("#SearchPayerTextPattern").attr("disabled", "disabled");
	jQuery("#SearchPayerButton").attr("disabled", "disabled");
}
