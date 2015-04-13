tinyMCE.init({
	theme: "advanced",
	mode: "textareas",
	height: "500",
	width: "600",
	editor_selector: "tinymceAddressesHelpText"
});
tinyMCE.init({
	theme: "advanced",
	mode: "textareas",
	width: "600",
	editor_selector: "tinymce"
});

function addAttachRow(rowId) 
{
	$("#attachTable").append("<tr><td><input type='file' size='60'/></td></tr>");
}

$(function () {
	$('#generalSettings').click(function () {
		$('#activeTabName').val('#tab-generalSettings');
	});
	$('#miniMailSettings').click(function () {
		$('#activeTabName').val('#tab-miniMailSettings');
	});
	$('#supportTexts').click(function () {
		$('#activeTabName').val('#tab-supportTexts');
	});
	$('#techOperatingSettings').click(function () {
		$('#activeTabName').val('#tab-techOperatingSettings');
	});
	$('#processingNotices').click(function () {
		$('#activeTabName').val('#tab-processingNotices');
	});
	$('#deletingMinimails').click(function () {
		$('#activeTabName').val('#tab-deletingMinimails');
	});
});

