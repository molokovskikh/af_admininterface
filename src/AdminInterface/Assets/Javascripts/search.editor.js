$(function () {
	$("input[type=button].cancel-search").live("click", function () {
		cancel($(this).parents("td").first());
	});

	$("input[type=button].search").live("click", function () {
		var url = $(this).data("url");
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
				select.change(function () {
					rootRow.find("input[type=hidden]").val($(this).val());
				});
				rootRow.find("input[type=hidden]").val(select.val());
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

function searchTemplate(title, url) {
	return $("<div class=search><span class=search-title>" + title + "</span><br>"
			+ "<input type='text' class=term />"
			+ "<input type='button' class=search value='Найти' />"
			+ "<input type='button' class=cancel-search value='Отмена' />"
			+ "</div>")
			.find("input[type=button]").data("url", url).end();
}