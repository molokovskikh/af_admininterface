$(function () {
	var emptyTemplate = "<thead><tr><td class='EmptyData'>Нет ни одного. <a class='new' href='javascript:'>Создать?</a></td></tr></thead>";

	function checkEmptyTable(table) {
		var body = table.find("tbody");
		var index = body.children().length;
		if (index == 0) {
			table.children().remove();
			table.append(emptyTemplate);
		}
	}

	$("table.DataTable.inline-editable").each(function () {
		checkEmptyTable($(this));
	});

	$("a.new").live('click', function () {
		var table = $(this).parents("table").first();
		var body = table.children("tbody");
		var index = 0;
		if (body.length == 0) {
			var name = table.attr("data-name");
			table.children().remove();
			table.append("<thead><tr><td><a class='new' href='javascript:'>Новый</a></td><th>" + name + "</th></thead>");
			table.append("<tbody></tbody>");
			body = table.children("tbody");
		}
		else {
			index = _.max(_.map(body.find("input"), function (element) {
				var name = element.name;
				return parseInt(name.match(/\[(\d+)\]/)[1]) || 0;
			}));
			index++;
		}

		body.append("<tr><td><a class='delete' href='javascript:'>Удалить</a></td><td><input name='items[" + index + "].Name' type='text'></input></td></tr>");
	});

	$("a.delete").live('click', function () {
		var element = $(this);
		var table = element.parents("table").first();
		element.parents("tr").first().remove();
		checkEmptyTable(table);
	});
});
