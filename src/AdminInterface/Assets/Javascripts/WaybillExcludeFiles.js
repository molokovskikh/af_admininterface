
$(function () {
	$.validator.addClassRules("excludeFileMask", {
		required: true,
		minlength: 3
	});

	$('#DeleteUpdateForm').validate();
	$('#AddNewForm').validate();
});

function DeleteFileMask(itemId) {
	YesNoDialog('Удалить маску', 'Вы уверены, что хотите удалить эту маску ?', function() {
		$.post("DeleteMask", { maskId: itemId }, function(data) {
			window.location = "WaybillExcludeFiles?supplierId=" + data;
		});
	});
}