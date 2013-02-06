$(function () {
	$('.regionDiv').children('input').click(function () {
		$('.maxRegionDiv').children('input').attr("checked", false);
	});
	$('.maxRegionDiv').children('input').click(function () {
		if (this.checked)
			$('.regionDiv').children('input').attr("checked", false);
	});
});