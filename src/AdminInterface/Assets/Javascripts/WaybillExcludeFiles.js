
$(function () {
	$.validator.addClassRules("excludeFileMask", {
		required: true,
		minlength: 3
	});

	$('#DeleteUpdateForm').validate();
	$('#AddNewForm').validate();
});

function DeleteFileMask(itemId, item) {
	var mask = $(item).parent().parent().children('td').children('.excludeFileMask:first').val();
	YesNoDialog('Удалить маску', 'Вы уверены, что хотите удалить маску "' + mask + '" ?', function () {
		$.post("DeleteMask", { maskId: itemId }, function(data) {
			window.location = "WaybillExcludeFiles?supplierId=" + data;
		});
	});
}