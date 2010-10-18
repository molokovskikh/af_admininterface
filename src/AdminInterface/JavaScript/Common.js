$(function() {
	$('.HighLightCurrentRow tr').not('.NoHighLightRow').each(function () {
		$(this).mouseout(function () { $(this).removeClass('SelectedRow'); });
		$(this).mouseover(function () { $(this).addClass('SelectedRow'); });
	});

	$(".ShowHiden").live('click', function() {
		ShowHidden(this);
	});

	$(".HideVisible").live('click', function () {
		HideVisible(this);
	});

	SetupCalendarElements();

	$(".tabs ul li a").click(function () {
		$(".tab").hide();
		$(".tabs ul li a.selected").removeClass("selected");
		$("#" + this.id + "-tab").show();
		$(this).addClass("selected");
	});
});

function ShowHidden(folder) {
	$(folder).removeClass("ShowHiden");
	$(folder).addClass("HideVisible");
	$(folder).siblings("div").removeClass("hidden")
}

function HideVisible(folder) {
	$(folder).removeClass("HideVisible");
	$(folder).addClass("ShowHiden");
	$(folder).siblings("div").addClass("hidden")
}

function SetupCalendarElements() {
	$(".CalendarInput")
	.each(function (index, value) {
		value.id = "CalendarInput" + index;
		$(value).prev().id = "CalendarInputField" + index;
		Calendar.setup({
			ifFormat: "%d.%m.%Y",
			inputField: $(value).prev().get(0),
			button: value.id,
			weekNumbers: false,
			showOthers: true
		});
	});
}

function ShowElement(show, selector) {
	var displayValue = "none";
	if (show)
		displayValue = "block";
	$(selector).css("display", displayValue);
}