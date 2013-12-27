function OnPayerExistsChanged(selectedValue) {
	$('#PayerComboBox').find('option').remove().end();
	//что бы обновить view model
	$("#PayerComboBox").change();

	$("#SearchPayerDiv").hide();
	$("#SelectPayerDiv").hide();
	var disabled = null;
	var checked = true;
	if (selectedValue) {
		ShowSearchPayerDiv();
		disabled = 'disabled';
		checked = false;
	}
	$("#options_FillBillingInfo").attr('disabled', disabled);
	$("#FillBillingInfo").attr('disabled', disabled);
	$("#options_FillBillingInfo").prop('checked', checked);
	$("#FillBillingInfo").prop('checked', checked);
	$("#PayerExistsValue").val(selectedValue);
	$("#MessageForPayer").html("");
}

function ShowSearchPayerDiv() {
	$("#MessageForPayer").html("");
	$("#SearchPayerTextPattern").removeAttr("disabled");
	$("#SearchPayerButton").removeAttr("disabled");
	$('#SelectPayerDiv').hide();
	$("#SearchPayerDiv").show();
	$("#SearchPayerTextPattern").focus();
}

function ShowSelectPayerDiv() {
	var searchText = $("#SearchPayerTextPattern").val();
	if (searchText.length == 0)
		return;
	var data = { "searchPattern": searchText };
	$.get("SearchPayers",
		data,
		function (htmlOptions) {
			$('#PayerComboBox').find('option').remove().end();
			//что бы обновить view model
			$("#PayerComboBox").change();
			if (htmlOptions.length == 0) {
				$("#SearchPayerTextPattern").removeAttr("disabled");
				$("#SearchPayerButton").removeAttr("disabled");
				$("#MessageForPayer").html("<i>Ничего не найдено</i>");
				return;
			}
			$("#PayerComboBox").append(htmlOptions);
			//что бы обновить view model
			$("#PayerComboBox").change();
			$("#SearchPayerDiv").hide();
			$("#SelectPayerDiv").show();
			$("#MessageForPayer").html("");
		});
	$("#MessageForPayer").html("<i>Выполняется поиск</i>");
	$("#SearchPayerTextPattern").attr("disabled", "disabled");
	$("#SearchPayerButton").attr("disabled", "disabled");
}
