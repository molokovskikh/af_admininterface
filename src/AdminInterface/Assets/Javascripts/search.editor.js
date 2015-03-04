$(function () {
	$("input[type=button].cancel-search").on("click", function () {
		var editorData = $(this).data("search-editor");
		if (!editorData)
			return;
		var id = editorData.id;
		if (!id)
			return;
		cancel(id);
		if ($("#" + id).prop("checked")) {
			$("#" + id).prop("checked", false);
		}
	});

	$("input[type=button].search").on("click", function () {
		var editorData = $(this).data("search-editor");
		var url = $(this).data("url") || editorData.url;
		if (editorData)
			var item = editorData.item;

		var root = $(this).parents("div.search").first();
		var rootRow = root.parents("tr").first();
		var term = root.find("input[type=text].term").val();
		$.ajax({
			url: url,
			data: { "text": term },
			cache: false,
			success: function (data) {
				if (data.length == 0) {
					root.find("input[type=button].search").css("disabled", "");
					message(root, "Ничего не найдено.", "error");
					return;
				}
				root.children().not(".search-title").remove();
				rootRow.find(".settings").css("display", "");
				var select = $("<select>");
				$.each(data, function () {
					select.append($("<option>").attr("value", this.id).text(this.name));
				});
				var selector = "input[type=hidden]";

				select.change(function () {
					if (item)
						$(item).val($(this).val());
					else
						rootRow.find(selector).val($(this).val());
				});
				select.change();
				root.append(select);
			},
			error: function (xhr, textStatus, error) {
				message(root, "Произошла ошибка. Попробуйте еще раз.", "error");
			}
		});
		root.find("input[type=button].search").css("disabled", "disabled");
		message(root, "Идет поиск...");
	});
});

function message(root, text, clazz) {
	root.find(".message").remove();
	root.append($("<span class=message>" + text + "<span>").addClass(clazz));
}

function searchTemplate(title, url, item, id) {
	var search = $("<div class=search><span class=search-title>" + title + "</span><br>"
			+ "<input type='text' class=term />"
			+ "<input type='button' class=search value='Найти' />"
			+ "<input type='button' class=cancel-search value='Отмена' />"
			+ "</div>")
			.find("input[type=button]")
				.data("search-editor", {url: url, item: item, id: id})
			.end();
	search.attr("data-depend-on", id);
	return search;
}

function cancel(id) {
	var selector = "[data-depend-on=" + id + "]";
	$(selector).each(function () {
		var element = $(this);
		if (element.is("input[type=hidden]")) {
			element.val("");
		} else if (element.is(".settings")) {
			element.css("display", "none");
		} else {
			element.remove();
		}
	});
}

function search(id) {
	cancel(id);
	var selector = "[data-depend-on=" + id + "]";
	$("input[type=hidden][data-search-url]" + selector)
		.add($("input[type=hidden][data-search-editor]" + selector))
		.each(function () {
			var element = $(this);
			element.siblings(".value").remove();
			var editor = window.searchEditors[element.attr("data-search-editor")] || {};
			var title = element.attr("data-search-title") || editor.title;
			var url = element.attr("data-search-url") || editor.url;
			element.parent().prepend(searchTemplate(title, url, element, id));
		});
}

function activateSearch() {
	window.searchEditors = {};
	window.searchEditors["searchMatrix"] = {
		title: "Выберите матрицу",
		url: "/clients/SearchMatrix"
	};
	window.searchEditors["assortmentPrice"] = {
		title: "Выберите ассортиментный прайс лист",
		url: "/clients/SearchAssortmentPrices"
	};

	var items = $("input[type=checkbox].activate");
	items.each(function () {
		if (!this.checked)
			cancel(this.id);
	});
	items.click(function () {
		if (this.checked)
			search(this.id);
		else
			cancel(this.id);
	});
}
