$(function () {
	$('.regionDiv').children('input').click(function () {
		$('.maxRegionDiv').children('input').prop("checked", false);
	});
	$('.maxRegionDiv').children('input').click(function () {
		if (this.checked)
			$('.regionDiv').children('input').prop("checked", false);
	});
});