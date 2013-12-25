$(function () {
	window.trackDependencies();

	registerCheckboxAll();
	registerEditable();
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

	setupCalendar();
});

function joinRowHighlighter() {
	$('.HighLightCurrentRow tr').not('.NoHighLightRow').each(function () {
		$(this).mouseout(function () { $(this).removeClass('SelectedRow'); });
		$(this).mouseover(function () { $(this).addClass('SelectedRow'); });
	});
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