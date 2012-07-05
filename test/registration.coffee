$ ->
	test "set is hidden for supplier if register copy for supplier", ->
		model = new RegistrationViewModel()
		model.forSupplier(true)
		equal model.isHiddenFromSupplier(), true
		equal model.isHiddenFromSupplierEnable(), false

	test "update orgs after payer id change", ->
		model = new RegistrationViewModel()
		ajaxData = [{id: 1, name: "1"}, {id: 2, name: "2"}]
		$.extend
			get: (url, data, callback) ->
				callback(ajaxData)
		model.payerId(1)
		equal model.orgs().length, 2
