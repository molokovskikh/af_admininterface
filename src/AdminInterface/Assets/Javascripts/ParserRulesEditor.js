function mappingFix() {}
mappingFix.runForLines = function() {
	$("table.editable").on('click', "tr input[type=button].delete, tr a.delete", function() {
		var indexOfItem = -1;
		var lastCaseCharSet = '';
		$('#mapping').find("[name*='Lines[']").each(function() {
			var currentNameArray = $(this).attr('name').split(".");
			indexOfItem = lastCaseCharSet === currentNameArray[0] ? indexOfItem : indexOfItem + 1;
			lastCaseCharSet = currentNameArray[0];
			currentNameArray[0] = "Lines[" + indexOfItem + "]";
			$(this).attr('name', currentNameArray.join("."));
		});
	});
}