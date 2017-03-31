function setupCalendar() {
	SetupCalendarElements();

	function beginDateAllowed(date) {
		if (endCalendar)
			return !(date <= endCalendar.date);
		else
			return false;
	}

	function endDateAllowed(date) {
		if (beginCalendar)
			return !(date >= beginCalendar.date);
		else
			return false;
	}

	beginCalendar = null;
	endCalendar = null;

	$(".calendar").each(function () {
		var id = this.id.substring(0, this.id.indexOf("CalendarHolder"));
		var value = $("#" + id);
		if (!value.length)
			value = $(this).siblings("input[type=hidden]");

		var calendar = Calendar.setup({
			daFormat: "%Y-%m-%d",
			ifFormat: "%Y-%m-%d",
			weekNumbers: false,
			flat: this.id,
			flatCallback: function () {
				value.get(0).value = calendar.date.print("%Y-%m-%d");
				calendar.refresh();
				if (beginCalendar && endCalendar) {
					beginCalendar.refresh();
					endCalendar.refresh();
				}
			},
			showOthers: true
		});
		calendar.parseDate(value.val());
		if (id.indexOf("begin") >= 0) {
			beginCalendar = calendar;
			calendar.setDateStatusHandler(beginDateAllowed);
		}
		if (id.indexOf("end") >= 0) {
			endCalendar = calendar;
			calendar.setDateStatusHandler(endDateAllowed);
		}
	});

	if (beginCalendar)
		beginCalendar.refresh();
	if (endCalendar)
		endCalendar.refresh();
}

function SetupCalendarElements() {
	$(".CalendarInput").each(function (index, value) {
		value.id = "CalendarInput" + index;
		$(value).prev().id = "CalendarInputField" + index;
		var input = $(value).prev().get(0);
		Calendar.setup({
			ifFormat: "%Y-%m-%d",
			inputField: input,
			button: value.id,
			weekNumbers: false,
			showOthers: true
		});
	});
}