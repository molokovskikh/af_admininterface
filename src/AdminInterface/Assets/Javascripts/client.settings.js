$(function () {
	activateSearch();

	window.searchEditors["parser"] = {
		title: "Выберите парсер",
		url: "/clients/SearchParseAlgorithm"
	};

	$("#excludes").data("template", function () {
		return $("<tr>")
			.append($("<td>").append($("<input type=button value=Удалить>").addClass("delete")))
			.append($("<td>")
				.append(searchTemplate("Выберете поставщика", "/clients/" + $("#client_Id").val() + "/SearchSuppliers"))
				.append("<input type=hidden name=drugstore.OfferMatrixExcludes[0].Id>"));
	});
});

function NotifySuppliers() {
	$('#NotifySuppliers').submit();
}
