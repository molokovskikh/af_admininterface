$(document).ready(function () {
	if ($("#filter_ClientType").val() != 1) {
		jQuery("#filter_WithoutSuppliers").parents('tr').hide();
	}

	$('#filter_ClientType').change(function () {
		if ($(this).val() == 1) {
			jQuery("#filter_WithoutSuppliers").parents('tr').show();
		}
		else {
			jQuery("#filter_WithoutSuppliers").parents('tr').hide();
		}
	});
});