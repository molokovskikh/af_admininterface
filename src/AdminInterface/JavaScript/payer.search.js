function OnPayerExistsChanged(selectedValue) {
	$("#SearchPayerDiv").hide();
	$("#SelectPayerDiv").hide();
	var disabled = null;
	var checked = 'checked';
	if (selectedValue) {
		ShowSearchPayerDiv();
		disabled = 'disabled';
		checked = null;
	}
	$("#FillBillingInfo").attr('disabled', disabled);
	$("#FillBillingInfo").attr('checked', checked);
	$("#PayerExistsValue").val(selectedValue);
	$("#MessageForPayer").html("");
}

function ShowSearchPayerDiv() {
	$('#PayerComboBox').find('option').remove().end();
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
	$.get("SearchPayers",
		{"searchPattern": searchText},
		function (htmlOptions) {
			$('#PayerComboBox').find('option').remove().end();
			if (htmlOptions.length == 0) {
				$("#SearchPayerTextPattern").removeAttr("disabled");
				$("#SearchPayerButton").removeAttr("disabled");
				$("#MessageForPayer").html("<i>Ничего не найдено</i>");
				return;
			}
			$("#PayerComboBox").append(htmlOptions);
			$("#SearchPayerDiv").hide();
			$("#SelectPayerDiv").show();
			$("#MessageForPayer").html("");
		});
	$("#MessageForPayer").html("<i>Выполняется поиск</i>");
	$("#SearchPayerTextPattern").attr("disabled", "disabled");
	$("#SearchPayerButton").attr("disabled", "disabled");
}
