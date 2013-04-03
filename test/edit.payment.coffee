$ ->
	test "", ->
		window.siteroot = "/AdminInterface"
		ajaxData = [{id: 1, label: "1"}, {id: 2, label: "2"}]
		$.extend
			ajax: (data) ->
				data.success(ajaxData)

		el = $("#payment_Payer_Name")
		el.val("test")
		el.autocomplete("search")
		href = $(".ui-menu-item a").attr("href")
		equal href, "/AdminInterface/Billing/Edit?billingcode=1"
