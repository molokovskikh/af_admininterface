$(function () {
	window.trackDependencies();
	$("input[type=checkbox].all").click(function () {
		$(this)
			.parents("table")
			.find("input[type=checkbox]")
			.attr("checked", this.checked);
	});

	$("table.editable tr input[type=button].add, table.editable tr a.add").live('click', function () {
		var table = $(this).parents("table");
		var body = table.find("tbody").get(0);
		var row = table.data("template")(table);

		maxIndex = $(body).children("tr").length + 1;
		row.find("input").each(function () {
			this.name = this.name.replace(/\d+/, maxIndex);
		});
		row.find("textarea").each(function () {
			this.name = this.name.replace(/\d+/, maxIndex);
		});

		row.appendTo(body);
	});

	$("table.editable tr input[type=button].delete, table.editable tr a.delete").live('click', function () {
		$($(this).parents("tr").get(0)).remove();
	});

	joinRowHighlighter();

	$('.input-date').each(function () {
		$(this).mask("99.99.9999");
	});
	/*
	$('.input-sum').each(function () {
	$(this).mask("999999999,99");
	});
	*/
	$("form.accountant-friendly input[type=text],form.accountant-friendly select,form.accountant-friendly textarea").keydown(function (e) {
		if (event.keyCode == 13) {
			var items = $(this).parents("form").find("input[type=text],select,textarea");
			var currentIndex = items.index(this);
			var nextItem = items.get(currentIndex + 1);
			if (nextItem) {
				nextItem.focus();
				nextItem.select();
				return false;
			}
		}
	});

	$(".ShowHiden").live('click', function () {
		ShowHidden(this);
	});

	$(".HideVisible").live('click', function () {
		HideVisible(this);
	});

	SetupCalendarElements();

	var TabRouter = Backbone.Router.extend({
		routes: {
			"tab-:id": "tab",
			"*actions": "defaultRoute"
		},

		tab: function (id) {
			showTab(id);
		},

		defaultRoute: function () {
			var tabs = $(".tabs ul li a[href='#'], .tabs ul li a.inline-tab");
			if (window.skipFirstDefaultRoute) {
				window.skipFirstDefaultRoute = false;
				return;
			}
			if (tabs.length)
				showTab(tabs.get(0).id);
		}
	});

	new TabRouter();
	Backbone.history.start();

	//если он у ссылки есть href и он не inline-tab значит это просто ссылка в виде таба и не надо ее обрабатывать
	$(".tabs ul li a[href='#'], .tabs ul li a.inline-tab").click(function () {
		window.location.hash = "tab-" + this.id;
		return false;
	});

	//ie агресивно кеширует по этому cache: false
	//и запрос будет только один раз
	function loadTabContent(element) {
		$.ajax({
			url: element.attr("href"),
			cache: false,
			success: function (content) {
				$("#" + element.attr("id") + "-tab").html(content);
			},
			error: function (error) {
				alert("Во время обработки ващего запроса произошел сбой, попробуйте повторить ваш запрос.");
			}
		});
	}

	function showTab(id) {
		$(".tab").hide();
		$(".tabs ul li a.selected").removeClass("selected");
		$("#" + id + "-tab").show();
		var element = $("#" + id);
		element.addClass("selected");
		if (element.attr("href") != "#")
			loadTabContent(element);
	}

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
		var value = $("#" + id).get(0).value;

		var calendar = Calendar.setup({
			daFormat: "%d.%m.%Y",
			ifFormat: "%d.%m.%Y",
			weekNumbers: false,
			flat: this.id,
			flatCallback: function () {
				$("#" + id).get(0).value = calendar.date.print("%d.%m.%Y");
				calendar.refresh();
				if (beginCalendar && endCalendar) {
					beginCalendar.refresh();
					endCalendar.refresh();
				}
			},
			showOthers: true
		});
		calendar.parseDate(value);
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
});

function joinRowHighlighter() {
	$('.HighLightCurrentRow tr').not('.NoHighLightRow').each(function () {
		$(this).mouseout(function () { $(this).removeClass('SelectedRow'); });
		$(this).mouseover(function () { $(this).addClass('SelectedRow'); });
	});
}

function cloneRowTemplate(table) {
	var row = $(table.find("tr").get(1)).clone();
	row.find("input[type=hidden][name$=Id]").val("0");
	row.find("input[type=hidden][name$=id]").val("0");
	return row;
}

function ShowHidden(folder) {
	$(folder).removeClass("ShowHiden");
	$(folder).addClass("HideVisible");
	$(folder).siblings("div").removeClass("hidden");
}

function HideVisible(folder) {
	$(folder).removeClass("HideVisible");
	$(folder).addClass("ShowHiden");
	$(folder).siblings("div").addClass("hidden");
}

function SetupCalendarElements() {
	$(".CalendarInput").each(function (index, value) {
		value.id = "CalendarInput" + index;
		$(value).prev().id = "CalendarInputField" + index;
		input = $(value).prev().get(0);
		if (!$(input).attr("disabled")) {
			Calendar.setup({
				ifFormat: "%d.%m.%Y",
				inputField: input,
				button: value.id,
				weekNumbers: false,
				showOthers: true
			});
		}
	});
}

function ShowElement(show, selector) {
	var displayValue = "none";
	if (show)
		displayValue = "block";
	$(selector).css("display", displayValue);
}
