$ ->
	test "set is hidden for supplier if register copy for supplier", ->
		model = new RegistrationViewModel()
		model.forSupplier(true)
		equal model.isHiddenFromSupplier(), true
		equal model.isHiddenFromSupplierEnable(), false
