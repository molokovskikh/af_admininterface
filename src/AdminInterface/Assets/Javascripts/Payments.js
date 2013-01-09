function Filter(linkCaption, link, id) {
	var oldCaption = $(link).html();
	var addresses = $("#" + id + " tr.hidable").toggleClass("hidden");
	$(link).html(linkCaption);
	link.onclick = function () { Filter(oldCaption, link, id) };
}

function ShowAdditionalInfo(id, selectorAdditionalInfo, selectorInfoCell, ajaxLink, cssClass) {
	var count = $(selectorAdditionalInfo + id).length;
	if (count > 0) {
		$(selectorAdditionalInfo + id).remove();
		return;
	}
	AjaxRequest(ajaxLink,
		function (html) { $(selectorInfoCell + id).append(html); });
}

function ShowAdditionalInfoForUser(userId, cssClass) {
	ShowAdditionalInfo(userId, "#additionalUserInfo", "#additionalUserInfoCell",
		"AdditionalUserInfo.rails?userId=" + userId + "&cssClassName=" + cssClass);
}

function ShowAdditionalInfoForSupplier(supplierId, cssClass) {
	ShowAdditionalInfo(supplierId, "#additionalSupplierInfo", "#additionalSupplierInfoCell",
		"AdditionalSupplierInfo.rails?supplierId=" + supplierId + "&cssClassName=" + cssClass);
}

function ShowAdditionalInfoForAddress(addressId, cssClass) {
	ShowAdditionalInfo(addressId, "#additionalAddressInfo", "#additionalAddressInfoCell",
		"AdditionalAddressInfo.rails?addressId=" + addressId + "&cssClassName=" + cssClass);
}

function updateChildren(id, status) {
	if (status) {
		$("tr[name=Client" + id + "]").removeClass("disabled-by-parent");
		$("tr[name=Client" + id + "] input[name=status]").removeAttr("disabled");
	}
	else {
		$("tr[name=Client" + id + "]").addClass("disabled-by-parent");
		$("tr[name=Client" + id + "] input[name=status]").attr("disabled", "disabled");
	}
}

function SetClientStatus(clientId, item) {
	var link = "UpdateClientStatus?id=" + clientId + "&status=" + item.checked;
	checkBoxChanger(link, $(item));
}

function ShowJuridicalOrganization(id) {
	$(".tab").hide();
	$(".tabs ul li a.selected").removeClass("selected");
	$("#juridicalOrganization-tab").show();
	$("#juridicalOrganization").addClass("selected");

	var elements = $("div[id*='JuridicalOrganization']");
	for (var i = 0; i < elements.size() ; i++) {
		if ($("#" + elements[i].id).attr("class").indexOf("HideVisible") >= 0) {
			$("#" + elements[i].id).click();
		}
	}
	if ($("#JuridicalOrganization" + id + "Header").attr("class").indexOf("ShowHiden") >= 0)
		$("#JuridicalOrganization" + id + "Header").click();
}

function showWaiter(element) {
	element.hide();
	var waiter = $("<div></div>").attr("class", "waiter");
	element.after(waiter);
	return waiter;
}

function hideWaiter(element, waiter) {
	element.show();
	waiter.remove();
}

function Update(url, item, success) {
	var waiter = showWaiter(item);
	AjaxRequest(url,
		function (html) {
			hideWaiter(item, waiter);
			RefreshTotalSum();
			//				showSuccessMessage("Сохранено");
			processResponse(html, item);
			if (success)
				success();
		},
		function (xhr, textStatus, error) {
			hideWaiter(item, waiter);
		}
	);
}

function processResponse(data, element) {
	if (data.message)
		showSuccessMessage(data.message);
	if (data.accounts) {
		$(data.accounts).each(function (index, account) {
			idElement = $("input[name=id][value=" + account.id + "]");
			row = idElement.parents("tr").first();
			row.find("input[name=free]").attr("checked", account.free);
			if (account.free)
				row.addClass("consolidate-free");
			else
				row.removeClass("consolidate-free");
		});
	}
	if (data.data) {
		$(element).parent().append(data.data)
	}
}

function buildAjaxUrl(url, element) {
	var id = element.parents("tr").find("input[name=id]").val();
	var value = element.val();
	if (element.attr("type") == "checkbox")
		value = element.get(0).checked;

	var link = url + "?id=" + id + "&" + element.attr("name") + "=" + value;
	return link;
}

function checkBoxChanger(url, item) {
	fillDependedData(url, item, function (resultUrl) {
		Update(resultUrl, item, function () {
			if (item.attr("name") == "status") {
				var id = $(item).parents("tr").find("input[name=service_id]").val();
				if (id) {
					updateChildren(id, item.attr("checked"));
				}
				if (item.attr("checked") == false || item.attr("checked") == undefined) {
					var accounted = $(item).parents("tr").find("input[name=accounted]");
					accounted.attr("checked", false);
					accounted.change();
				}
			}
		});
	});
}

$(function () {
	var sendMessageView = new PayerSendMessage();

	$("input[name=free]").change(function () {
		var accounted = $(this).parent().parent().children().children('input[name=accounted]');
		if (this.checked) {
			$(this).parents("tr").addClass("consolidate-free");
			accounted.attr("disabled", true);
			accounted.removeAttr("checked");
		}
		else {
			$(this).parents("tr").removeClass("consolidate-free");
			accounted.attr("disabled", false);
		}
	});

	$("input[name=status]").change(function () {
		if (this.checked) {
			$(this).parents("tr").removeClass("disabled");
		}
		else {
			$(this).parents("tr").addClass("disabled");
		}
	});

	$("#addresses input, #users input, #reports input, #suppliers input").change(function () {
		var url = "../Accounts/Update";
		var item = $(this);
		url = buildAjaxUrl(url, item);
		checkBoxChanger(url, item);
	});

	$("input[name=free]").each(function () {
		if (this.checked) {
			var accounted = $(this).parent().parent().children().children('input[name=accounted]');
			accounted.attr("disabled", true);
		}
	});

	$("div.collapsible, div.autocollapsible").each(function () {
		var folder = $(this).find("div.VisibleFolder");
		var triggers = $(this).find("div.trigger");

		var count = folder.find("table tr").length;
		if (count > 0)
			count--;
		if (count == 0) {
			if (folder.find("table").length == 0) {
				count = folder.children().length;
			}
		}

		if (count > 5 && $(this).hasClass("autocollapsible")) {
			triggers.addClass("ShowHiden");
			folder.addClass("hidden")
		}
		else {
			triggers.addClass("HideVisible");
		}
	});

	$("form#MessagesForm").validate({
		rules: {
			"messageText": { "required": true, "validateForbiddenSymbols": true }
		}
	});
});